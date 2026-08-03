import { useState } from "react";
import { Archive, Download, Pencil, Plus, Trash2, Upload } from "lucide-react";
import { vehicleDocuments as documentsApi, type DocumentInput } from "@/api/endpoints";
import { assetUrl } from "@/api/client";
import { DocumentType, type VehicleDocument } from "@/api/types";
import { documentTypes } from "@/lib/enums";
import { date, daysUntil, relativeDays, toDateInput } from "@/lib/format";
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
  Label,
  MenuItem,
  MenuSeparator,
  NativeSelect,
  useConfirm,
} from "@/ui";

const FILTERS = ["vehicleId", "documentType"] as const;

function DocumentExpiry({ value }: { value?: string | null }) {
  // Null is a real answer here — plenty of documents simply do not expire.
  if (!value) return <span className="text-xs text-muted-foreground">Bitmə tarixi yoxdur</span>;

  const days = daysUntil(value);
  const tone = days === null ? "neutral" : days < 0 ? "danger" : days <= 30 ? "warning" : "success";

  return (
    <span className="inline-flex items-center gap-2">
      <span className="tnum">{date(value)}</span>
      <Badge tone={tone}>{days !== null && days < 0 ? "bitib" : relativeDays(value)}</Badge>
    </span>
  );
}

export function DocumentsPage() {
  const { canManage, isAdmin } = useAuth();

  const list = useResourceList<VehicleDocument>({
    key: "vehicledocuments",
    fetcher: documentsApi.list,
    defaultSort: "uploadDate",
    filters: FILTERS,
  });

  const [editing, setEditing] = useState<VehicleDocument | "new" | null>(null);
  const archive = useConfirm<VehicleDocument>();
  const destroy = useConfirm<VehicleDocument>();

  const toggle = useResourceMutation((row: VehicleDocument) => documentsApi.toggle(row.id), {
    invalidate: ["vehicledocuments", "dashboard"],
  });

  const remove = useResourceMutation((row: VehicleDocument) => documentsApi.remove(row.id), {
    invalidate: ["vehicledocuments", "dashboard"],
  });

  const columns: Array<Column<VehicleDocument>> = [
    {
      key: "title",
      header: "Sənəd",
      sortable: true,
      mobile: "title",
      cell: (row) => <span className="font-medium">{row.title}</span>,
    },
    {
      key: "plateNumber",
      header: "Maşın",
      sortable: true,
      mobile: "subtitle",
      cell: (row) => (
        <div className="min-w-0">
          <p className="truncate">{row.vehiclePlateNumber}</p>
          <p className="truncate text-xs text-muted-foreground">{row.vehicleName}</p>
        </div>
      ),
    },
    {
      key: "documentType",
      header: "Növ",
      sortable: true,
      mobile: "meta",
      cell: (row) => (
        <Badge tone={documentTypes.tone(row.documentType)}>
          {documentTypes.label(row.documentType)}
        </Badge>
      ),
    },
    {
      key: "uploadDate",
      header: "Yüklənib",
      sortable: true,
      mobile: "meta",
      cell: (row) => <span className="tnum">{date(row.uploadDate)}</span>,
    },
    {
      key: "expiryDate",
      header: "Bitmə tarixi",
      sortable: true,
      mobile: "meta",
      cell: (row) => <DocumentExpiry value={row.expiryDate} />,
    },
  ];

  return (
    <>
      <PageHeader
        title="Sənədlər"
        description="Texniki pasport, sığorta, müqavilə və digər sənədlər."
        actions={
          canManage && (
            <Button variant="primary" onClick={() => setEditing("new")}>
              <Plus />
              Sənəd yüklə
            </Button>
          )
        }
      />

      <Toolbar
        search={list.value("search")}
        onSearch={(value) => list.setParam("search", value)}
        searchPlaceholder="Başlıq və ya dövlət nişanı…"
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
                value={list.value("documentType")}
                onChange={(event) => list.setParam("documentType", event.target.value)}
              >
                <EnumOptions entries={documentTypes.list} placeholder="Bütün növlər" />
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
        emptyTitle={list.hasFilters ? "Uyğun sənəd tapılmadı" : "Hələ sənəd yüklənməyib"}
        emptyDescription={list.hasFilters ? "Filtrləri dəyişib yenidən cəhd edin." : undefined}
        actions={(row) => (
          <RowActions>
            <MenuItem asChild>
              <a href={assetUrl(row.fileUrl)} target="_blank" rel="noreferrer">
                <Download />
                Faylı aç
              </a>
            </MenuItem>

            {canManage && (
              <>
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
              </>
            )}
          </RowActions>
        )}
      />

      {editing && (
        <DocumentFormDialog
          document={editing === "new" ? null : editing}
          onClose={() => setEditing(null)}
        />
      )}

      <Confirm
        open={archive.open}
        onOpenChange={archive.onOpenChange}
        title="Sənədi arxivə göndər"
        description={`"${archive.target?.title}" arxivə göndəriləcək. Arxivdən geri qaytara bilərsiniz.`}
        confirmLabel="Arxivə göndər"
        onConfirm={() => toggle.mutateAsync(archive.target!)}
      />

      <Confirm
        open={destroy.open}
        onOpenChange={destroy.onOpenChange}
        title="Həmişəlik sil"
        description={`"${destroy.target?.title}" və onun faylı bazadan tamamilə silinəcək. Geri qaytarıla bilməz.`}
        confirmLabel="Həmişəlik sil"
        danger
        onConfirm={() => remove.mutateAsync(destroy.target!)}
      />
    </>
  );
}

