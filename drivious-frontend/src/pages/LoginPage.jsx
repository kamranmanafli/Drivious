import { useState } from "react";
import { Navigate, useLocation, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { errorMessage } from "../api/client";

export default function LoginPage() {
  const { login, isAuthenticated } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const [userName, setUserName] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState(null);
  const [busy, setBusy] = useState(false);

  if (isAuthenticated) return <Navigate to="/" replace />;

  async function submit(event) {
    event.preventDefault();

    setBusy(true);
    setError(null);

    try {
      await login(userName, password);

      navigate(location.state?.from?.pathname ?? "/", { replace: true });
    } catch (err) {
      setError(errorMessage(err));
    } finally {
      setBusy(false);
    }
  }

  return (
    <form className="login" onSubmit={submit}>
      <h2>Drivious</h2>

      {error && <p className="error">{error}</p>}

      <label>
        İstifadəçi adı
        <input
          value={userName}
          onChange={(event) => setUserName(event.target.value)}
          autoComplete="username"
          required
        />
      </label>

      <label>
        Şifrə
        <input
          type="password"
          value={password}
          onChange={(event) => setPassword(event.target.value)}
          autoComplete="current-password"
          required
        />
      </label>

      <button className="primary" type="submit" disabled={busy}>
        {busy ? "Gözləyin…" : "Daxil ol"}
      </button>
    </form>
  );
}
