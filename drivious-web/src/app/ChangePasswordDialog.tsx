import { useState } from "react";
import { toast } from "sonner";
import { auth } from "@/api/endpoints";
import { errorMessage } from "@/api/client";
import { Field, Input } from "@/ui";
import { FormDialog } from "@/components/FormDialog";

const EMPTY = { currentPassword: "", newPassword: "", confirmNewPassword: "" };

export function ChangePasswordDialog({
  open,
  onOpenChange,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}) {
  const [values, setValues] = useState(EMPTY);
  const [errors, setErrors] = useState<Partial<typeof EMPTY>>({});
  const [busy, setBusy] = useState(false);

  const set = (key: keyof typeof EMPTY) => (event: React.ChangeEvent<HTMLInputElement>) =>
    setValues((previous) => ({ ...previous, [key]: event.target.value }));

  function close() {
    setValues(EMPTY);
    setErrors({});
    onOpenChange(false);
  }

  async function submit() {
    // Mirrors the API's own rules so the obvious mistakes are caught before a
    // round trip; the server remains the authority on the rest.
    const next: Partial<typeof EMPTY> = {};

    if (!values.currentPassword) next.currentPassword = "Cari şifrə tələb olunur.";
    if (values.newPassword.length < 6) next.newPassword = "Ən azı 6 simvol olmalıdır.";
    else if (values.newPassword === values.currentPassword)
      next.newPassword = "Yeni şifrə cari şifrədən fərqli olmalıdır.";
    if (values.confirmNewPassword !== values.newPassword)
      next.confirmNewPassword = "Şifrələr uyğun gəlmir.";

    setErrors(next);
    if (Object.keys(next).length > 0) return;

    setBusy(true);
    try {
      const message = await auth.changePassword(values);
      toast.success(message);
      // The API revokes every refresh token on a password change, so the next
      // 401 will bounce this session to the login screen — say so now.
      toast.info("Digər cihazlardakı sessiyalar bağlandı.");
      close();
    } catch (error) {
      toast.error(errorMessage(error));
    } finally {
      setBusy(false);
    }
  }

  return (
    <FormDialog
      open={open}
      onOpenChange={(next) => (next ? onOpenChange(true) : close())}
      title="Şifrəni dəyiş"
      description="Şifrə dəyişdikdən sonra bütün açıq sessiyalar bağlanır."
      size="sm"
      submitLabel="Dəyiş"
      submitting={busy}
      onSubmit={submit}
    >
      <Field label="Cari şifrə" required error={errors.currentPassword} className="sm:col-span-2">
        {(props) => (
          <Input
            {...props}
            type="password"
            autoComplete="current-password"
            value={values.currentPassword}
            onChange={set("currentPassword")}
          />
        )}
      </Field>

      <Field
        label="Yeni şifrə"
        required
        error={errors.newPassword}
        hint="Ən azı 6 simvol, böyük və kiçik hərf, rəqəm və xüsusi simvol."
        className="sm:col-span-2"
      >
        {(props) => (
          <Input
            {...props}
            type="password"
            autoComplete="new-password"
            value={values.newPassword}
            onChange={set("newPassword")}
          />
        )}
      </Field>

      <Field
        label="Yeni şifrənin təkrarı"
        required
        error={errors.confirmNewPassword}
        className="sm:col-span-2"
      >
        {(props) => (
          <Input
            {...props}
            type="password"
            autoComplete="new-password"
            value={values.confirmNewPassword}
            onChange={set("confirmNewPassword")}
          />
        )}
      </Field>
    </FormDialog>
  );
}
