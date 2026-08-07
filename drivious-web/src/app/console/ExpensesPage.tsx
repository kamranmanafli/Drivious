import { useState } from "react";
import { Archive, Pencil, Plus, Trash2 } from "lucide-react";
import { expenses as expensesApi, type ExpenseInput } from "@/api/endpoints";
import { ExpenseCategory, type Expense } from "@/api/types";
import { expenseCategories } from "@/lib/enums";
import { date, money, toDateInput } from "@/lib/format";
import { useAuth } from "@/auth/AuthContext";
import { useResourceList } from "@/components/useResourceList";
import { DataTable, type Column } from "@/components/DataTable";
import { PageHeader } from "@/components/PageHeader";
import { FilterControl, Toolbar } from "@/components/Toolbar";
import { RowActions } from "@/components/RowActions";
import { FormDialog, useResourceMutation } from "@/components/FormDialog";
import { EnumOptions, VehiclePicker } from "@/components/pickers";
import {
  Badge,
  Button,
  Confirm,
  Field,
  Input,
  MenuItem,
  MenuSeparator,
  NativeSelect,
  Textarea,
  useConfirm,
} from "@/ui";

const FILTERS = ["vehicleId", "category", "from", "to", "minAmount", "maxAmount"] as const;

export function ExpensesPage() {
  const { canManage, isAdmin } = useAuth();

  const list = useResourceList<Expense>({
    key: "expenses",
    fetcher: expensesApi.list,
    defaultSort: "expenseDate",
    filters: FILTERS,
  });

  const [editing, setEditing] = useState<Expense | "new" | null>(null);
  const archive = useConfirm<Expense>();
  const destroy = useConfirm<Expense>();

  const toggle = useResourceMutation((row: Expense) => expensesApi.toggle(row.id), {
    invalidate: ["expenses", "dashboard"],
  });

  const remove = useResourceMutation((row: Expense) => expensesApi.remove(row.id), {
    invalidate: ["expenses", "dashboard"],
  });

  const columns: Array<Column<Expense>> = [
    {
      key: "expenseDate",
      header: "Tarix",
      sortable: true,
      mobile: "subtitle",
      cell: (row) => <span className="tnum">{date(row.expenseDate)}</span>,
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
      key: "category",
      header: "Kateqoriya",
      sortable: true,
      mobile: "meta",
      cell: (row) => (
        <Badge tone={expenseCategories.tone(row.category)}>
          {expenseCategories.label(row.category)}
        </Badge>
      ),
    },
    {
      key: "description",
      header: "Təsvir",
      className: "max-w-xs",
      mobile: "meta",
      cell: (row) => (
        <span className="block truncate text-muted-foreground" title={row.description}>
          {row.description}
        </span>
      ),
    },
    {
      key: "amount",
      header: "Məbləğ",
      sortable: true,
      align: "right",
      mobile: "trailing",
      cell: (row) => <span className="font-medium tnum">{money(row.amount)}</span>,
    },
  ];

  return (
    <>
      <PageHeader
        title="Xərclər"
        description="Maşınlar üzrə bütün xərc qeydləri."
        actions={
          canManage && (
            <Button variant="primary" onClick={() => setEditing("new")}>
              <Plus />
              Yeni xərc
            </Button>
          )
        }
      />

      <Toolbar
        search={list.value("search")}
        onSearch={(value) => list.setParam("search", value)}
        searchPlaceholder="Təsvir və ya dövlət nişanı…"
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

            <FilterControl label="Kateqoriya">
              <NativeSelect
                value={list.value("category")}
                onChange={(event) => list.setParam("category", event.target.value)}
              >
                <EnumOptions entries={expenseCategories.list} placeholder="Hamısı" />
              </NativeSelect>
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
        emptyTitle={list.hasFilters ? "Uyğun xərc tapılmadı" : "Hələ xərc qeydi yoxdur"}
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
        <ExpenseFormDialog
          expense={editing === "new" ? null : editing}
          onClose={() => setEditing(null)}
        />
      )}

      <Confirm
        open={archive.open}
        onOpenChange={archive.onOpenChange}
        title="Xərci arxivə göndər"
        description={`${money(archive.target?.amount ?? 0)} məbləğində xərc arxivə göndəriləcək. Arxivdən geri qaytara bilərsiniz.`}
        confirmLabel="Arxivə göndər"
        onConfirm={() => toggle.mutateAsync(archive.target!)}
      />

      <Confirm
        open={destroy.open}
        onOpenChange={destroy.onOpenChange}
        title="Həmişəlik sil"
        description="Bu xərc qeydi bazadan tamamilə silinəcək və geri qaytarıla bilməz."
        confirmLabel="Həmişəlik sil"
        danger
        onConfirm={() => remove.mutateAsync(destroy.target!)}
      />
    </>
  );
}

