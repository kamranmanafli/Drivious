import { useState } from "react";
import { ArchiveRestore, Trash2 } from "lucide-react";
import {
  drivers as driversApi,
  expenses as expensesApi,
  fuelLogs as fuelApi,
  incomes as incomesApi,
  insurances as insuranceApi,
  maintenances as maintenanceApi,
  notifications as notificationsApi,
  vehicleAssignments as assignmentsApi,
  vehicleDocuments as documentsApi,
  vehicles as vehiclesApi,
} from "@/api/endpoints";
import type { PagedResult } from "@/api/types";
import { cn } from "@/lib/cn";
import { date, money } from "@/lib/format";
import { useAuth } from "@/auth/AuthContext";
import { useResourceList } from "@/components/useResourceList";
import { DataTable, type Column } from "@/components/DataTable";
import { PageHeader } from "@/components/PageHeader";
import { RowActions } from "@/components/RowActions";
import { useResourceMutation } from "@/components/FormDialog";
import { Confirm, MenuItem, MenuSeparator, useConfirm } from "@/ui";

/** Anything archived shares these three columns; `describe` supplies the rest. */
interface ArchivedRow {
  id: string;
  deletedAt?: string | null;
  createdAt: string;
  [key: string]: unknown;
}

interface Section {
  key: string;
  label: string;
  api: {
    deleted: (params?: Record<string, unknown>) => Promise<PagedResult<never>>;
    toggle: (id: string) => Promise<string>;
    remove: (id: string) => Promise<string>;
  };
  defaultSort: string;
  /** One line naming the row, and a second with its detail. */
  describe: (row: ArchivedRow) => { title: string; subtitle: string };
  /** Warned about because restoring or deleting it touches other rows too. */
  cascades?: boolean;
}

const SECTIONS: Section[] = [
  {
    key: "vehicles",
    label: "Maşınlar",
    api: vehiclesApi as unknown as Section["api"],
    defaultSort: "createdAt",
    cascades: true,
    describe: (r) => ({
      title: `${r.brand} ${r.model}`,
      subtitle: String(r.plateNumber ?? ""),
    }),
  },
  {
    key: "drivers",
    label: "Sürücülər",
    api: driversApi as unknown as Section["api"],
    defaultSort: "createdAt",
    cascades: true,
    describe: (r) => ({
      title: String(r.fullName ?? `${r.firstName} ${r.lastName}`),
      subtitle: String(r.phoneNumber ?? ""),
    }),
  },
  {
    key: "vehicleassignments",
    label: "Təyinatlar",
    api: assignmentsApi as unknown as Section["api"],
    defaultSort: "assignedDate",
    describe: (r) => ({
      title: `${r.vehiclePlateNumber} → ${r.driverFullName}`,
      subtitle: `${date(r.assignedDate as string)}-dən`,
    }),
  },
  {
    key: "incomes",
    label: "Gəlirlər",
    api: incomesApi as unknown as Section["api"],
    defaultSort: "incomeDate",
    describe: (r) => ({
      title: money(r.amount as number),
      subtitle: `${r.vehiclePlateNumber} · ${r.driverFullName}`,
    }),
  },
  {
    key: "expenses",
    label: "Xərclər",
    api: expensesApi as unknown as Section["api"],
    defaultSort: "expenseDate",
    describe: (r) => ({
      title: money(r.amount as number),
      subtitle: `${r.vehiclePlateNumber} · ${r.description}`,
    }),
  },
  {
    key: "fuellogs",
    label: "Yanacaq",
    api: fuelApi as unknown as Section["api"],
    defaultSort: "fuelDate",
    describe: (r) => ({
      title: money(r.price as number),
      subtitle: `${r.vehiclePlateNumber} · ${r.stationName}`,
    }),
  },
  {
    key: "maintenances",
    label: "Servis",
    api: maintenanceApi as unknown as Section["api"],
    defaultSort: "maintenanceDate",
    describe: (r) => ({
      title: money(r.cost as number),
      subtitle: `${r.vehiclePlateNumber} · ${r.serviceCenter}`,
    }),
  },
  {
    key: "insurances",
    label: "Sığorta",
    api: insuranceApi as unknown as Section["api"],
    defaultSort: "endDate",
    describe: (r) => ({
      title: String(r.policyNumber ?? ""),
      subtitle: `${r.vehiclePlateNumber} · ${r.companyName}`,
    }),
  },
  {
    key: "vehicledocuments",
    label: "Sənədlər",
    api: documentsApi as unknown as Section["api"],
    defaultSort: "uploadDate",
    describe: (r) => ({
      title: String(r.title ?? ""),
      subtitle: String(r.vehiclePlateNumber ?? ""),
    }),
  },
  {
    key: "notifications",
    label: "Bildirişlər",
    api: notificationsApi as unknown as Section["api"],
    defaultSort: "notificationDate",
    describe: (r) => ({
      title: String(r.title ?? ""),
      subtitle: String(r.message ?? ""),
    }),
  },
];

