import { useState } from "react";
import { Archive, Pencil, Plus, Trash2 } from "lucide-react";
import { maintenances as maintenanceApi, type MaintenanceInput } from "@/api/endpoints";
import { MaintenanceType, type Maintenance } from "@/api/types";
import { maintenanceTypes } from "@/lib/enums";
import { date, daysUntil, km, money, relativeDays, toDateInput } from "@/lib/format";
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

const FILTERS = ["vehicleId", "serviceType", "from", "to", "dueBefore"] as const;

/** Turns a next-service date into the badge a planner actually reads. */
export function DueBadge({ value }: { value?: string | null }) {
  if (!value) return <span className="text-muted-foreground">—</span>;

  const days = daysUntil(value);
  const tone = days === null ? "neutral" : days < 0 ? "danger" : days <= 30 ? "warning" : "neutral";

  return (
    <span className="inline-flex items-center gap-2">
      <span className="tnum">{date(value)}</span>
      {tone !== "neutral" && <Badge tone={tone}>{relativeDays(value)}</Badge>}
    </span>
  );
}

export function MaintenancePage() {
  const { canManage, isAdmin } = useAuth();

  const list = useResourceList<Maintenance>({
    key: "maintenances",
    fetcher: maintenanceApi.list,
    defaultSort: "maintenanceDate",
    filters: FILTERS,
  });

  const [editing, setEditing] = useState<Maintenance | "new" | null>(null);
  const archive = useConfirm<Maintenance>();
  const destroy = useConfirm<Maintenance>();

  const toggle = useResourceMutation((row: Maintenance) => maintenanceApi.toggle(row.id), {
    invalidate: ["maintenances", "dashboard"],
  });

  const remove = useResourceMutation((row: Maintenance) => maintenanceApi.remove(row.id), {
    invalidate: ["maintenances", "dashboard"],
  });

  const columns: Array<Column<Maintenance>> = [
    {
      key: "maintenanceDate",
      header: "Tarix",
      sortable: true,
      mobile: "subtitle",
      cell: (row) => <span className="tnum">{date(row.maintenanceDate)}</span>,
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
      key: "serviceType",
      header: "Servis növü",
      sortable: true,
      mobile: "meta",
      cell: (row) => (
        <Badge tone={maintenanceTypes.tone(row.serviceType)}>
          {maintenanceTypes.label(row.serviceType)}
        </Badge>
      ),
    },
    {
      key: "serviceCenter",
      header: "Servis mərkəzi",
      sortable: true,
      mobile: "meta",
      cell: (row) => row.serviceCenter,
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
      key: "nextMaintenanceDate",
      header: "Növbəti servis",
      sortable: true,
      mobile: "meta",
      cell: (row) => <DueBadge value={row.nextMaintenanceDate} />,
    },
    {
      key: "cost",
      header: "Xərc",
      sortable: true,
      align: "right",
      mobile: "trailing",
      cell: (row) => <span className="font-medium tnum">{money(row.cost)}</span>,
    },
  ];

  return (
    <>
      <PageHeader
        title="Servis"
        description="Texniki baxış tarixçəsi və planlaşdırılmış növbəti servislər."
        actions={
          canManage && (
            <Button variant="primary" onClick={() => setEditing("new")}>
              <Plus />
              Yeni servis
            </Button>
          )
        }
      />

      <Toolbar
        search={list.value("search")}
        onSearch={(value) => list.setParam("search", value)}
        searchPlaceholder="Servis mərkəzi, qeyd və ya nişan…"
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

            <FilterControl label="Növ" className="lg:w-44">
              <NativeSelect
                value={list.value("serviceType")}
                onChange={(event) => list.setParam("serviceType", event.target.value)}
              >
                <EnumOptions entries={maintenanceTypes.list} placeholder="Bütün növlər" />
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

            <FilterControl label="Növbəti servis ...-dək">
              <Input
                type="date"
                value={list.value("dueBefore")}
                onChange={(event) => list.setParam("dueBefore", event.target.value)}
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
        emptyTitle={list.hasFilters ? "Uyğun servis tapılmadı" : "Hələ servis qeydi yoxdur"}
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
        <MaintenanceFormDialog
          record={editing === "new" ? null : editing}
          onClose={() => setEditing(null)}
        />
      )}

      <Confirm
        open={archive.open}
        onOpenChange={archive.onOpenChange}
        title="Servisi arxivə göndər"
        description="Bu servis qeydi arxivə göndəriləcək. Arxivdən geri qaytara bilərsiniz."
        confirmLabel="Arxivə göndər"
        onConfirm={() => toggle.mutateAsync(archive.target!)}
      />

      <Confirm
        open={destroy.open}
        onOpenChange={destroy.onOpenChange}
        title="Həmişəlik sil"
        description="Bu servis qeydi bazadan tamamilə silinəcək və geri qaytarıla bilməz."
        confirmLabel="Həmişəlik sil"
        danger
        onConfirm={() => remove.mutateAsync(destroy.target!)}
      />
    </>
  );
}

