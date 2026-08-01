import { Navigate, useLocation } from "react-router-dom";
import { useAuth } from "./AuthContext";

/**
 * Gate for a route. `roles` narrows it further - leave it out to accept any
 * signed in user.
 *
 * This only decides what is worth rendering. The API enforces the same rules
 * again on every request, so hiding a button is a convenience, not the control.
 */
export function RequireAuth({ roles, children }) {
  const { isAuthenticated, loading, roles: mine } = useAuth();
  const location = useLocation();

  if (loading) return <p style={{ padding: 24 }}>Yüklənir…</p>;

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  if (roles && !roles.some((role) => mine.includes(role))) {
    return <Navigate to="/" replace />;
  }

  return children;
}
