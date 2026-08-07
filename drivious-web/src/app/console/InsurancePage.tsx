import { useState } from "react";
import { Archive, Pencil, Plus, Trash2 } from "lucide-react";
import { insurances as insuranceApi, type InsuranceInput } from "@/api/endpoints";
import type { Insurance } from "@/api/types";
import { date, daysUntil, money, relativeDays, toDateInput } from "@/lib/format";
import { useAuth } from "@/auth/AuthContext";
import { useResourceList } from "@/components/useResourceList";
import { DataTable, type Column } from "@/components/DataTable";
import { PageHeader } from "@/components/PageHeader";
import { FilterControl, Toolbar } from "@/components/Toolbar";
import { RowActions } from "@/components/RowActions";
import { FormDialog, useResourceMutation } from "@/components/FormDialog";
import { VehiclePicker } from "@/components/pickers";
import { Badge, Button, Confirm, Field, Input, MenuItem, MenuSeparator, useConfirm } from "@/ui";

const FILTERS = ["vehicleId", "expiresBefore", "activeOn"] as const;

/** Shared with the vehicle detail page. */
export function ExpiryBadge({ value }: { value?: string | null }) {
  if (!value) return <span className="text-muted-foreground">—</span>;

  const days = daysUntil(value);
  const tone = days === null ? "neutral" : days < 0 ? "danger" : days <= 30 ? "warning" : "success";

  return (
    <span className="inline-flex items-center gap-2">
      <span className="tnum">{date(value)}</span>
      <Badge tone={tone}>{days !== null && days < 0 ? "bitib" : relativeDays(value)}</Badge>
    </span>
  );
}

export function InsurancePage() {
  const { canManage, isAdmin } = useAuth();

  const list = useResourceList<Insurance>({
    key: "insurances",
    fetcher: insuranceApi.list,
    defaultSort: "endDate",
    defaultDescending: false,
    filters: FILTERS,
  });

  const [editing, setEditing] = useState<Insurance | "new" | null>(null);
  const archive = useConfirm<Insurance>();
  const destroy = useConfirm<Insurance>();

  const toggle = useResourceMutation((row: Insurance) => insuranceApi.toggle(row.id), {
    invalidate: ["insurances", "dashboard"],
  });

  const remove = useResourceMutation((row: Insurance) => insuranceApi.remove(row.id), {
    invalidate: ["insurances", "dashboard"],
  });

  const columns: Array<Column<Insurance>> = [
    {
      key: "plateNumber",
      header: "Maşın",
      sortable: true,
      mobile: "title",
      cell: (row) => (
        <div className="min-w-0">
          <p className="truncate font-medium">{row.vehiclePlateNumber}</p>
          <p className="truncate text-xs text-muted-foreground">{row.vehicleName}</p>
        </div>
      ),
    },
    {
      key: "companyName",
      header: "Şirkət",
      sortable: true,
      mobile: "subtitle",
      cell: (row) => row.companyName,
    },
    {
      key: "policyNumber",
      header: "Polis nömrəsi",
      sortable: true,
      mobile: "meta",
      cell: (row) => <span className="font-mono text-xs">{row.policyNumber}</span>,
    },
    {
      key: "startDate",
      header: "Başlama",
      sortable: true,
      mobile: "meta",
      cell: (row) => <span className="tnum">{date(row.startDate)}</span>,
    },
    {
      key: "endDate",
      header: "Bitmə",
      sortable: true,
      mobile: "meta",
      cell: (row) => <ExpiryBadge value={row.endDate} />,
    },
    {
      key: "price",
      header: "Qiymət",
      sortable: true,
      align: "right",
      mobile: "trailing",
      cell: (row) => <span className="font-medium tnum">{money(row.price)}</span>,
    },
  ];

  return (
    <>
      <PageHeader
        title="Sığorta"
        description="Polislər və bitmə tarixləri. Ən yaxın bitən əvvəldə."
        actions={
          canManage && (
            <Button variant="primary" onClick={() => setEditing("new")}>
              <Plus />
              Yeni polis
            </Button>
          )
        }
      />

      <Toolbar
        search={list.value("search")}
        onSearch={(value) => list.setParam("search", value)}
        searchPlaceholder="Şirkət, polis nömrəsi və ya nişan…"
        activeFilterCount={list.activeFilterCount}
        onClear={list.clearFilters}
        filters={
          <>
            <FilterControl label="Maşın" className="lg:w-52">
              <VehiclePicker
                placeholder="Bütün maşınlar"
                value={list.value("vehicleId")}
                onChange={(event) => list.setParam("vehicleId", event.target.value)}
              />
            </FilterControl>

            <FilterControl label="...-dək bitənlər" className="lg:w-44">
              <Input
                type="date"
                value={list.value("expiresBefore")}
                onChange={(event) => list.setParam("expiresBefore", event.target.value)}
              />
            </FilterControl>

            <FilterControl label="Bu tarixdə qüvvədə" className="lg:w-44">
              <Input
                type="date"
                value={list.value("activeOn")}
                onChange={(event) => list.setParam("activeOn", event.target.value)}
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
        emptyTitle={list.hasFilters ? "Uyğun polis tapılmadı" : "Hələ sığorta qeydi yoxdur"}
        emptyDescription={list.hasFilters ? "Filtrləri dəyişib yenidən cəhd edin." : undefined}
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
        <InsuranceFormDialog
          policy={editing === "new" ? null : editing}
          onClose={() => setEditing(null)}
        />
      )}

      <Confirm
        open={archive.open}
        onOpenChange={archive.onOpenChange}
        title="Polisi arxivə göndər"
        description={`${archive.target?.policyNumber} nömrəli polis arxivə göndəriləcək. Arxivdən geri qaytara bilərsiniz.`}
        confirmLabel="Arxivə göndər"
        onConfirm={() => toggle.mutateAsync(archive.target!)}
      />

      <Confirm
        open={destroy.open}
        onOpenChange={destroy.onOpenChange}
        title="Həmişəlik sil"
        description="Bu sığorta qeydi bazadan tamamilə silinəcək və geri qaytarıla bilməz."
        confirmLabel="Həmişəlik sil"
        danger
        onConfirm={() => remove.mutateAsync(destroy.target!)}
      />
    </>
  );
}

