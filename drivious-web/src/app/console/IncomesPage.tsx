import { useState } from "react";
import { Archive, Pencil, Plus, Trash2 } from "lucide-react";
import { incomes as incomesApi, type IncomeInput } from "@/api/endpoints";
import type { Income } from "@/api/types";
import { date, money, toDateInput } from "@/lib/format";
import { useAuth } from "@/auth/AuthContext";
import { useResourceList } from "@/components/useResourceList";
import { DataTable, type Column } from "@/components/DataTable";
import { PageHeader } from "@/components/PageHeader";
import { FilterControl, Toolbar } from "@/components/Toolbar";
import { RowActions } from "@/components/RowActions";
import { FormDialog, useResourceMutation } from "@/components/FormDialog";
import { DriverPicker, VehiclePicker } from "@/components/pickers";
import {
  Button,
  Confirm,
  Field,
  Input,
  MenuItem,
  MenuSeparator,
  Textarea,
  useConfirm,
} from "@/ui";

const FILTERS = ["vehicleId", "driverId", "from", "to", "minAmount", "maxAmount"] as const;

export function IncomesPage() {
  const { canManage, isAdmin } = useAuth();

  const list = useResourceList<Income>({
    key: "incomes",
    fetcher: incomesApi.list,
    defaultSort: "incomeDate",
    filters: FILTERS,
  });

  const [editing, setEditing] = useState<Income | "new" | null>(null);
  const archive = useConfirm<Income>();
  const destroy = useConfirm<Income>();

  const toggle = useResourceMutation((row: Income) => incomesApi.toggle(row.id), {
    invalidate: ["incomes", "dashboard"],
  });

  const remove = useResourceMutation((row: Income) => incomesApi.remove(row.id), {
    invalidate: ["incomes", "dashboard"],
  });

  const columns: Array<Column<Income>> = [
    {
      key: "incomeDate",
      header: "Tarix",
      sortable: true,
      mobile: "subtitle",
      cell: (row) => <span className="tnum">{date(row.incomeDate)}</span>,
    },
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
      key: "driver",
      header: "Sürücü",
      sortable: true,
      mobile: "meta",
      cell: (row) => row.driverFullName ?? "—",
    },
    {
      key: "description",
      header: "Qeyd",
      className: "max-w-xs",
      mobile: "meta",
      cell: (row) => (
        <span className="block truncate text-muted-foreground">{row.description || "—"}</span>
      ),
    },
    {
      key: "amount",
      header: "Məbləğ",
      sortable: true,
      align: "right",
      mobile: "trailing",
      cell: (row) => <span className="font-medium text-success tnum">{money(row.amount)}</span>,
    },
  ];

  return (
    <>
      <PageHeader
        title="Gəlirlər"
        description="Maşın və sürücü üzrə qazanc qeydləri."
        actions={
          canManage && (
            <Button variant="primary" onClick={() => setEditing("new")}>
              <Plus />
              Yeni gəlir
            </Button>
          )
        }
      />

      <Toolbar
        search={list.value("search")}
        onSearch={(value) => list.setParam("search", value)}
        searchPlaceholder="Qeyd və ya dövlət nişanı…"
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

            <FilterControl label="Sürücü" className="lg:w-44">
              <DriverPicker
                placeholder="Bütün sürücülər"
                value={list.value("driverId")}
                onChange={(event) => list.setParam("driverId", event.target.value)}
              />
            </FilterControl>

            <FilterControl label="Tarixdən">
              <Input
                type="date"
                value={list.value("from")}
                onChange={(event) => list.setParam("from", event.target.value)}
              />
            </FilterControl>

            <FilterControl label="Tarixə">
              <Input
                type="date"
                value={list.value("to")}
                onChange={(event) => list.setParam("to", event.target.value)}
              />
            </FilterControl>

            <FilterControl label="Min ₼" className="lg:w-24">
              <Input
                type="number"
                inputMode="decimal"
                value={list.value("minAmount")}
                onChange={(event) => list.setParam("minAmount", event.target.value)}
                placeholder="0"
              />
            </FilterControl>

            <FilterControl label="Maks ₼" className="lg:w-24">
              <Input
                type="number"
                inputMode="decimal"
                value={list.value("maxAmount")}
                onChange={(event) => list.setParam("maxAmount", event.target.value)}
                placeholder="∞"
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
        emptyTitle={list.hasFilters ? "Uyğun gəlir tapılmadı" : "Hələ gəlir qeydi yoxdur"}
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
        <IncomeFormDialog
          income={editing === "new" ? null : editing}
          onClose={() => setEditing(null)}
        />
      )}

      <Confirm
        open={archive.open}
        onOpenChange={archive.onOpenChange}
        title="Gəliri arxivə göndər"
        description={`${money(archive.target?.amount ?? 0)} məbləğində gəlir arxivə göndəriləcək. Arxivdən geri qaytara bilərsiniz.`}
        confirmLabel="Arxivə göndər"
        onConfirm={() => toggle.mutateAsync(archive.target!)}
      />

      <Confirm
        open={destroy.open}
        onOpenChange={destroy.onOpenChange}
        title="Həmişəlik sil"
        description="Bu gəlir qeydi bazadan tamamilə silinəcək və geri qaytarıla bilməz."
        confirmLabel="Həmişəlik sil"
        danger
        onConfirm={() => remove.mutateAsync(destroy.target!)}
      />
    </>
  );
}

