import { useState } from "react";
import { Archive, CornerDownLeft, Pencil, Plus, Trash2 } from "lucide-react";
import { vehicleAssignments as assignmentsApi, type AssignmentInput } from "@/api/endpoints";
import type { VehicleAssignment } from "@/api/types";
import { date, toDateInput } from "@/lib/format";
import { useAuth } from "@/auth/AuthContext";
import { useResourceList } from "@/components/useResourceList";
import { DataTable, type Column } from "@/components/DataTable";
import { PageHeader } from "@/components/PageHeader";
import { FilterControl, Toolbar } from "@/components/Toolbar";
import { RowActions } from "@/components/RowActions";
import { FormDialog, useResourceMutation } from "@/components/FormDialog";
import { DriverPicker, VehiclePicker } from "@/components/pickers";
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

const FILTERS = ["vehicleId", "driverId", "isActive", "isOpen"] as const;

export function AssignmentsPage() {
  const { canManage, isAdmin } = useAuth();

  const list = useResourceList<VehicleAssignment>({
    key: "vehicleassignments",
    fetcher: assignmentsApi.list,
    defaultSort: "assignedDate",
    filters: FILTERS,
  });

  const [editing, setEditing] = useState<VehicleAssignment | "new" | null>(null);
  const returning = useConfirm<VehicleAssignment>();
  const archive = useConfirm<VehicleAssignment>();
  const destroy = useConfirm<VehicleAssignment>();

  const invalidate = ["vehicleassignments", "dashboard"];

  const handBack = useResourceMutation(
    (row: VehicleAssignment) => assignmentsApi.return(row.id),
    { invalidate },
  );

  const toggle = useResourceMutation((row: VehicleAssignment) => assignmentsApi.toggle(row.id), {
    invalidate,
  });

  const remove = useResourceMutation((row: VehicleAssignment) => assignmentsApi.remove(row.id), {
    invalidate,
  });

  const columns: Array<Column<VehicleAssignment>> = [
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
      mobile: "subtitle",
      cell: (row) => row.driverFullName ?? "—",
    },
    {
      key: "assignedDate",
      header: "Verilib",
      sortable: true,
      mobile: "meta",
      cell: (row) => <span className="tnum">{date(row.assignedDate)}</span>,
    },
    {
      key: "returnedDate",
      header: "Qaytarılıb",
      sortable: true,
      mobile: "meta",
      cell: (row) =>
        row.returnedDate ? (
          <span className="tnum">{date(row.returnedDate)}</span>
        ) : (
          <span className="text-muted-foreground">—</span>
        ),
    },
    {
      key: "note",
      header: "Qeyd",
      className: "max-w-xs",
      mobile: "hidden",
      cell: (row) => (
        <span className="block truncate text-muted-foreground">{row.note || "—"}</span>
      ),
    },
    {
      key: "isActive",
      header: "Vəziyyət",
      sortable: true,
      mobile: "trailing",
      cell: (row) =>
        row.returnedDate ? (
          <Badge tone="neutral">Qaytarılıb</Badge>
        ) : (
          <Badge tone="success" dot>
            Sürücüdə
          </Badge>
        ),
    },
  ];

  return (
    <>
      <PageHeader
        title="Təyinatlar"
        description="Maşınların sürücülərə verilməsi və qaytarılması."
        actions={
          canManage && (
            <Button variant="primary" onClick={() => setEditing("new")}>
              <Plus />
              Yeni təyinat
            </Button>
          )
        }
      />

      <Toolbar
        search={list.value("search")}
        onSearch={(value) => list.setParam("search", value)}
        searchPlaceholder="Qeyd, nişan və ya sürücü adı…"
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

            <FilterControl label="Vəziyyət">
              <NativeSelect
                value={list.value("isOpen")}
                onChange={(event) => list.setParam("isOpen", event.target.value)}
              >
                <option value="">Hamısı</option>
                <option value="true">Sürücüdədir</option>
                <option value="false">Qaytarılıb</option>
              </NativeSelect>
            </FilterControl>

            <FilterControl label="Aktivlik">
              <NativeSelect
                value={list.value("isActive")}
                onChange={(event) => list.setParam("isActive", event.target.value)}
              >
                <option value="">Hamısı</option>
                <option value="true">Aktiv</option>
                <option value="false">Deaktiv</option>
              </NativeSelect>
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
        emptyTitle={list.hasFilters ? "Uyğun təyinat tapılmadı" : "Hələ təyinat yoxdur"}
        emptyDescription={
          list.hasFilters
            ? "Filtrləri dəyişib yenidən cəhd edin."
            : "Bir maşını sürücüyə təyin edərək başlayın."
        }
        actions={
          canManage
            ? (row) => (
                <RowActions>
                  {!row.returnedDate && (
                    <MenuItem onSelect={() => returning.ask(row)}>
                      <CornerDownLeft />
                      Qaytarıldı kimi işarələ
                    </MenuItem>
                  )}

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
        <AssignmentFormDialog
          assignment={editing === "new" ? null : editing}
          onClose={() => setEditing(null)}
        />
      )}

      <Confirm
        open={returning.open}
        onOpenChange={returning.onOpenChange}
        title="Maşın qaytarıldı"
        description={
          `${returning.target?.vehiclePlateNumber} nömrəli maşın ${returning.target?.driverFullName} ` +
          "tərəfindən bu gün qaytarılmış kimi qeydə alınacaq və maşın yenidən boş sayılacaq."
        }
        confirmLabel="Qaytarıldı"
        onConfirm={() => handBack.mutateAsync(returning.target!)}
      />

      <Confirm
        open={archive.open}
        onOpenChange={archive.onOpenChange}
        title="Təyinatı arxivə göndər"
        description="Bu təyinat qeydi arxivə göndəriləcək. Arxivdən geri qaytara bilərsiniz."
        confirmLabel="Arxivə göndər"
        onConfirm={() => toggle.mutateAsync(archive.target!)}
      />

      <Confirm
        open={destroy.open}
        onOpenChange={destroy.onOpenChange}
        title="Həmişəlik sil"
        description="Bu təyinat qeydi bazadan tamamilə silinəcək və geri qaytarıla bilməz."
        confirmLabel="Həmişəlik sil"
        danger
        onConfirm={() => remove.mutateAsync(destroy.target!)}
      />
    </>
  );
}

