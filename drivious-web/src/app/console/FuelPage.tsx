import { useState } from "react";
import { Archive, Pencil, Plus, Trash2 } from "lucide-react";
import { fuelLogs as fuelApi, type FuelLogInput } from "@/api/endpoints";
import type { FuelLog } from "@/api/types";
import { date, km, liters, money, toDateInput } from "@/lib/format";
import { useAuth } from "@/auth/AuthContext";
import { useResourceList } from "@/components/useResourceList";
import { DataTable, type Column } from "@/components/DataTable";
import { PageHeader } from "@/components/PageHeader";
import { FilterControl, Toolbar } from "@/components/Toolbar";
import { RowActions } from "@/components/RowActions";
import { FormDialog, useResourceMutation } from "@/components/FormDialog";
import { VehiclePicker } from "@/components/pickers";
import { Button, Confirm, Field, Input, MenuItem, MenuSeparator, useConfirm } from "@/ui";

const FILTERS = ["vehicleId", "from", "to"] as const;

export function FuelPage() {
  const { canManage, isAdmin } = useAuth();

  const list = useResourceList<FuelLog>({
    key: "fuellogs",
    fetcher: fuelApi.list,
    defaultSort: "fuelDate",
    filters: FILTERS,
  });

  const [editing, setEditing] = useState<FuelLog | "new" | null>(null);
  const archive = useConfirm<FuelLog>();
  const destroy = useConfirm<FuelLog>();

  const toggle = useResourceMutation((row: FuelLog) => fuelApi.toggle(row.id), {
    invalidate: ["fuellogs", "dashboard"],
  });

  const remove = useResourceMutation((row: FuelLog) => fuelApi.remove(row.id), {
    invalidate: ["fuellogs", "dashboard"],
  });

  const columns: Array<Column<FuelLog>> = [
    {
      key: "fuelDate",
      header: "Tarix",
      sortable: true,
      mobile: "subtitle",
      cell: (row) => <span className="tnum">{date(row.fuelDate)}</span>,
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
      key: "stationName",
      header: "Məntəqə",
      sortable: true,
      mobile: "meta",
      cell: (row) => row.stationName,
    },
    {
      key: "liters",
      header: "Litr",
      sortable: true,
      align: "right",
      mobile: "meta",
      cell: (row) => <span className="tnum">{liters(row.liters)}</span>,
    },
    {
      key: "mileage",
      header: "Yürüş",
      sortable: true,
      align: "right",
      mobile: "meta",
      cell: (row) => <span className="tnum">{km(row.mileage)}</span>,
    },
    {
      key: "unit",
      header: "₼/L",
      align: "right",
      mobile: "meta",
      // Derived, not stored — the useful number when comparing two fill-ups.
      cell: (row) => (
        <span className="tnum text-muted-foreground">
          {row.liters > 0 ? (row.price / row.liters).toFixed(2) : "—"}
        </span>
      ),
    },
    {
      key: "price",
      header: "Məbləğ",
      sortable: true,
      align: "right",
      mobile: "trailing",
      cell: (row) => <span className="font-medium tnum">{money(row.price)}</span>,
    },
  ];

  return (
    <>
      <PageHeader
        title="Yanacaq"
        description="Yanacaqdoldurma qeydləri və yürüş göstəriciləri."
        actions={
          canManage && (
            <Button variant="primary" onClick={() => setEditing("new")}>
              <Plus />
              Yeni qeyd
            </Button>
          )
        }
      />

      <Toolbar
        search={list.value("search")}
        onSearch={(value) => list.setParam("search", value)}
        searchPlaceholder="Məntəqə və ya dövlət nişanı…"
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
        emptyTitle={list.hasFilters ? "Uyğun qeyd tapılmadı" : "Hələ yanacaq qeydi yoxdur"}
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
        <FuelFormDialog log={editing === "new" ? null : editing} onClose={() => setEditing(null)} />
      )}

      <Confirm
        open={archive.open}
        onOpenChange={archive.onOpenChange}
        title="Qeydi arxivə göndər"
        description="Bu yanacaq qeydi arxivə göndəriləcək. Arxivdən geri qaytara bilərsiniz."
        confirmLabel="Arxivə göndər"
        onConfirm={() => toggle.mutateAsync(archive.target!)}
      />

      <Confirm
        open={destroy.open}
        onOpenChange={destroy.onOpenChange}
        title="Həmişəlik sil"
        description="Bu yanacaq qeydi bazadan tamamilə silinəcək və geri qaytarıla bilməz."
        confirmLabel="Həmişəlik sil"
        danger
        onConfirm={() => remove.mutateAsync(destroy.target!)}
      />
    </>
  );
}

