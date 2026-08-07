import { useState } from "react";
import { Link, Navigate, useNavigate } from "react-router-dom";
import { AlertCircle } from "lucide-react";
import { useAuth } from "@/auth/AuthContext";
import { errorMessage } from "@/api/client";
import { auth as authApi } from "@/api/endpoints";
import { Role } from "@/api/types";
import { roleLabels } from "@/lib/enums";
import { AuthShell } from "./AuthShell";
import { Button, Field, Input, NativeSelect } from "@/ui";

type FieldName = "userName" | "email" | "password" | "confirmPassword" | "inviteCode";

/**
 * These mirror RegisterDTOValidator on the API. Checking here as well is not
 * about trusting the client — the server still rejects bad input — it is so the
 * four password rules are pointed out at the field that broke them instead of
 * arriving as one sentence after a round trip.
 */
function validate(
  values: Record<FieldName, string>,
  role: Role,
): Partial<Record<FieldName, string>> {
  const errors: Partial<Record<FieldName, string>> = {};

  // The API is the one that decides; this only spares a round trip for the
  // obvious case of an empty box.
  if (role !== Role.Driver && !values.inviteCode.trim()) {
    errors.inviteCode = "Bu rol üçün dəvət kodu tələb olunur.";
  }

  const userName = values.userName.trim();

  if (!userName) {
    errors.userName = "İstifadəçi adı tələb olunur.";
  } else if (userName.length < 3) {
    errors.userName = "İstifadəçi adı ən azı 3 simvol olmalıdır.";
  } else if (userName.length > 50) {
    errors.userName = "İstifadəçi adı 50 simvoldan uzun ola bilməz.";
  } else if (!/^[a-zA-Z0-9._@+-]+$/.test(userName)) {
    errors.userName = "Yalnız latın hərfləri, rəqəmlər və . _ @ + - işarələri olar.";
  }

  const email = values.email.trim();

  if (!email) {
    errors.email = "E-poçt tələb olunur.";
  } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
    errors.email = "E-poçt ünvanı düzgün deyil.";
  } else if (email.length > 100) {
    errors.email = "E-poçt 100 simvoldan uzun ola bilməz.";
  }

  const { password, confirmPassword } = values;

  if (!password) {
    errors.password = "Şifrə tələb olunur.";
  } else if (password.length < 6) {
    errors.password = "Şifrə ən azı 6 simvol olmalıdır.";
  } else if (!/[A-Z]/.test(password)) {
    errors.password = "Şifrədə ən azı bir böyük hərf olmalıdır.";
  } else if (!/[a-z]/.test(password)) {
    errors.password = "Şifrədə ən azı bir kiçik hərf olmalıdır.";
  } else if (!/[0-9]/.test(password)) {
    errors.password = "Şifrədə ən azı bir rəqəm olmalıdır.";
  } else if (!/[^a-zA-Z0-9]/.test(password)) {
    errors.password = "Şifrədə ən azı bir simvol olmalıdır (məsələn ! ? * #).";
  }

  if (!confirmPassword) {
    errors.confirmPassword = "Şifrənin təkrarı tələb olunur.";
  } else if (confirmPassword !== password) {
    errors.confirmPassword = "Şifrələr uyğun gəlmir.";
  }

  return errors;
}