function AssignmentFormDialog({
  assignment,
  onClose,
}: {
  assignment: VehicleAssignment | null;
  onClose: () => void;
}) {
  const isNew = assignment === null;

  const [values, setValues] = useState({
    vehicleId: assignment?.vehicleId ?? "",
    driverId: assignment?.driverId ?? "",
    assignedDate: toDateInput(assignment?.assignedDate ?? new Date()),
    returnedDate: toDateInput(assignment?.returnedDate),
    isActive: String(assignment?.isActive ?? true),
    note: assignment?.note ?? "",
  });

  const [errors, setErrors] = useState<Partial<Record<keyof typeof values, string>>>({});

  const save = useResourceMutation(
    (payload: AssignmentInput | Partial<AssignmentInput>) =>
      isNew
        ? assignmentsApi.create(payload as AssignmentInput)
        : assignmentsApi.update(assignment.id, payload),
    { invalidate: ["vehicleassignments", "dashboard"], onSuccess: onClose },
  );

  const set =
    (key: keyof typeof values) =>
    (event: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) =>
      setValues((previous) => ({ ...previous, [key]: event.target.value }));

  function submit() {
    const next: typeof errors = {};

    if (!values.vehicleId) next.vehicleId = "Maşın seçilməlidir.";
    if (!values.driverId) next.driverId = "Sürücü seçilməlidir.";
    if (values.assignedDate > toDateInput(new Date())) {
      next.assignedDate = "Gələcək tarix ola bilməz.";
    }
    if (values.returnedDate && values.returnedDate < values.assignedDate) {
      next.returnedDate = "Təyinat tarixindən əvvəl ola bilməz.";
    }

    setErrors(next);
    if (Object.keys(next).length > 0) return;

    save.mutate({
      vehicleId: values.vehicleId,
      driverId: values.driverId,
      assignedDate: new Date(values.assignedDate).toISOString(),
      returnedDate: values.returnedDate ? new Date(values.returnedDate).toISOString() : null,
      isActive: values.isActive === "true",
      note: values.note.trim() || null,
    });
  }

  return (
    <FormDialog
      open
      onOpenChange={(open) => !open && onClose()}
      title={isNew ? "Yeni təyinat" : "Təyinatı redaktə et"}
      description={
        isNew
          ? "Bir maşın eyni anda yalnız bir sürücüdə, bir sürücü də yalnız bir maşında ola bilər."
          : undefined
      }
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

      <Field label="Verilmə tarixi" required error={errors.assignedDate}>
        {(props) => (
          <Input
            {...props}
            type="date"
            max={toDateInput(new Date())}
            value={values.assignedDate}
            onChange={set("assignedDate")}
          />
        )}
      </Field>

      <Field
        label="Qaytarılma tarixi"
        error={errors.returnedDate}
        hint={errors.returnedDate ? undefined : "Maşın hələ sürücüdədirsə, boş buraxın"}
      >
        {(props) => (
          <Input
            {...props}
            type="date"
            min={values.assignedDate}
            value={values.returnedDate}
            onChange={set("returnedDate")}
          />
        )}
      </Field>

      <Field label="Aktivlik" required className="sm:col-span-2">
        {(props) => (
          <NativeSelect {...props} value={values.isActive} onChange={set("isActive")}>
            <option value="true">Aktiv</option>
            <option value="false">Deaktiv</option>
          </NativeSelect>
        )}
      </Field>

      <Field label="Qeyd" hint="İstəyə bağlı, maks. 500 simvol" className="sm:col-span-2">
        {(props) => (
          <Textarea
            {...props}
            value={values.note ?? ""}
            onChange={set("note")}
            maxLength={500}
            placeholder="Təyinat haqqında qeyd"
          />
        )}
      </Field>
    </FormDialog>
  );
}