function FuelFormDialog({ log, onClose }: { log: FuelLog | null; onClose: () => void }) {
  const isNew = log === null;

  const [values, setValues] = useState({
    vehicleId: log?.vehicleId ?? "",
    liters: log ? String(log.liters) : "",
    price: log ? String(log.price) : "",
    fuelDate: toDateInput(log?.fuelDate ?? new Date()),
    mileage: log ? String(log.mileage) : "",
    stationName: log?.stationName ?? "",
  });

  const [errors, setErrors] = useState<Partial<Record<keyof typeof values, string>>>({});

  const save = useResourceMutation(
    (payload: FuelLogInput | Partial<FuelLogInput>) =>
      isNew ? fuelApi.create(payload as FuelLogInput) : fuelApi.update(log.id, payload),
    // A fuel log advances the vehicle's odometer server-side, so the vehicle
    // list is refreshed alongside it.
    { invalidate: ["fuellogs", "vehicles", "dashboard"], onSuccess: onClose },
  );

  const set =
    (key: keyof typeof values) =>
    (event: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) =>
      setValues((previous) => ({ ...previous, [key]: event.target.value }));

  function submit() {
    const next: typeof errors = {};

    if (!values.vehicleId) next.vehicleId = "Maşın seçilməlidir.";
    if (!(Number(values.liters) > 0)) next.liters = "Litr 0-dan böyük olmalıdır.";
    if (!(Number(values.price) > 0)) next.price = "Məbləğ 0-dan böyük olmalıdır.";
    if (Number(values.mileage) < 0) next.mileage = "Mənfi ola bilməz.";
    if (values.stationName.trim().length < 2) next.stationName = "Ən azı 2 simvol.";
    if (values.fuelDate > toDateInput(new Date())) next.fuelDate = "Gələcək tarix ola bilməz.";

    setErrors(next);
    if (Object.keys(next).length > 0) return;

    save.mutate({
      vehicleId: values.vehicleId,
      liters: Number(values.liters),
      price: Number(values.price),
      fuelDate: new Date(values.fuelDate).toISOString(),
      mileage: Number(values.mileage),
      stationName: values.stationName.trim(),
    });
  }

  const unit =
    Number(values.liters) > 0 && Number(values.price) > 0
      ? (Number(values.price) / Number(values.liters)).toFixed(2)
      : null;

  return (
    <FormDialog
      open
      onOpenChange={(open) => !open && onClose()}
      title={isNew ? "Yeni yanacaq qeydi" : "Qeydi redaktə et"}
      submitting={save.isPending}
      onSubmit={submit}
    >
      <Field label="Maşın" required error={errors.vehicleId} className="sm:col-span-2">
        {(props) => (
          <VehiclePicker {...props} value={values.vehicleId} onChange={set("vehicleId")} />
        )}
      </Field>

      <Field label="Litr" required error={errors.liters}>
        {(props) => (
          <Input
            {...props}
            type="number"
            inputMode="decimal"
            step="0.01"
            min={0}
            value={values.liters}
            onChange={set("liters")}
            placeholder="0.00"
          />
        )}
      </Field>

      <Field
        label="Ümumi məbləğ (₼)"
        required
        error={errors.price}
        hint={errors.price ? undefined : unit ? `${unit} ₼/litr` : undefined}
      >
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

      <Field
        label="Yürüş (km)"
        required
        error={errors.mileage}
        hint={errors.mileage ? undefined : "Maşının odometri bu göstəriciyə görə yenilənir"}
      >
        {(props) => (
          <Input
            {...props}
            type="number"
            inputMode="numeric"
            min={0}
            value={values.mileage}
            onChange={set("mileage")}
          />
        )}
      </Field>

      <Field label="Tarix" required error={errors.fuelDate}>
        {(props) => (
          <Input
            {...props}
            type="date"
            max={toDateInput(new Date())}
            value={values.fuelDate}
            onChange={set("fuelDate")}
          />
        )}
      </Field>

      <Field label="Məntəqə" required error={errors.stationName} className="sm:col-span-2">
        {(props) => (
          <Input
            {...props}
            value={values.stationName}
            onChange={set("stationName")}
            placeholder="SOCAR"
          />
        )}
      </Field>
    </FormDialog>
  );
}
