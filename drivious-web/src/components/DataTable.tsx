import { ArrowDown, ArrowUp, ChevronsUpDown, Inbox } from "lucide-react";
import { cn } from "@/lib/cn";
import { EmptyState, ErrorState, Skeleton } from "@/ui";
import { errorMessage } from "@/api/client";
import type { PagedResult } from "@/api/types";
import { Pagination } from "./Pagination";

export interface Column<T> {
  /** Doubles as the API's `sortBy` value when `sortable` is set. */
  key: string;
  header: string;
  cell: (row: T) => React.ReactNode;
  sortable?: boolean;
  align?: "left" | "right";
  /** Applied to the cell, e.g. to stop a description column from stretching. */
  className?: string;
  headerClassName?: string;
  /**
   * Where this column lands in the mobile card. One definition drives both
   * layouts, so a column added to the table cannot be forgotten on the phone.
   *   title     — the headline
   *   subtitle  — under the headline
   *   trailing  — right-hand side, usually the amount or the status
   *   meta      — the labelled grid below
   *   hidden    — desktop only
   */
  mobile?: "title" | "subtitle" | "trailing" | "meta" | "hidden";
}

interface DataTableProps<T> {
  columns: Array<Column<T>>;
  result?: PagedResult<T>;
  isLoading: boolean;
  isFetching?: boolean;
  error?: unknown;
  onRetry?: () => void;
  sortBy: string;
  descending: boolean;
  onSort: (field: string) => void;
  onPage: (page: number) => void;
  onRowClick?: (row: T) => void;
  /** Rendered at the end of every row — a menu, usually. */
  actions?: (row: T) => React.ReactNode;
  rowKey: (row: T) => string;
  emptyTitle?: string;
  emptyDescription?: string;
  emptyAction?: React.ReactNode;
}

export function DataTable<T>({
  columns,
  result,
  isLoading,
  isFetching,
  error,
  onRetry,
  sortBy,
  descending,
  onSort,
  onPage,
  onRowClick,
  actions,
  rowKey,
  emptyTitle = "Hələ məlumat yoxdur",
  emptyDescription,
  emptyAction,
}: DataTableProps<T>) {
  if (error) {
    return (
      <div className="rounded-lg border border-border bg-surface">
        <ErrorState message={errorMessage(error)} onRetry={onRetry} />
      </div>
    );
  }

  if (isLoading) return <TableSkeleton columns={columns.length} />;

  const rows = result?.items ?? [];

  if (rows.length === 0) {
    return (
      <div className="rounded-lg border border-border bg-surface">
        <EmptyState
          icon={<Inbox />}
          title={emptyTitle}
          description={emptyDescription}
          action={emptyAction}
        />
      </div>
    );
  }

  const desktop = columns.filter((c) => c.mobile !== "hidden" || true);
  const title = columns.find((c) => c.mobile === "title") ?? columns[0];
  const subtitle = columns.find((c) => c.mobile === "subtitle");
  const trailing = columns.find((c) => c.mobile === "trailing");
  const meta = columns.filter((c) => c.mobile === "meta");

  return (
    <div className={cn("space-y-3 transition-opacity", isFetching && "opacity-60")}>
      {/* ── Desktop ─────────────────────────────────────────────────────── */}
      <div className="hidden overflow-x-auto rounded-lg border border-border bg-surface md:block">
        <table className="w-full border-collapse text-sm">
          <thead>
            <tr className="border-b border-border bg-surface-raised">
              {desktop.map((column) => (
                <th
                  key={column.key}
                  scope="col"
                  className={cn(
                    "px-3 py-2.5 text-xs font-medium text-muted-foreground",
                    column.align === "right" ? "text-right" : "text-left",
                    column.headerClassName,
                  )}
                >
                  {column.sortable ? (
                    <button
                      onClick={() => onSort(column.key)}
                      className={cn(
                        "inline-flex items-center gap-1 rounded hover:text-foreground",
                        column.align === "right" && "flex-row-reverse",
                      )}
                    >
                      {column.header}
                      {sortBy === column.key ? (
                        descending ? (
                          <ArrowDown className="size-3 text-primary" />
                        ) : (
                          <ArrowUp className="size-3 text-primary" />
                        )
                      ) : (
                        <ChevronsUpDown className="size-3 opacity-40" />
                      )}
                    </button>
                  ) : (
                    column.header
                  )}
                </th>
              ))}
              {actions && <th className="w-10 px-3 py-2.5" />}
            </tr>
          </thead>

          <tbody>
            {rows.map((row) => (
              <tr
                key={rowKey(row)}
                onClick={onRowClick ? () => onRowClick(row) : undefined}
                className={cn(
                  "border-b border-border last:border-0",
                  onRowClick && "cursor-pointer hover:bg-surface-raised",
                )}
              >
                {desktop.map((column) => (
                  <td
                    key={column.key}
                    className={cn(
                      "px-3 py-2.5 align-middle",
                      column.align === "right" && "text-right",
                      column.className,
                    )}
                  >
                    {column.cell(row)}
                  </td>
                ))}
                {actions && (
                  <td
                    className="px-3 py-2.5 text-right"
                    onClick={(event) => event.stopPropagation()}
                  >
                    {actions(row)}
                  </td>
                )}
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {/* ── Mobile ──────────────────────────────────────────────────────── */}
      <ul className="space-y-2 md:hidden">
        {rows.map((row) => (
          <li key={rowKey(row)}>
            <div
              onClick={onRowClick ? () => onRowClick(row) : undefined}
              className={cn(
                "rounded-lg border border-border bg-surface p-3",
                onRowClick && "cursor-pointer active:bg-surface-raised",
              )}
            >
              <div className="flex items-start justify-between gap-3">
                <div className="min-w-0 flex-1">
                  <div className="truncate text-sm font-medium">{title.cell(row)}</div>
                  {subtitle && (
                    <div className="mt-0.5 truncate text-xs text-muted-foreground">
                      {subtitle.cell(row)}
                    </div>
                  )}
                </div>

                <div className="flex shrink-0 items-center gap-1.5">
                  {trailing && <div className="text-right text-sm">{trailing.cell(row)}</div>}
                  {actions && (
                    <div onClick={(event) => event.stopPropagation()}>{actions(row)}</div>
                  )}
                </div>
              </div>

              {meta.length > 0 && (
                <dl className="mt-3 grid grid-cols-2 gap-x-3 gap-y-2 border-t border-border pt-3">
                  {meta.map((column) => (
                    <div key={column.key} className="min-w-0">
                      <dt className="text-[11px] text-muted-foreground">{column.header}</dt>
                      <dd className="truncate text-xs">{column.cell(row)}</dd>
                    </div>
                  ))}
                </dl>
              )}
            </div>
          </li>
        ))}
      </ul>

      {result && <Pagination result={result} onPage={onPage} />}
    </div>
  );
}

function TableSkeleton({ columns }: { columns: number }) {
  return (
    <div className="overflow-hidden rounded-lg border border-border bg-surface">
      <div className="hidden border-b border-border bg-surface-raised px-3 py-2.5 md:block">
        <Skeleton className="h-3.5 w-32" />
      </div>

      <div className="divide-y divide-border">
        {Array.from({ length: 8 }, (_, row) => (
          <div key={row} className="flex items-center gap-3 px-3 py-3">
            {Array.from({ length: Math.min(columns, 5) }, (_, cell) => (
              <Skeleton
                key={cell}
                className="h-4 flex-1"
                // Uneven widths so it reads as content loading rather than a grid.
              />
            ))}
          </div>
        ))}
      </div>
    </div>
  );
}