function ExpenseFormDialog({
  expense,
  onClose,
}: {
  expense: Expense | null;
  onClose: () => void;
}) {
  const isNew = expense === null;

  const [values, setValues] = useState({
    vehicleId: expense?.vehicleId ?? "",
    category: String(expense?.category ?? ExpenseCategory.Fuel),
    amount: expense ? String(expense.amount) : "",
    expenseDate: toDateInput(expense?.expenseDate ?? new Date()),
    description: expense?.description ?? "",
  });

  const [errors, setErrors] = useState<Partial<Record<keyof typeof values, string>>>({});

  const save = useResourceMutation(
    (payload: ExpenseInput | Partial<ExpenseInput>) =>
      isNew
        ? expensesApi.create(payload as ExpenseInput)
        : expensesApi.update(expense.id, payload),
    { invalidate: ["expenses", "dashboard"], onSuccess: onClose },
  );

  const set =
    (key: keyof typeof values) =>
    (event: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) =>
      setValues((previous) => ({ ...previous, [key]: event.target.value }));

  function submit() {
    const next: typeof errors = {};

    if (!values.vehicleId) next.vehicleId = "Maşın seçilməlidir.";
    if (!(Number(values.amount) > 0)) next.amount = "Məbləğ 0-dan böyük olmalıdır.";
    if (values.description.trim().length < 5) next.description = "Ən azı 5 simvol.";
    // The API refuses a future expense date.
    if (values.expenseDate > toDateInput(new Date())) {
      next.expenseDate = "Gələcək tarix ola bilməz.";
    }

    setErrors(next);
    if (Object.keys(next).length > 0) return;

    save.mutate({
      vehicleId: values.vehicleId,
      category: Number(values.category),
      amount: Number(values.amount),
      expenseDate: new Date(values.expenseDate).toISOString(),
      description: values.description.trim(),
    });
  }

  return (
    <FormDialog
      open
      onOpenChange={(open) => !open && onClose()}
      title={isNew ? "Yeni xərc" : "Xərci redaktə et"}
      submitting={save.isPending}
      onSubmit={submit}
    >
      <Field label="Maşın" required error={errors.vehicleId} className="sm:col-span-2">
        {(props) => (
          <VehiclePicker {...props} value={values.vehicleId} onChange={set("vehicleId")} />
        )}
      </Field>

      <Field label="Kateqoriya" required>
        {(props) => (
          <NativeSelect {...props} value={values.category} onChange={set("category")}>
            <EnumOptions entries={expenseCategories.list} />
          </NativeSelect>
        )}
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

      <Field label="Tarix" required error={errors.expenseDate} className="sm:col-span-2">
        {(props) => (
          <Input
            {...props}
            type="date"
            max={toDateInput(new Date())}
            value={values.expenseDate}
            onChange={set("expenseDate")}
          />
        )}
      </Field>

      <Field
        label="Təsvir"
        required
        error={errors.description}
        hint={errors.description ? undefined : "5–500 simvol"}
        className="sm:col-span-2"
      >
        {(props) => (
          <Textarea
            {...props}
            value={values.description}
            onChange={set("description")}
            maxLength={500}
            placeholder="Xərc haqqında qeyd"
          />
        )}
      </Field>
    </FormDialog>
  );
}
