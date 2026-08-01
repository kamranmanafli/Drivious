import { createContext, useContext, useEffect, useMemo, useState } from "react";
import { auth } from "../api/endpoints";
import { setSessionLostHandler, tokenStore } from "../api/client";
import { ROLES } from "../constants/enums";

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // The interceptor cannot navigate on its own; this is how a dead session
    // reaches React state.
    setSessionLostHandler(() => setUser(null));

    if (!tokenStore.access) {
      setLoading(false);
      return;
    }

    // A stored token may be expired or issued before a role change, so the
    // server is the authority on who this is.
    auth
      .me()
      .then(setUser)
      .catch(() => {
        tokenStore.clear();
        setUser(null);
      })
      .finally(() => setLoading(false));
  }, []);

  const value = useMemo(() => {
    const roles = user?.roles ?? [];

    return {
      user,
      loading,
      roles,
      isAuthenticated: Boolean(user),

      isAdmin: roles.includes(ROLES.Admin),

      /** Allowed to create, update and archive fleet data. */
      canManage: roles.includes(ROLES.Admin) || roles.includes(ROLES.Manager),

      async login(userName, password) {
        const data = await auth.login(userName, password);

        tokenStore.save(data.accessToken, data.refreshToken);

        setUser(await auth.me());
      },

      async logout() {
        // Best effort: the session ends locally whether or not the server
        // could be told about it.
        try {
          await auth.logout(tokenStore.refresh);
        } catch {
          /* ignored */
        }

        tokenStore.clear();
        setUser(null);
      },
    };
  }, [user, loading]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error("useAuth must be used inside an AuthProvider.");
  }

  return context;
}
