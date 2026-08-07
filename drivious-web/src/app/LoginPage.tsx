import { useState } from "react";
import { Link, Navigate, useLocation, useNavigate } from "react-router-dom";
import { AlertCircle } from "lucide-react";
import { useAuth } from "@/auth/AuthContext";
import { errorMessage } from "@/api/client";
import { AuthShell } from "./AuthShell";
import { Button, Field, Input } from "@/ui";

export function LoginPage() {
  const { isAuthenticated, ready, login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const [userName, setUserName] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  if (ready && isAuthenticated) {
    const from = (location.state as { from?: Location } | null)?.from?.pathname ?? "/";
    return <Navigate to={from} replace />;
  }

  async function submit(event: React.FormEvent) {
    event.preventDefault();
    setError(null);
    setBusy(true);

    try {
      await login(userName.trim(), password);
      navigate("/", { replace: true });
    } catch (loginError) {
      setError(errorMessage(loginError));
    } finally {
      setBusy(false);
    }
  }

  return (
    <AuthShell>
      <h1 className="text-2xl font-semibold tracking-tight">Xoş gəlmisiniz</h1>
      <p className="mt-1.5 text-sm text-muted-foreground">
        Filonu idarə etmək üçün hesabınıza daxil olun.
      </p>

      <form onSubmit={submit} className="mt-7 space-y-4" noValidate>
        <Field label="İstifadəçi adı" required>
          {(props) => (
            <Input
              {...props}
              value={userName}
              onChange={(event) => setUserName(event.target.value)}
              autoComplete="username"
              autoFocus
              required
            />
          )}
        </Field>

        <Field label="Şifrə" required>
          {(props) => (
            <Input
              {...props}
              type="password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              autoComplete="current-password"
              required
            />
          )}
        </Field>

        {error && (
          <div
            role="alert"
            className="flex items-start gap-2 rounded-md bg-danger-muted px-3 py-2 text-xs text-danger"
          >
            <AlertCircle className="mt-px size-3.5 shrink-0" />
            {error}
          </div>
        )}

        <Button type="submit" variant="primary" size="lg" loading={busy} className="w-full">
          Daxil ol
        </Button>
      </form>

      <p className="mt-6 text-center text-sm text-muted-foreground">
        Hesabınız yoxdur?{" "}
        <Link to="/register" className="font-medium text-foreground underline-offset-4 hover:underline">
          Qeydiyyatdan keçin
        </Link>
      </p>
    </AuthShell>
  );
}
