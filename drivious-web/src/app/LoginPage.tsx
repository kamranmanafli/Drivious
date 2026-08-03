import { useState } from "react";
import { Navigate, useLocation, useNavigate } from "react-router-dom";
import { AlertCircle } from "lucide-react";
import { useAuth } from "@/auth/AuthContext";
import { errorMessage } from "@/api/client";
import { DEMO, DEMO_ACCOUNTS } from "@/api/demo/adapter";
import { roleLabels } from "@/lib/enums";
import { ThemeToggle } from "@/theme";
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

  function useDemoAccount(account: (typeof DEMO_ACCOUNTS)[number]) {
    setUserName(account.userName);
    setPassword(account.password);
    setError(null);
  }

  return (
    <div className="grid min-h-dvh lg:grid-cols-2">
      {/* Left: the form */}
      <div className="flex flex-col px-6 py-8 sm:px-10">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2 font-semibold tracking-tight">
            <span className="flex size-7 items-center justify-center rounded-md bg-primary text-primary-foreground">
              <svg viewBox="0 0 24 24" fill="none" className="size-4">
                <path
                  d="M4 16.5V13l1.6-4.4A2 2 0 0 1 7.5 7.2h9a2 2 0 0 1 1.9 1.4L20 13v3.5"
                  stroke="currentColor"
                  strokeWidth="1.8"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                />
                <circle cx="7.5" cy="16.5" r="1.6" stroke="currentColor" strokeWidth="1.8" />
                <circle cx="16.5" cy="16.5" r="1.6" stroke="currentColor" strokeWidth="1.8" />
              </svg>
            </span>
            Drivious
          </div>

          <ThemeToggle />
        </div>

        <div className="mx-auto flex w-full max-w-sm flex-1 flex-col justify-center py-10">
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

          {DEMO && (
            <div className="mt-8 rounded-lg border border-border bg-surface-raised p-3">
              <p className="text-xs font-medium">Demo hesabları</p>
              <p className="mt-0.5 text-[11px] text-muted-foreground">
                Backend qoşulu deyil — məlumatlar yaddaşdadır. Birinə toxunun.
              </p>

              <div className="mt-2.5 grid gap-1.5">
                {DEMO_ACCOUNTS.map((account) => (
                  <button
                    key={account.userName}
                    type="button"
                    onClick={() => useDemoAccount(account)}
                    className="flex items-center justify-between rounded-md border border-border bg-surface px-2.5 py-1.5 text-left text-xs hover:border-border-strong"
                  >
                    <span className="font-medium">{account.userName}</span>
                    <span className="text-muted-foreground">
                      {roleLabels[account.role] ?? account.role}
                    </span>
                  </button>
                ))}
              </div>
            </div>
          )}
        </div>
      </div>

      {/* Right: a quiet panel rather than a stock photo — it stays legible in
          both themes and does not compete with the form. */}
      <div className="relative hidden overflow-hidden border-l border-border bg-surface-sunken lg:block">
        <div
          className="absolute inset-0 opacity-[0.35]"
          style={{
            backgroundImage:
              "linear-gradient(to right, var(--border) 1px, transparent 1px), linear-gradient(to bottom, var(--border) 1px, transparent 1px)",
            backgroundSize: "44px 44px",
          }}
          aria-hidden
        />

        <div className="relative flex h-full flex-col justify-end p-12">
          <blockquote className="max-w-md">
            <p className="text-xl font-medium leading-relaxed tracking-tight">
              Maşınlar, sürücülər, xərclər və bitmə tarixləri — hamısı bir yerdə.
            </p>
            <footer className="mt-4 text-sm text-muted-foreground">
              Sığorta, vəsiqə, texniki baxış və servis tarixləri avtomatik izlənir; bitməzdən
              əvvəl sizə xəbər verilir.
            </footer>
          </blockquote>
        </div>
      </div>
    </div>
  );
}