/**
 * The archive.
 *
 * `PATCH toggle/{id}` is the everyday delete, so nothing is really gone until
 * someone chooses to remove it here. Restoring a vehicle or a driver also
 * brings back the rows that were archived alongside it — but not the ones that
 * were archived deliberately, which is the API's rule and is spelled out in the
 * confirmation rather than left to be discovered.
 */
export function ArchivePage() {
  const { isAdmin } = useAuth();
  const [active, setActive] = useState(SECTIONS[0]);

  return (
    <>
      <PageHeader
        title="Arxiv"
        description="Arxivə göndərilmiş qeydlər. Buradan geri qaytarmaq və ya həmişəlik silmək olar."
      />

      <div className="-mx-1 overflow-x-auto px-1">
        <div className="flex w-max gap-1.5">
          {SECTIONS.map((section) => (
            <button
              key={section.key}
              onClick={() => setActive(section)}
              className={cn(
                "whitespace-nowrap rounded-md border px-3 py-1.5 text-sm transition-colors",
                section.key === active.key
                  ? "border-primary bg-primary-muted font-medium text-primary"
                  : "border-border text-muted-foreground hover:bg-muted hover:text-foreground",
              )}
            >
              {section.label}
            </button>
          ))}
        </div>
      </div>

      {/* Remounted per section so paging and sorting reset with the tab. */}
      <ArchiveList key={active.key} section={active} isAdmin={isAdmin} />
    </>
  );
}

function ArchiveList({ section, isAdmin }: { section: Section; isAdmin: boolean }) {
  const list = useResourceList<ArchivedRow>({
    key: `${section.key}:deleted`,
    fetcher: section.api.deleted as (p: Record<string, unknown>) => Promise<PagedResult<ArchivedRow>>,
    defaultSort: section.defaultSort,
  });

  const restore = useConfirm<ArchivedRow>();
  const destroy = useConfirm<ArchivedRow>();

  const invalidate = [`${section.key}:deleted`, section.key, "dashboard"];

  const toggle = useResourceMutation((row: ArchivedRow) => section.api.toggle(row.id), {
    invalidate,
  });

  const remove = useResourceMutation((row: ArchivedRow) => section.api.remove(row.id), {
    invalidate,
  });

  const columns: Array<Column<ArchivedRow>> = [
    {
      key: "title",
      header: "Qeyd",
      mobile: "title",
      cell: (row) => {
        const { title, subtitle } = section.describe(row);
        return (
          <div className="min-w-0">
            <p className="truncate font-medium">{title}</p>
            <p className="truncate text-xs text-muted-foreground">{subtitle}</p>
          </div>
        );
      },
    },
    {
      key: "createdAt",
      header: "Yaradılıb",
      sortable: true,
      mobile: "meta",
      cell: (row) => <span className="tnum">{date(row.createdAt)}</span>,
    },
    {
      key: "deletedAt",
      header: "Arxivə göndərilib",
      mobile: "trailing",
      cell: (row) => (
        <span className="tnum text-muted-foreground">{date(row.deletedAt)}</span>
      ),
    },
  ];

  return (
    <>
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
        emptyTitle={`Arxivdə ${section.label.toLowerCase()} yoxdur`}
        emptyDescription="Arxivə göndərilən qeydlər burada görünəcək."
        actions={(row) => (
          <RowActions>
            <MenuItem onSelect={() => restore.ask(row)}>
              <ArchiveRestore />
              Geri qaytar
            </MenuItem>

            {isAdmin && (
              <>
                <MenuSeparator />
                <MenuItem danger onSelect={() => destroy.ask(row)}>
                  <Trash2 />
                  Həmişəlik sil
                </MenuItem>
              </>
            )}
          </RowActions>
        )}
      />

      {!isAdmin && (
        <p className="px-1 text-xs text-muted-foreground">
          Həmişəlik silmək yalnız administrator hesabı ilə mümkündür.
        </p>
      )}

      <Confirm
        open={restore.open}
        onOpenChange={restore.onOpenChange}
        title="Arxivdən geri qaytar"
        description={
          restore.target
            ? `"${section.describe(restore.target).title}" yenidən aktiv siyahıya qayıdacaq.` +
              (section.cascades
                ? " Onunla birlikdə arxivlənmiş əlaqəli qeydlər də qayıdacaq — ayrıca arxivə göndərilənlər isə arxivdə qalacaq."
                : "")
            : ""
        }
        confirmLabel="Geri qaytar"
        onConfirm={() => toggle.mutateAsync(restore.target!)}
      />

      <Confirm
        open={destroy.open}
        onOpenChange={destroy.onOpenChange}
        title="Həmişəlik sil"
        description={
          destroy.target
            ? `"${section.describe(destroy.target).title}" bazadan tamamilə silinəcək və geri qaytarıla bilməz.` +
              (section.cascades
                ? " Əlaqəli qeydləri varsa, server bu əməliyyatı rədd edəcək."
                : "")
            : ""
        }
        confirmLabel="Həmişəlik sil"
        danger
        onConfirm={() => remove.mutateAsync(destroy.target!)}
      />
    </>
  );
}