function InsuranceFormDialog({
  policy,
  onClose,
}: {
  policy: Insurance | null;
  onClose: () => void;
}) {
  const isNew = policy === null;

  const [values, setValues] = useState({
    vehicleId: policy?.vehicleId ?? "",
    companyName: policy?.companyName ?? "",
    policyNumber: policy?.policyNumber ?? "",
    startDate: toDateInput(policy?.startDate ?? new Date()),
    endDate: toDateInput(policy?.endDate),
    price: policy ? String(policy.price) : "",
  });

  const [errors, setErrors] = useState<Partial<Record<keyof typeof values, string>>>({});

  const save = useResourceMutation(
    (payload: InsuranceInput | Partial<InsuranceInput>) =>
      isNew
        ? insuranceApi.create(payload as InsuranceInput)
        : insuranceApi.update(policy.id, payload),
    { invalidate: ["insurances", "notifications", "dashboard"], onSuccess: onClose },
  );

  const set =
    (key: keyof typeof values) => (event: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) =>
      setValues((previous) => ({ ...previous, [key]: event.target.value }));

  function submit() {
    const next: typeof errors = {};

    if (!values.vehicleId) next.vehicleId = "Maşın seçilməlidir.";
    if (values.companyName.trim().length < 2) next.companyName = "Ən azı 2 simvol.";
    if (values.policyNumber.trim().length < 3) next.policyNumber = "Ən azı 3 simvol.";
    if (!values.endDate) next.endDate = "Bitmə tarixi tələb olunur.";
    else if (values.endDate <= values.startDate) {
      next.endDate = "Başlama tarixindən sonra olmalıdır.";
    }
    if (!(Number(values.price) > 0)) next.price = "Qiymət 0-dan böyük olmalıdır.";

    setErrors(next);
    if (Object.keys(next).length > 0) return;

    save.mutate({
      vehicleId: values.vehicleId,
      companyName: values.companyName.trim(),
      policyNumber: values.policyNumber.trim(),
      startDate: new Date(values.startDate).toISOString(),
      endDate: new Date(values.endDate).toISOString(),
      price: Number(values.price),
    });
  }

  return (
    <FormDialog
      open
      onOpenChange={(open) => !open && onClose()}
      title={isNew ? "Yeni sığorta polisi" : "Polisi redaktə et"}
      submitting={save.isPending}
      onSubmit={submit}
    >
      <Field label="Maşın" required error={errors.vehicleId} className="sm:col-span-2">
        {(props) => (
          <VehiclePicker {...props} value={values.vehicleId} onChange={set("vehicleId")} />
        )}
      </Field>

      <Field label="Sığorta şirkəti" required error={errors.companyName}>
        {(props) => (
          <Input
            {...props}
            value={values.companyName}
            onChange={set("companyName")}
            placeholder="PAŞA Sığorta"
          />
        )}
      </Field>

      <Field label="Polis nömrəsi" required error={errors.policyNumber}>
        {(props) => (
          <Input
            {...props}
            value={values.policyNumber}
            onChange={set("policyNumber")}
            placeholder="POL-2026-12345"
            className="font-mono"
          />
        )}
      </Field>

      <Field label="Başlama tarixi" required>
        {(props) => (
          <Input {...props} type="date" value={values.startDate} onChange={set("startDate")} />
        )}
      </Field>

      <Field
        label="Bitmə tarixi"
        required
        error={errors.endDate}
        hint={errors.endDate ? undefined : "Bitməyə 30 gün qalanda avtomatik xəbərdarlıq yaranır"}
      >
        {(props) => (
          <Input
            {...props}
            type="date"
            min={values.startDate}
            value={values.endDate}
            onChange={set("endDate")}
          />
        )}
      </Field>

      <Field label="Qiymət (₼)" required error={errors.price} className="sm:col-span-2">
        {(props) => (
          <Input
            {...props}
            type="number"
            inputMode="decimal"
            step="0.01"
            min={0}
            value={values.price}
            onChange={set("price")}
            placeholder="0.00"
          />
        )}
      </Field>
    </FormDialog>
  );
}