export function RegisterPage() {
  const { isAuthenticated, ready, login } = useAuth();
  const navigate = useNavigate();

  const [values, setValues] = useState<Record<FieldName, string>>({
    userName: "",
    email: "",
    password: "",
    confirmPassword: "",
    inviteCode: "",
  });
  const [role, setRole] = useState<Role>(Role.Driver);
  const [errors, setErrors] = useState<Partial<Record<FieldName, string>>>({});
  const [error, setError] = useState<string | null>(null);
  const [busy, setBusy] = useState(false);

  if (ready && isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  function set(field: FieldName, value: string) {
    setValues((current) => ({ ...current, [field]: value }));
    // Clearing as they type keeps a message from sitting under a field the
    // person is already fixing.
    setErrors((current) => ({ ...current, [field]: undefined }));
  }

  async function submit(event: React.FormEvent) {
    event.preventDefault();
    setError(null);

    const found = validate(values, role);

    if (Object.keys(found).length > 0) {
      setErrors(found);
      return;
    }

    setBusy(true);

    try {
      await authApi.register({
        userName: values.userName.trim(),
        email: values.email.trim(),
        password: values.password,
        confirmPassword: values.confirmPassword,
        role,
        inviteCode: role === Role.Driver ? undefined : values.inviteCode.trim(),
      });

      // The API returns no tokens from register, so sign in with what was just
      // typed rather than making them enter it a second time.
      try {
        await login(values.userName.trim(), values.password);
        navigate("/", { replace: true });
      } catch {
        // The account exists either way; only the automatic sign-in failed.
        navigate("/login", { replace: true, state: { registered: true } });
      }
    } catch (registerError) {
      setError(errorMessage(registerError));
      setBusy(false);
    }
  }

  return (
    <AuthShell>
      <h1 className="text-2xl font-semibold tracking-tight">Hesab yaradın</h1>
      <p className="mt-1.5 text-sm text-muted-foreground">
        Sürücü hesabı sərbəst açılır. Administrator və Menecer üçün dəvət kodu lazımdır.
      </p>

      <form onSubmit={submit} className="mt-7 space-y-4" noValidate>
        <Field label="İstifadəçi adı" required error={errors.userName}>
          {(props) => (
            <Input
              {...props}
              value={values.userName}
              onChange={(event) => set("userName", event.target.value)}
              autoComplete="username"
              autoFocus
              required
            />
          )}
        </Field>

        <Field label="E-poçt" required error={errors.email}>
          {(props) => (
            <Input
              {...props}
              type="email"
              value={values.email}
              onChange={(event) => set("email", event.target.value)}
              autoComplete="email"
              required
            />
          )}
        </Field>

        <Field
          label="Şifrə"
          required
          error={errors.password}
          hint="Ən azı 6 simvol: böyük və kiçik hərf, rəqəm və bir işarə."
        >
          {(props) => (
            <Input
              {...props}
              type="password"
              value={values.password}
              onChange={(event) => set("password", event.target.value)}
              autoComplete="new-password"
              required
            />
          )}
        </Field>

        <Field label="Şifrənin təkrarı" required error={errors.confirmPassword}>
          {(props) => (
            <Input
              {...props}
              type="password"
              value={values.confirmPassword}
              onChange={(event) => set("confirmPassword", event.target.value)}
              autoComplete="new-password"
              required
            />
          )}
        </Field>

        <Field label="Rol" required>
          {(props) => (
            <NativeSelect
              {...props}
              value={role}
              onChange={(event) => {
                setRole(event.target.value as Role);
                setErrors((current) => ({ ...current, inviteCode: undefined }));
              }}
            >
              {Object.values(Role).map((value) => (
                <option key={value} value={value}>
                  {roleLabels[value] ?? value}
                </option>
              ))}
            </NativeSelect>
          )}
        </Field>

        {role !== Role.Driver && (
          <Field
            label="Dəvət kodu"
            required
            error={errors.inviteCode}
            hint="Administrator və Menecer hesabları yalnız kodla açılır."
          >
            {(props) => (
              <Input
                {...props}
                value={values.inviteCode}
                onChange={(event) => set("inviteCode", event.target.value)}
                autoComplete="off"
                required
              />
            )}
          </Field>
        )}

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
          Qeydiyyatdan keç
        </Button>
      </form>

      <p className="mt-6 text-center text-sm text-muted-foreground">
        Artıq hesabınız var?{" "}
        <Link to="/login" className="font-medium text-foreground underline-offset-4 hover:underline">
          Daxil olun
        </Link>
      </p>
    </AuthShell>
  );
}
