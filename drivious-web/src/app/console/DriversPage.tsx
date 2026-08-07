import { useState } from "react";
import { Archive, Mail, Pencil, Phone, Plus, Trash2 } from "lucide-react";
import { drivers as driversApi, type DriverInput } from "@/api/endpoints";
import { assetUrl } from "@/api/client";
import type { Driver } from "@/api/types";
import { date, toDateInput } from "@/lib/format";
import { useAuth } from "@/auth/AuthContext";
import { useResourceList } from "@/components/useResourceList";
import { DataTable, type Column } from "@/components/DataTable";
import { PageHeader } from "@/components/PageHeader";
import { FilterControl, Toolbar } from "@/components/Toolbar";
import { RowActions } from "@/components/RowActions";
import { FormDialog, useResourceMutation } from "@/components/FormDialog";
import { ImageInput } from "@/components/ImageInput";
import { ExpiryBadge } from "./InsurancePage";
import {
  Avatar,
  Badge,
  Button,
  Confirm,
  Field,
  Input,
  MenuItem,
  MenuSeparator,
  NativeSelect,
  useConfirm,
} from "@/ui";

const FILTERS = ["isActive", "licenseExpiresBefore"] as const;

export function DriversPage() {
  const { canManage, isAdmin } = useAuth();

  const list = useResourceList<Driver>({
    key: "drivers",
    fetcher: driversApi.list,
    defaultSort: "createdAt",
    filters: FILTERS,
  });

  const [editing, setEditing] = useState<Driver | "new" | null>(null);
  const archive = useConfirm<Driver>();
  const destroy = useConfirm<Driver>();

  const toggle = useResourceMutation((row: Driver) => driversApi.toggle(row.id), {
    invalidate: ["drivers", "dashboard"],
  });

  const remove = useResourceMutation((row: Driver) => driversApi.remove(row.id), {
    invalidate: ["drivers", "dashboard"],
  });

  const columns: Array<Column<Driver>> = [
    {
      key: "firstName",
      header: "Sürücü",
      sortable: true,
      mobile: "title",
      cell: (row) => (
        <div className="flex items-center gap-2.5">
          <Avatar src={assetUrl(row.imageUrl)} name={row.fullName} className="size-8 md:size-9" />
          <div className="min-w-0">
            <p className="truncate font-medium">{row.fullName ?? `${row.firstName} ${row.lastName}`}</p>
            <p className="truncate text-xs text-muted-foreground">{row.driverLicenseNumber}</p>
          </div>
        </div>
      ),
    },
    {
      key: "phoneNumber",
      header: "Əlaqə",
      mobile: "meta",
      cell: (row) => (
        <div className="space-y-0.5">
          <a
            href={`tel:${row.phoneNumber}`}
            onClick={(event) => event.stopPropagation()}
            className="flex items-center gap-1.5 text-xs hover:text-primary"
          >
            <Phone className="size-3 text-muted-foreground" />
            {row.phoneNumber}
          </a>
          <a
            href={`mailto:${row.email}`}
            onClick={(event) => event.stopPropagation()}
            className="flex items-center gap-1.5 text-xs text-muted-foreground hover:text-primary"
          >
            <Mail className="size-3" />
            <span className="truncate">{row.email}</span>
          </a>
        </div>
      ),
    },
    {
      key: "licenseExpireDate",
      header: "Vəsiqənin bitmə tarixi",
      sortable: true,
      mobile: "meta",
      cell: (row) => <ExpiryBadge value={row.licenseExpireDate} />,
    },
    {
      key: "hireDate",
      header: "İşə qəbul",
      sortable: true,
      mobile: "meta",
      cell: (row) => <span className="tnum">{date(row.hireDate)}</span>,
    },
    {
      key: "isActive",
      header: "Status",
      sortable: true,
      mobile: "trailing",
      cell: (row) => (
        <Badge tone={row.isActive ? "success" : "neutral"} dot>
          {row.isActive ? "Aktiv" : "Deaktiv"}
        </Badge>
      ),
    },
  ];

  return (
    <>
      <PageHeader
        title="Sürücülər"
        description="Sürücü kartotekası və vəsiqə bitmə tarixləri."
        actions={
          canManage && (
            <Button variant="primary" onClick={() => setEditing("new")}>
              <Plus />
              Yeni sürücü
            </Button>
          )
        }
      />

      <Toolbar
        search={list.value("search")}
        onSearch={(value) => list.setParam("search", value)}
        searchPlaceholder="Ad, soyad, telefon və ya e-poçt…"
        activeFilterCount={list.activeFilterCount}
        onClear={list.clearFilters}
        filters={
          <>
            <FilterControl label="Status">
              <NativeSelect
                value={list.value("isActive")}
                onChange={(event) => list.setParam("isActive", event.target.value)}
              >
                <option value="">Hamısı</option>
                <option value="true">Aktiv</option>
                <option value="false">Deaktiv</option>
              </NativeSelect>
            </FilterControl>

            <FilterControl label="Vəsiqəsi ...-dək bitənlər" className="lg:w-44">
              <Input
                type="date"
                value={list.value("licenseExpiresBefore")}
                onChange={(event) => list.setParam("licenseExpiresBefore", event.target.value)}
              />
            </FilterControl>
          </>
        }
      />

      <DataTable
        columns={columns}
        result={list.result}
        isLoading={list.isLoading}
        isFetching={list.isFetching}
        error={list.error}
        onRetry={() => void list.refetch()}
        sortBy={list.sortBy}
        descending={list.descending}
        onSort={list.setSort}
        onPage={list.setPage}
        rowKey={(row) => row.id}
        emptyTitle={list.hasFilters ? "Uyğun sürücü tapılmadı" : "Hələ sürücü əlavə edilməyib"}
        emptyDescription={list.hasFilters ? "Filtrləri dəyişib yenidən cəhd edin." : undefined}
        emptyAction={
          canManage && !list.hasFilters ? (
            <Button variant="primary" onClick={() => setEditing("new")}>
              <Plus />
              Yeni sürücü
            </Button>
          ) : undefined
        }
        actions={
          canManage
            ? (row) => (
                <RowActions>
                  <MenuItem onSelect={() => setEditing(row)}>
                    <Pencil />
                    Redaktə et
                  </MenuItem>

                  <MenuSeparator />

                  <MenuItem onSelect={() => archive.ask(row)}>
                    <Archive />
                    Arxivə göndər
                  </MenuItem>

                  {isAdmin && (
                    <MenuItem danger onSelect={() => destroy.ask(row)}>
                      <Trash2 />
                      Həmişəlik sil
                    </MenuItem>
                  )}
                </RowActions>
              )
            : undefined
        }
      />

      {editing && (
        <DriverFormDialog
          driver={editing === "new" ? null : editing}
          onClose={() => setEditing(null)}
        />
      )}

      <Confirm
        open={archive.open}
        onOpenChange={archive.onOpenChange}
        title="Sürücünü arxivə göndər"
        description={
          `${archive.target?.fullName} arxivə göndəriləcək. ` +
          "Onun gəlir qeydləri və maşın təyinatları da birlikdə arxivlənir. " +
          "Arxivdən geri qaytara bilərsiniz."
        }
        confirmLabel="Arxivə göndər"
        onConfirm={() => toggle.mutateAsync(archive.target!)}
      />

      <Confirm
        open={destroy.open}
        onOpenChange={destroy.onOpenChange}
        title="Həmişəlik sil"
        description={
          `${destroy.target?.fullName} bazadan tamamilə silinəcək və geri qaytarıla bilməz. ` +
          "Sürücünün gəlir və ya təyinat qeydləri varsa, server bu əməliyyatı rədd edəcək."
        }
        confirmLabel="Həmişəlik sil"
        danger
        onConfirm={() => remove.mutateAsync(destroy.target!)}
      />
    </>
  );
}

