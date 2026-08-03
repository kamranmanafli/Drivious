import { createContext, useCallback, useContext, useEffect, useMemo, useState } from "react";
import { useQueryClient } from "@tanstack/react-query";
import { setSessionLostHandler, tokenStore } from "@/api/client";
import { auth as authApi } from "@/api/endpoints";
import { Role, type CurrentUser } from "@/api/types";

interface AuthValue {
  user: CurrentUser | null;
  roles: Role[];
  /** False only while the stored token is being checked on first load. */
  ready: boolean;
  isAuthenticated: boolean;
  /** Admin or Manager — may change fleet data. */
  canManage: boolean;
  isAdmin: boolean;
  /** Holds the Driver role and nothing else; the API narrows their data. */
  isDriverOnly: boolean;
  login: (userName: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
  refreshUser: () => Promise<void>;
}

const AuthContext = createContext<AuthValue | null>(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<CurrentUser | null>(null);
  const [ready, setReady] = useState(false);
  const queryClient = useQueryClient();

  const clear = useCallback(() => {
    tokenStore.clear();
    setUser(null);
    queryClient.clear();
  }, [queryClient]);

  // The axios interceptor calls this when a refresh fails, which is the only
  // place that knows the session is truly gone rather than momentarily stale.
  useEffect(() => {
    setSessionLostHandler(clear);
  }, [clear]);

  const loadUser = useCallback(async () => {
    const me = await authApi.me();
    setUser(me);
  }, []);

  useEffect(() => {
    let cancelled = false;

    async function restore() {
      if (!tokenStore.access) {
        setReady(true);
        return;
      }

      try {
        const me = await authApi.me();
        if (!cancelled) setUser(me);
      } catch {
        // A stored token that no longer works is the same as no token. The
        // interceptor has already tried to refresh it by the time we get here.
        if (!cancelled) tokenStore.clear();
      } finally {
        if (!cancelled) setReady(true);
      }
    }

    void restore();

    return () => {
      cancelled = true;
    };
  }, []);

  const login = useCallback(
    async (userName: string, password: string) => {
      const session = await authApi.login(userName, password);
      tokenStore.save(session.accessToken, session.refreshToken);
      await loadUser();
    },
    [loadUser],
  );

  const logout = useCallback(async () => {
    const refreshToken = tokenStore.refresh;

    // Retiring the refresh token server-side is best effort — the local session
    // has to end either way, so a failed call must not strand the user.
    if (refreshToken) {
      try {
        await authApi.logout(refreshToken);
      } catch {
        /* ignored on purpose */
      }
    }

    clear();
  }, [clear]);

  const value = useMemo<AuthValue>(() => {
    const roles = user?.roles ?? [];

    return {
      user,
      roles,
      ready,
      isAuthenticated: Boolean(user),
      canManage: roles.includes(Role.Admin) || roles.includes(Role.Manager),
      isAdmin: roles.includes(Role.Admin),
      isDriverOnly:
        roles.includes(Role.Driver) &&
        !roles.includes(Role.Admin) &&
        !roles.includes(Role.Manager),
      login,
      logout,
      refreshUser: loadUser,
    };
  }, [user, ready, login, logout, loadUser]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthValue {
  const value = useContext(AuthContext);
  if (!value) throw new Error("useAuth must be used inside <AuthProvider>.");
  return value;
}