function IncomeFormDialog({ income, onClose }: { income: Income | null; onClose: () => void }) {
  const isNew = income === null;

  const [values, setValues] = useState({
    vehicleId: income?.vehicleId ?? "",
    driverId: income?.driverId ?? "",
    amount: income ? String(income.amount) : "",
    incomeDate: toDateInput(income?.incomeDate ?? new Date()),
    description: income?.description ?? "",
  });

  const [errors, setErrors] = useState<Partial<Record<keyof typeof values, string>>>({});

  const save = useResourceMutation(
    (payload: IncomeInput | Partial<IncomeInput>) =>
      isNew ? incomesApi.create(payload as IncomeInput) : incomesApi.update(income.id, payload),
    { invalidate: ["incomes", "dashboard"], onSuccess: onClose },
  );

  const set =
    (key: keyof typeof values) =>
    (event: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) =>
      setValues((previous) => ({ ...previous, [key]: event.target.value }));

  function submit() {
    const next: typeof errors = {};

    if (!values.vehicleId) next.vehicleId = "Maşın seçilməlidir.";
    if (!values.driverId) next.driverId = "Sürücü seçilməlidir.";
    if (!(Number(values.amount) > 0)) next.amount = "Məbləğ 0-dan böyük olmalıdır.";
    if (values.incomeDate > toDateInput(new Date())) {
      next.incomeDate = "Gələcək tarix ola bilməz.";
    }

    setErrors(next);
    if (Object.keys(next).length > 0) return;

    save.mutate({
      vehicleId: values.vehicleId,
      driverId: values.driverId,
      amount: Number(values.amount),
      incomeDate: new Date(values.incomeDate).toISOString(),
      description: values.description.trim() || null,
    });
  }

  return (
    <FormDialog
      open
      onOpenChange={(open) => !open && onClose()}
      title={isNew ? "Yeni gəlir" : "Gəliri redaktə et"}
      submitting={save.isPending}
      onSubmit={submit}
    >
      <Field label="Maşın" required error={errors.vehicleId}>
        {(props) => (
          <VehiclePicker {...props} value={values.vehicleId} onChange={set("vehicleId")} />
        )}
      </Field>

      <Field label="Sürücü" required error={errors.driverId}>
        {(props) => <DriverPicker {...props} value={values.driverId} onChange={set("driverId")} />}
      </Field>

      <Field label="Məbləğ (₼)" required error={errors.amount}>
        {(props) => (
          <Input
            {...props}
            type="number"
            inputMode="decimal"
            step="0.01"
            min={0}
            value={values.amount}
            onChange={set("amount")}
            placeholder="0.00"
          />
        )}
      </Field>

      <Field label="Tarix" required error={errors.incomeDate}>
        {(props) => (
          <Input
            {...props}
            type="date"
            max={toDateInput(new Date())}
            value={values.incomeDate}
            onChange={set("incomeDate")}
          />
        )}
      </Field>

      <Field label="Qeyd" hint="İstəyə bağlı, maks. 500 simvol" className="sm:col-span-2">
        {(props) => (
          <Textarea
            {...props}
            value={values.description ?? ""}
            onChange={set("description")}
            maxLength={500}
            placeholder="Gəlir haqqında qeyd"
          />
        )}
      </Field>
    </FormDialog>
  );
}