/* ── Form ────────────────────────────────────────────────────────────────── */

// Matches the API's validator: +994 followed by nine digits, or 0 and nine more.
const PHONE = /^\+994\d{9}$|^0\d{9}$/;

function DriverFormDialog({ driver, onClose }: { driver: Driver | null; onClose: () => void }) {
  const isNew = driver === null;

  const [values, setValues] = useState({
    firstName: driver?.firstName ?? "",
    lastName: driver?.lastName ?? "",
    phoneNumber: driver?.phoneNumber ?? "",
    email: driver?.email ?? "",
    identityNumber: driver?.identityNumber ?? "",
    driverLicenseNumber: driver?.driverLicenseNumber ?? "",
    licenseExpireDate: toDateInput(driver?.licenseExpireDate),
    birthDate: toDateInput(driver?.birthDate),
    hireDate: toDateInput(driver?.hireDate ?? new Date()),
    address: driver?.address ?? "",
    isActive: String(driver?.isActive ?? true),
  });

  const [image, setImage] = useState<File | null>(null);
  const [errors, setErrors] = useState<Partial<Record<keyof typeof values | "image", string>>>({});

  const save = useResourceMutation(
    (payload: DriverInput | Partial<DriverInput>) =>
      isNew ? driversApi.create(payload as DriverInput) : driversApi.update(driver.id, payload),
    { invalidate: ["drivers", "notifications", "dashboard"], onSuccess: onClose },
  );

  const set =
    (key: keyof typeof values) => (event: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) =>
      setValues((previous) => ({ ...previous, [key]: event.target.value }));

  function submit() {
    const next: typeof errors = {};
    const today = toDateInput(new Date());

    if (values.firstName.trim().length < 2) next.firstName = "Ən azı 2 simvol.";
    if (values.lastName.trim().length < 2) next.lastName = "Ən azı 2 simvol.";
    if (!PHONE.test(values.phoneNumber.trim())) {
      next.phoneNumber = "Format: +994501234567 və ya 0501234567";
    }
    if (!/^\S+@\S+\.\S+$/.test(values.email.trim())) next.email = "E-poçt düzgün deyil.";
    if (!values.identityNumber.trim()) next.identityNumber = "FIN tələb olunur.";
    if (!values.driverLicenseNumber.trim()) next.driverLicenseNumber = "Vəsiqə nömrəsi tələb olunur.";

    // The API requires a licence date strictly in the future.
    if (!values.licenseExpireDate) next.licenseExpireDate = "Tarix tələb olunur.";
    else if (values.licenseExpireDate <= today) next.licenseExpireDate = "Gələcək tarix olmalıdır.";

    if (!values.birthDate) next.birthDate = "Tarix tələb olunur.";
    else if (values.birthDate >= today) next.birthDate = "Keçmiş tarix olmalıdır.";

    if (!values.hireDate) next.hireDate = "Tarix tələb olunur.";
    else if (values.hireDate > today) next.hireDate = "Gələcək tarix ola bilməz.";

    // Address is nullable on the model but the validator makes it required.
    if (values.address.trim().length < 5) next.address = "Ən azı 5 simvol.";

    if (isNew && !image) next.image = "Sürücü şəkli tələb olunur.";

    setErrors(next);
    if (Object.keys(next).length > 0) return;

    save.mutate({
      firstName: values.firstName.trim(),
      lastName: values.lastName.trim(),
      phoneNumber: values.phoneNumber.trim(),
      email: values.email.trim(),
      identityNumber: values.identityNumber.trim(),
      driverLicenseNumber: values.driverLicenseNumber.trim(),
      licenseExpireDate: new Date(values.licenseExpireDate).toISOString(),
      birthDate: new Date(values.birthDate).toISOString(),
      hireDate: new Date(values.hireDate).toISOString(),
      address: values.address.trim(),
      isActive: values.isActive === "true",
      ...(image ? { image } : {}),
    });
  }

  return (
    <FormDialog
      open
      onOpenChange={(open) => !open && onClose()}
      title={isNew ? "Yeni sürücü" : "Sürücünü redaktə et"}
      description={isNew ? undefined : driver.fullName ?? undefined}
      submitting={save.isPending}
      onSubmit={submit}
    >
      <ImageInput
        label="Şəkil"
        required={isNew}
        error={errors.image}
        currentUrl={assetUrl(driver?.imageUrl)}
        value={image}
        onChange={setImage}
        maxMb={5}
        className="sm:col-span-2"
      />

      <Field label="Ad" required error={errors.firstName}>
        {(props) => <Input {...props} value={values.firstName} onChange={set("firstName")} />}
      </Field>

      <Field label="Soyad" required error={errors.lastName}>
        {(props) => <Input {...props} value={values.lastName} onChange={set("lastName")} />}
      </Field>

      <Field label="Telefon" required error={errors.phoneNumber}>
        {(props) => (
          <Input
            {...props}
            type="tel"
            value={values.phoneNumber}
            onChange={set("phoneNumber")}
            placeholder="+994501234567"
          />
        )}
      </Field>

      <Field label="E-poçt" required error={errors.email}>
        {(props) => (
          <Input
            {...props}
            type="email"
            value={values.email}
            onChange={set("email")}
            placeholder="ad.soyad@nümunə.az"
          />
        )}
      </Field>

      <Field label="Ş/V nömrəsi (FIN)" required error={errors.identityNumber}>
        {(props) => (
          <Input
            {...props}
            value={values.identityNumber}
            onChange={set("identityNumber")}
            className="font-mono"
          />
        )}
      </Field>

      <Field label="Vəsiqə nömrəsi" required error={errors.driverLicenseNumber}>
        {(props) => (
          <Input
            {...props}
            value={values.driverLicenseNumber}
            onChange={set("driverLicenseNumber")}
            className="font-mono"
          />
        )}
      </Field>

      <Field
        label="Vəsiqənin bitmə tarixi"
        required
        error={errors.licenseExpireDate}
        hint={errors.licenseExpireDate ? undefined : "Bitməyə 30 gün qalanda xəbərdarlıq yaranır"}
      >
        {(props) => (
          <Input
            {...props}
            type="date"
            value={values.licenseExpireDate}
            onChange={set("licenseExpireDate")}
          />
        )}
      </Field>

      <Field label="Doğum tarixi" required error={errors.birthDate}>
        {(props) => (
          <Input
            {...props}
            type="date"
            max={toDateInput(new Date())}
            value={values.birthDate}
            onChange={set("birthDate")}
          />
        )}
      </Field>

      <Field label="İşə qəbul tarixi" required error={errors.hireDate}>
        {(props) => (
          <Input
            {...props}
            type="date"
            max={toDateInput(new Date())}
            value={values.hireDate}
            onChange={set("hireDate")}
          />
        )}
      </Field>

      <Field label="Status" required>
        {(props) => (
          <NativeSelect {...props} value={values.isActive} onChange={set("isActive")}>
            <option value="true">Aktiv</option>
            <option value="false">Deaktiv</option>
          </NativeSelect>
        )}
      </Field>

      <Field label="Ünvan" required error={errors.address} className="sm:col-span-2">
        {(props) => (
          <Input
            {...props}
            value={values.address}
            onChange={set("address")}
            placeholder="Bakı, Nəsimi r., 12-ci ev"
          />
        )}
      </Field>
    </FormDialog>
  );
}