function DocumentFormDialog({
  document,
  onClose,
}: {
  document: VehicleDocument | null;
  onClose: () => void;
}) {
  const isNew = document === null;

  const [values, setValues] = useState({
    vehicleId: document?.vehicleId ?? "",
    title: document?.title ?? "",
    documentType: String(document?.documentType ?? DocumentType.Registration),
    expiryDate: toDateInput(document?.expiryDate),
  });

  const [file, setFile] = useState<File | null>(null);
  const [errors, setErrors] = useState<Partial<Record<keyof typeof values | "file", string>>>({});

  const save = useResourceMutation(
    (payload: DocumentInput | Partial<DocumentInput>) =>
      isNew
        ? documentsApi.create(payload as DocumentInput)
        : documentsApi.update(document.id, payload),
    { invalidate: ["vehicledocuments", "notifications", "dashboard"], onSuccess: onClose },
  );

  const set =
    (key: keyof typeof values) => (event: React.ChangeEvent<HTMLInputElement | HTMLSelectElement>) =>
      setValues((previous) => ({ ...previous, [key]: event.target.value }));

  function submit() {
    const next: typeof errors = {};

    if (!values.vehicleId) next.vehicleId = "Maşın seçilməlidir.";
    if (values.title.trim().length < 3) next.title = "Ən azı 3 simvol.";
    // The API's create validator marks the file NotNull; an edit may keep the old one.
    if (isNew && !file) next.file = "Sənəd faylı tələb olunur.";
    if (file && file.size > 10 * 1024 * 1024) next.file = "Fayl 10 MB-dan böyük ola bilməz.";

    setErrors(next);
    if (Object.keys(next).length > 0) return;

    save.mutate({
      vehicleId: values.vehicleId,
      title: values.title.trim(),
      documentType: Number(values.documentType),
      expiryDate: values.expiryDate ? new Date(values.expiryDate).toISOString() : null,
      ...(file ? { file } : {}),
    });
  }

  return (
    <FormDialog
      open
      onOpenChange={(open) => !open && onClose()}
      title={isNew ? "Sənəd yüklə" : "Sənədi redaktə et"}
      submitting={save.isPending}
      onSubmit={submit}
    >
      <Field label="Maşın" required error={errors.vehicleId}>
        {(props) => (
          <VehiclePicker {...props} value={values.vehicleId} onChange={set("vehicleId")} />
        )}
      </Field>

      <Field label="Sənədin növü" required>
        {(props) => (
          <NativeSelect {...props} value={values.documentType} onChange={set("documentType")}>
            <EnumOptions entries={documentTypes.list} />
          </NativeSelect>
        )}
      </Field>

      <Field label="Başlıq" required error={errors.title} className="sm:col-span-2">
        {(props) => (
          <Input
            {...props}
            value={values.title}
            onChange={set("title")}
            maxLength={100}
            placeholder="Texniki pasport"
          />
        )}
      </Field>

      <Field
        label="Bitmə tarixi"
        hint="Bitməyən sənəd üçün boş buraxın. Doldursanız, xəbərdarlıq avtomatik yaranır."
        className="sm:col-span-2"
      >
        {(props) => (
          <Input {...props} type="date" value={values.expiryDate} onChange={set("expiryDate")} />
        )}
      </Field>

      <div className="flex flex-col gap-1.5 sm:col-span-2">
        <Label required={isNew}>Fayl</Label>

        <label
          className={
            "flex cursor-pointer items-center gap-3 rounded-md border border-dashed px-3 py-3 transition-colors hover:bg-muted " +
            (errors.file ? "border-danger" : "border-border")
          }
        >
          <span className="flex size-9 shrink-0 items-center justify-center rounded-md bg-muted text-muted-foreground">
            <Upload className="size-4" />
          </span>

          <span className="min-w-0 flex-1">
            <span className="block truncate text-sm">
              {file ? file.name : (document?.fileName ?? "Fayl seçin")}
            </span>
            <span className="block text-xs text-muted-foreground">
              PDF, şəkil və ya sənəd · maks. 10 MB
            </span>
          </span>

          <input
            type="file"
            className="sr-only"
            onChange={(event) => setFile(event.target.files?.[0] ?? null)}
          />
        </label>

        {errors.file && <p className="text-xs text-danger">{errors.file}</p>}
      </div>
    </FormDialog>
  );
}
