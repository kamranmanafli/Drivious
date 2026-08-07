import { ChevronLeft, ChevronRight } from "lucide-react";
import { cn } from "@/lib/cn";
import { number } from "@/lib/format";
import type { PagedResult } from "@/api/types";

interface PaginationProps {
  result: PagedResult<unknown>;
  onPage: (page: number) => void;
}

/** Builds "1 … 4 5 6 … 20" — never more than seven slots, however many pages. */
function pageWindow(current: number, total: number): Array<number | "gap"> {
  if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1);

  const pages: Array<number | "gap"> = [1];

  const from = Math.max(2, current - 1);
  const to = Math.min(total - 1, current + 1);

  if (from > 2) pages.push("gap");
  for (let page = from; page <= to; page++) pages.push(page);
  if (to < total - 1) pages.push("gap");

  pages.push(total);

  return pages;
}

export function Pagination({ result, onPage }: PaginationProps) {
  const { page, pageSize, totalCount, totalPages, hasPrevious, hasNext } = result;

  if (totalCount === 0) return null;

  const first = (page - 1) * pageSize + 1;
  const last = Math.min(page * pageSize, totalCount);

  return (
    <div className="flex flex-wrap items-center justify-between gap-3 px-1">
      <p className="text-xs text-muted-foreground tnum">
        {number(totalCount)} nəticədən {number(first)}–{number(last)}
      </p>

      {totalPages > 1 && (
        <nav className="flex items-center gap-1" aria-label="Səhifələr">
          <button
            onClick={() => onPage(page - 1)}
            disabled={!hasPrevious}
            aria-label="Əvvəlki səhifə"
            className="flex size-8 items-center justify-center rounded-md border border-border text-muted-foreground hover:bg-muted disabled:pointer-events-none disabled:opacity-40"
          >
            <ChevronLeft className="size-4" />
          </button>

          {pageWindow(page, totalPages).map((slot, index) =>
            slot === "gap" ? (
              <span key={`gap-${index}`} className="px-1 text-xs text-muted-foreground">
                …
              </span>
            ) : (
              <button
                key={slot}
                onClick={() => onPage(slot)}
                aria-current={slot === page ? "page" : undefined}
                className={cn(
                  "h-8 min-w-8 rounded-md px-2 text-xs font-medium tnum",
                  slot === page
                    ? "bg-primary text-primary-foreground"
                    : "border border-border text-muted-foreground hover:bg-muted",
                )}
              >
                {slot}
              </button>
            ),
          )}

          <button
            onClick={() => onPage(page + 1)}
            disabled={!hasNext}
            aria-label="Növbəti səhifə"
            className="flex size-8 items-center justify-center rounded-md border border-border text-muted-foreground hover:bg-muted disabled:pointer-events-none disabled:opacity-40"
          >
            <ChevronRight className="size-4" />
          </button>
        </nav>
      )}
    </div>
  );
}