function MaintenanceFormDialog({
  record,
  onClose,
}: {
  record: Maintenance | null;
  onClose: () => void;
}) {
  const isNew = record === null;

  const [values, setValues] = useState({
    vehicleId: record?.vehicleId ?? "",
    serviceType: String(record?.serviceType ?? MaintenanceType.OilChange),
    description: record?.description ?? "",
    cost: record ? String(record.cost) : "",
    maintenanceDate: toDateInput(record?.maintenanceDate ?? new Date()),
    nextMaintenanceDate: toDateInput(record?.nextMaintenanceDate),
    mileage: record ? String(record.mileage) : "",
    serviceCenter: record?.serviceCenter ?? "",
  });

  const [errors, setErrors] = useState<Partial<Record<keyof typeof values, string>>>({});

  const save = useResourceMutation(
    (payload: MaintenanceInput | Partial<MaintenanceInput>) =>
      isNew
        ? maintenanceApi.create(payload as MaintenanceInput)
        : maintenanceApi.update(record.id, payload),
    // Like a fuel log, a service visit advances the odometer server-side.
    { invalidate: ["maintenances", "vehicles", "dashboard"], onSuccess: onClose },
  );

  const set =
    (key: keyof typeof values) =>
    (event: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) =>
      setValues((previous) => ({ ...previous, [key]: event.target.value }));

  function submit() {
    const next: typeof errors = {};

    if (!values.vehicleId) next.vehicleId = "Maşın seçilməlidir.";
    if (!(Number(values.cost) > 0)) next.cost = "Xərc 0-dan böyük olmalıdır.";
    if (Number(values.mileage) < 0) next.mileage = "Mənfi ola bilməz.";
    if (values.serviceCenter.trim().length < 2) next.serviceCenter = "Ən azı 2 simvol.";
    if (values.maintenanceDate > toDateInput(new Date())) {
      next.maintenanceDate = "Gələcək tarix ola bilməz.";
    }
    if (values.nextMaintenanceDate && values.nextMaintenanceDate <= values.maintenanceDate) {
      next.nextMaintenanceDate = "Servis tarixindən sonra olmalıdır.";
    }

    setErrors(next);
    if (Object.keys(next).length > 0) return;

    save.mutate({
      vehicleId: values.vehicleId,
      serviceType: Number(values.serviceType),
      description: values.description.trim() || null,
      cost: Number(values.cost),
      maintenanceDate: new Date(values.maintenanceDate).toISOString(),
      nextMaintenanceDate: values.nextMaintenanceDate
        ? new Date(values.nextMaintenanceDate).toISOString()
        : null,
      mileage: Number(values.mileage),
      serviceCenter: values.serviceCenter.trim(),
    });
  }

  return (
    <FormDialog
      open
      onOpenChange={(open) => !open && onClose()}
      title={isNew ? "Yeni servis qeydi" : "Servisi redaktə et"}
      submitting={save.isPending}
      onSubmit={submit}
    >
      <Field label="Maşın" required error={errors.vehicleId}>
        {(props) => (
          <VehiclePicker {...props} value={values.vehicleId} onChange={set("vehicleId")} />
        )}
      </Field>

      <Field label="Servis növü" required>
        {(props) => (
          <NativeSelect {...props} value={values.serviceType} onChange={set("serviceType")}>
            <EnumOptions entries={maintenanceTypes.list} />
          </NativeSelect>
        )}
      </Field>

      <Field label="Servis mərkəzi" required error={errors.serviceCenter}>
        {(props) => (
          <Input
            {...props}
            value={values.serviceCenter}
            onChange={set("serviceCenter")}
            placeholder="AutoLux Servis"
          />
        )}
      </Field>

      <Field label="Xərc (₼)" required error={errors.cost}>
        {(props) => (
          <Input
            {...props}
            type="number"
            inputMode="decimal"
            step="0.01"
            min={0}
            value={values.cost}
            onChange={set("cost")}
            placeholder="0.00"
          />
        )}
      </Field>

      <Field label="Servis tarixi" required error={errors.maintenanceDate}>
        {(props) => (
          <Input
            {...props}
            type="date"
            max={toDateInput(new Date())}
            value={values.maintenanceDate}
            onChange={set("maintenanceDate")}
          />
        )}
      </Field>

      <Field
        label="Növbəti servis"
        error={errors.nextMaintenanceDate}
        hint={
          errors.nextMaintenanceDate
            ? undefined
            : "Doldursanız, tarix yaxınlaşanda avtomatik xəbərdarlıq yaranır"
        }
      >
        {(props) => (
          <Input
            {...props}
            type="date"
            min={values.maintenanceDate}
            value={values.nextMaintenanceDate}
            onChange={set("nextMaintenanceDate")}
          />
        )}
      </Field>

      <Field label="Yürüş (km)" required error={errors.mileage} className="sm:col-span-2">
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

      <Field label="Qeyd" hint="İstəyə bağlı, maks. 500 simvol" className="sm:col-span-2">
        {(props) => (
          <Textarea
            {...props}
            value={values.description ?? ""}
            onChange={set("description")}
            maxLength={500}
            placeholder="Görülən işlər"
          />
        )}
      </Field>
    </FormDialog>
  );
}
