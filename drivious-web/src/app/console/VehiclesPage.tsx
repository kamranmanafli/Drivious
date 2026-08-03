import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Archive, Car, Pencil, Plus, Trash2 } from "lucide-react";
import { vehicles as vehiclesApi, type VehicleInput } from "@/api/endpoints";
import { assetUrl } from "@/api/client";
import { FuelType, VehicleStatus, type Vehicle } from "@/api/types";
import { fuelTypes, vehicleStatuses } from "@/lib/enums";
import { km, number } from "@/lib/format";
import { useAuth } from "@/auth/AuthContext";
import { useResourceList } from "@/components/useResourceList";
import { DataTable, type Column } from "@/components/DataTable";
import { PageHeader } from "@/components/PageHeader";
import { FilterControl, Toolbar } from "@/components/Toolbar";
import { RowActions } from "@/components/RowActions";
import { FormDialog, useResourceMutation } from "@/components/FormDialog";
import { ImageInput } from "@/components/ImageInput";
import { EnumOptions } from "@/components/pickers";
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

const FILTERS = ["status", "fuelType", "brand", "minYear", "maxYear"] as const;

export function VehiclesPage() {
  const { canManage, isAdmin } = useAuth();
  const navigate = useNavigate();

  const list = useResourceList<Vehicle>({
    key: "vehicles",
    fetcher: vehiclesApi.list,
    defaultSort: "createdAt",
    filters: FILTERS,
  });

  const [editing, setEditing] = useState<Vehicle | "new" | null>(null);
  const archive = useConfirm<Vehicle>();
  const destroy = useConfirm<Vehicle>();

  const toggle = useResourceMutation((vehicle: Vehicle) => vehiclesApi.toggle(vehicle.id), {
    invalidate: ["vehicles", "dashboard"],
  });

  const remove = useResourceMutation((vehicle: Vehicle) => vehiclesApi.remove(vehicle.id), {
    invalidate: ["vehicles", "dashboard"],
  });

  const columns: Array<Column<Vehicle>> = [
    {
      key: "brand",
      header: "Maşın",
      sortable: true,
      mobile: "title",
      cell: (vehicle) => (
        <div className="flex items-center gap-2.5">
          <Avatar
            src={assetUrl(vehicle.imageURL)}
            name={vehicle.brand}
            shape="square"
            className="size-8 md:size-9"
          />
          <div className="min-w-0">
            <p className="truncate font-medium">
              {vehicle.brand} {vehicle.model}
            </p>
            <p className="truncate text-xs text-muted-foreground md:hidden">
              {vehicle.plateNumber}
            </p>
          </div>
        </div>
      ),
    },
    {
      key: "plateNumber",
      header: "Dövlət nişanı",
      sortable: true,
      mobile: "meta",
      cell: (vehicle) => <span className="font-mono text-xs">{vehicle.plateNumber}</span>,
    },
    {
      key: "year",
      header: "İl",
      sortable: true,
      mobile: "meta",
      cell: (vehicle) => vehicle.year,
    },
    {
      key: "fuelType",
      header: "Yanacaq",
      mobile: "meta",
      cell: (vehicle) => fuelTypes.label(vehicle.fuelType),
    },
    {
      key: "mileage",
      header: "Yürüş",
      sortable: true,
      align: "right",
      mobile: "meta",
      cell: (vehicle) => <span className="tnum">{km(vehicle.mileage)}</span>,
    },
    {
      key: "status",
      header: "Status",
      sortable: true,
      mobile: "trailing",
      cell: (vehicle) => (
        <Badge tone={vehicleStatuses.tone(vehicle.status)} dot>
          {vehicleStatuses.label(vehicle.status)}
        </Badge>
      ),
    },
  ];

  return (
    <>
      <PageHeader
        title="Maşınlar"
        description="Filodakı bütün nəqliyyat vasitələri."
        actions={
          canManage && (
            <Button variant="primary" onClick={() => setEditing("new")}>
              <Plus />
              Yeni maşın
            </Button>
          )
        }
      />

      <Toolbar
        search={list.value("search")}
        onSearch={(value) => list.setParam("search", value)}
        searchPlaceholder="Marka, model, nişan və ya VIN…"
        activeFilterCount={list.activeFilterCount}
        onClear={list.clearFilters}
        filters={
          <>
            <FilterControl label="Status">
              <NativeSelect
                value={list.value("status")}
                onChange={(event) => list.setParam("status", event.target.value)}
              >
                <EnumOptions entries={vehicleStatuses.list} placeholder="Bütün statuslar" />
              </NativeSelect>
            </FilterControl>

            <FilterControl label="Yanacaq">
              <NativeSelect
                value={list.value("fuelType")}
                onChange={(event) => list.setParam("fuelType", event.target.value)}
              >
                <EnumOptions entries={fuelTypes.list} placeholder="Bütün növlər" />
              </NativeSelect>
            </FilterControl>

            <FilterControl label="Marka">
              <Input
                value={list.value("brand")}
                onChange={(event) => list.setParam("brand", event.target.value)}
                placeholder="Marka"
              />
            </FilterControl>

            <FilterControl label="İl (min)" className="lg:w-24">
              <Input
                type="number"
                inputMode="numeric"
                value={list.value("minYear")}
                onChange={(event) => list.setParam("minYear", event.target.value)}
                placeholder="1900"
              />
            </FilterControl>

            <FilterControl label="İl (maks)" className="lg:w-24">
              <Input
                type="number"
                inputMode="numeric"
                value={list.value("maxYear")}
                onChange={(event) => list.setParam("maxYear", event.target.value)}
                placeholder={String(new Date().getFullYear() + 1)}
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
        onRowClick={(vehicle) => navigate(`/vehicles/${vehicle.id}`)}
        rowKey={(vehicle) => vehicle.id}
        emptyTitle={list.hasFilters ? "Uyğun maşın tapılmadı" : "Hələ maşın əlavə edilməyib"}
        emptyDescription={
          list.hasFilters
            ? "Filtrləri dəyişib yenidən cəhd edin."
            : canManage
              ? "İlk maşını əlavə edərək başlayın."
              : undefined
        }
        emptyAction={
          canManage && !list.hasFilters ? (
            <Button variant="primary" onClick={() => setEditing("new")}>
              <Plus />
              Yeni maşın
            </Button>
          ) : undefined
        }
        actions={
          canManage
            ? (vehicle) => (
                <RowActions>
                  <MenuItem onSelect={() => navigate(`/vehicles/${vehicle.id}`)}>
                    <Car />
                    Ətraflı
                  </MenuItem>

                  <MenuItem onSelect={() => setEditing(vehicle)}>
                    <Pencil />
                    Redaktə et
                  </MenuItem>

                  <MenuSeparator />

                  <MenuItem onSelect={() => archive.ask(vehicle)}>
                    <Archive />
                    Arxivə göndər
                  </MenuItem>

                  {isAdmin && (
                    <MenuItem danger onSelect={() => destroy.ask(vehicle)}>
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
        <VehicleFormDialog
          vehicle={editing === "new" ? null : editing}
          onClose={() => setEditing(null)}
        />
      )}

      <Confirm
        open={archive.open}
        onOpenChange={archive.onOpenChange}
        title="Maşını arxivə göndər"
        description={
          `${archive.target?.brand} ${archive.target?.model} (${archive.target?.plateNumber}) arxivə göndəriləcək. ` +
          "Bütün əlaqəli xərc, gəlir, servis və sənəd qeydləri də onunla birlikdə arxivlənir. " +
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
          `${destroy.target?.plateNumber} bazadan tamamilə silinəcək və geri qaytarıla bilməz. ` +
          "Maşının əlaqəli qeydləri varsa, server bu əməliyyatı rədd edəcək — o halda arxivləmə istifadə edin."
        }
        confirmLabel="Həmişəlik sil"
        danger
        onConfirm={() => remove.mutateAsync(destroy.target!)}
      />
    </>
  );
}

/* ── Form ────────────────────────────────────────────────────────────────── */

interface FormState {
  brand: string;
  model: string;
  year: string;
  plateNumber: string;
  vin: string;
  color: string;
  fuelType: string;
  mileage: string;
  status: string;
}

function toState(vehicle: Vehicle | null): FormState {
  return {
    brand: vehicle?.brand ?? "",
    model: vehicle?.model ?? "",
    year: String(vehicle?.year ?? new Date().getFullYear()),
    plateNumber: vehicle?.plateNumber ?? "",
    vin: vehicle?.vin ?? "",
    color: vehicle?.color ?? "",
    fuelType: String(vehicle?.fuelType ?? FuelType.Gasoline),
    mileage: String(vehicle?.mileage ?? 0),
    status: String(vehicle?.status ?? VehicleStatus.Active),
  };
}

export function VehicleFormDialog({
  vehicle,
  onClose,
}: {
  vehicle: Vehicle | null;
  onClose: () => void;
}) {
  const [values, setValues] = useState<FormState>(() => toState(vehicle));
  const [image, setImage] = useState<File | null>(null);
  const [errors, setErrors] = useState<Partial<Record<keyof FormState | "image", string>>>({});

  const isNew = vehicle === null;

  const save = useResourceMutation(
    (payload: VehicleInput | Partial<VehicleInput>) =>
      isNew
        ? vehiclesApi.create(payload as VehicleInput)
        : vehiclesApi.update(vehicle.id, payload),
    { invalidate: ["vehicles", "dashboard"], onSuccess: onClose },
  );

  const set = (key: keyof FormState) => (event: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) =>
    setValues((previous) => ({ ...previous, [key]: event.target.value }));

  function submit() {
    const next: typeof errors = {};
    const maxYear = new Date().getFullYear() + 1;

    if (values.brand.trim().length < 2) next.brand = "Ən azı 2 simvol.";
    if (values.model.trim().length < 2) next.model = "Ən azı 2 simvol.";
    if (!values.plateNumber.trim()) next.plateNumber = "Dövlət nişanı tələb olunur.";
    // The API stores VIN as exactly 17 characters and rejects anything else.
    if (values.vin.trim().length !== 17) next.vin = "VIN dəqiq 17 simvol olmalıdır.";
    if (!values.color.trim()) next.color = "Rəng tələb olunur.";

    const year = Number(values.year);
    if (!Number.isFinite(year) || year < 1900 || year > maxYear) {
      next.year = `1900–${maxYear} aralığında olmalıdır.`;
    }

    if (Number(values.mileage) < 0) next.mileage = "Mənfi ola bilməz.";

    // Create requires a picture — the server's validator marks it NotNull.
    if (isNew && !image) next.image = "Maşın şəkli tələb olunur.";

    setErrors(next);
    if (Object.keys(next).length > 0) return;

    const payload = {
      brand: values.brand.trim(),
      model: values.model.trim(),
      year,
      plateNumber: values.plateNumber.trim().toUpperCase(),
      vin: values.vin.trim().toUpperCase(),
      color: values.color.trim(),
      fuelType: Number(values.fuelType),
      mileage: Number(values.mileage),
      status: Number(values.status),
      ...(image ? { image } : {}),
    };

    save.mutate(payload);
  }

  return (
    <FormDialog
      open
      onOpenChange={(open) => !open && onClose()}
      title={isNew ? "Yeni maşın" : "Maşını redaktə et"}
      description={isNew ? undefined : `${vehicle.brand} ${vehicle.model} · ${vehicle.plateNumber}`}
      submitting={save.isPending}
      onSubmit={submit}
    >
      <ImageInput
        label="Şəkil"
        required={isNew}
        error={errors.image}
        currentUrl={assetUrl(vehicle?.imageURL)}
        value={image}
        onChange={setImage}
        maxMb={5}
        className="sm:col-span-2"
      />

      <Field label="Marka" required error={errors.brand}>
        {(props) => <Input {...props} value={values.brand} onChange={set("brand")} placeholder="Toyota" />}
      </Field>

      <Field label="Model" required error={errors.model}>
        {(props) => <Input {...props} value={values.model} onChange={set("model")} placeholder="Corolla" />}
      </Field>

      <Field label="Dövlət nişanı" required error={errors.plateNumber}>
        {(props) => (
          <Input
            {...props}
            value={values.plateNumber}
            onChange={set("plateNumber")}
            placeholder="10-AB-123"
            className="font-mono uppercase"
          />
        )}
      </Field>

      <Field
        label="VIN"
        required
        error={errors.vin}
        hint={errors.vin ? undefined : `${values.vin.length}/17 simvol`}
      >
        {(props) => (
          <Input
            {...props}
            value={values.vin}
            onChange={set("vin")}
            maxLength={17}
            placeholder="JTDBR32E720012345"
            className="font-mono uppercase"
          />
        )}
      </Field>

      <Field label="Buraxılış ili" required error={errors.year}>
        {(props) => (
          <Input {...props} type="number" inputMode="numeric" value={values.year} onChange={set("year")} />
        )}
      </Field>

      <Field label="Rəng" required error={errors.color}>
        {(props) => <Input {...props} value={values.color} onChange={set("color")} placeholder="Ağ" />}
      </Field>

      <Field label="Yanacaq növü" required>
        {(props) => (
          <NativeSelect {...props} value={values.fuelType} onChange={set("fuelType")}>
            <EnumOptions entries={fuelTypes.list} />
          </NativeSelect>
        )}
      </Field>

      <Field label="Status" required>
        {(props) => (
          <NativeSelect {...props} value={values.status} onChange={set("status")}>
            <EnumOptions entries={vehicleStatuses.list} />
          </NativeSelect>
        )}
      </Field>

      <Field
        label="Yürüş (km)"
        required
        error={errors.mileage}
        hint={errors.mileage ? undefined : `Cari: ${number(Number(values.mileage) || 0)} km`}
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
    </FormDialog>
  );
}
