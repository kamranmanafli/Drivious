import { NavLink, Navigate, Route, Routes } from "react-router-dom";
import { useAuth } from "./auth/AuthContext";
import { RequireAuth } from "./auth/RequireAuth";
import { ROLES } from "./constants/enums";
import LoginPage from "./pages/LoginPage";
import VehiclesPage from "./pages/VehiclesPage";

function Layout({ children }) {
  const { user, roles, canManage, logout } = useAuth();

  return (
    <div className="layout">
      <aside className="sidebar">
        <h1>Drivious</h1>

        <nav>
          <NavLink to="/">İdarə paneli</NavLink>
          <NavLink to="/vehicles">Maşınlar</NavLink>
          {/* Add the rest here as you build them. Hiding a link the role cannot
              use keeps the user away from a guaranteed 403. */}
          {canManage && <NavLink to="/drivers">Sürücülər</NavLink>}
        </nav>

        <footer>
          {user?.userName}
          <br />
          {roles.join(", ")}
          <br />
          <button className="primary" style={{ marginTop: 10 }} onClick={logout}>
            Çıxış
          </button>
        </footer>
      </aside>

      <main className="content">{children}</main>
    </div>
  );
}

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />

      <Route
        path="/"
        element={
          <RequireAuth>
            <Layout>
              <h2>İdarə paneli</h2>
              <p>Buranı /api/dashboards ilə doldur.</p>
            </Layout>
          </RequireAuth>
        }
      />

      <Route
        path="/vehicles"
        element={
          <RequireAuth>
            <Layout>
              <VehiclesPage />
            </Layout>
          </RequireAuth>
        }
      />

      {/* Manager and Admin only - the API refuses this data for a Driver anyway. */}
      <Route
        path="/drivers"
        element={
          <RequireAuth roles={[ROLES.Admin, ROLES.Manager]}>
            <Layout>
              <h2>Sürücülər</h2>
              <p>VehiclesPage.jsx-i kopyala, endpoint və sütunları dəyiş.</p>
            </Layout>
          </RequireAuth>
        }
      />

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
