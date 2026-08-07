import { Navigate, Outlet, useLocation } from "react-router-dom";
import { ShieldX } from "lucide-react";
import { useAuth } from "./AuthContext";
import { Button, EmptyState, Spinner } from "@/ui";
import type { Role } from "@/api/types";

function Loading() {
  return (
    <div className="flex min-h-dvh items-center justify-center">
      <Spinner className="size-6" />
    </div>
  );
}

/** Blocks anonymous access and remembers where the user was heading. */
export function RequireAuth() {
  const { ready, isAuthenticated } = useAuth();
  const location = useLocation();

  if (!ready) return <Loading />;

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  return <Outlet />;
}

/**
 * Hides a route the caller's role cannot use. The API enforces this too — this
 * only saves the user from a screen that would answer 403 on every request.
 */
export function RequireRole({ allow }: { allow: Role[] }) {
  const { ready, isAuthenticated, roles } = useAuth();

  if (!ready) return <Loading />;
  if (!isAuthenticated) return <Navigate to="/login" replace />;

  if (!roles.some((role) => allow.includes(role))) {
    return (
      <div className="rounded-lg border border-border bg-surface">
        <EmptyState
          icon={<ShieldX />}
          title="Bu səhifəyə icazəniz yoxdur"
          description="Bu bölmə yalnız müəyyən rollar üçün açıqdır. Səhv olduğunu düşünürsünüzsə, administratorla əlaqə saxlayın."
          action={
            <Button asChild variant="secondary">
              <a href="/">Ana səhifəyə qayıt</a>
            </Button>
          }
        />
      </div>
    );
  }

  return <Outlet />;
}
