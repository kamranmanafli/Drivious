import { useEffect, useState } from "react";
import { Search, SlidersHorizontal, X } from "lucide-react";
import { cn } from "@/lib/cn";
import { Badge, Button, DialogContent, DialogRoot, Input } from "@/ui";

interface ToolbarProps {
  search: string;
  onSearch: (value: string) => void;
  searchPlaceholder?: string;
  /** Filter controls. Inline on a wide screen, in a sheet on a narrow one. */
  filters?: React.ReactNode;
  activeFilterCount?: number;
  onClear?: () => void;
  actions?: React.ReactNode;
}

export function Toolbar({
  search,
  onSearch,
  searchPlaceholder = "Axtar…",
  filters,
  activeFilterCount = 0,
  onClear,
  actions,
}: ToolbarProps) {
  const [sheetOpen, setSheetOpen] = useState(false);

  // The search box is typed into; committing every keystroke to the URL would
  // fire a request per character and fight the cursor. 300ms is long enough to
  // finish a word and short enough not to feel laggy.
  const [draft, setDraft] = useState(search);

  useEffect(() => setDraft(search), [search]);

  useEffect(() => {
    if (draft === search) return;
    const timer = setTimeout(() => onSearch(draft), 300);
    return () => clearTimeout(timer);
  }, [draft, search, onSearch]);

  return (
    <div className="flex flex-wrap items-center gap-2">
      <div className="relative min-w-0 flex-1 sm:max-w-xs">
        <Search className="pointer-events-none absolute left-2.5 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
        <Input
          value={draft}
          onChange={(event) => setDraft(event.target.value)}
          placeholder={searchPlaceholder}
          className="pl-8 pr-8"
          type="search"
        />
        {draft && (
          <button
            onClick={() => setDraft("")}
            aria-label="Axtarışı təmizlə"
            className="absolute right-2 top-1/2 -translate-y-1/2 rounded p-0.5 text-muted-foreground hover:text-foreground"
          >
            <X className="size-3.5" />
          </button>
        )}
      </div>

      {filters && (
        <>
          {/* Inline on desktop */}
          <div className="hidden flex-wrap items-center gap-2 lg:flex">{filters}</div>

          {/* Sheet on mobile and tablet */}
          <Button
            variant="secondary"
            onClick={() => setSheetOpen(true)}
            className="lg:hidden"
          >
            <SlidersHorizontal />
            Filtrlər
            {activeFilterCount > 0 && (
              <Badge tone="primary" className="ml-0.5 px-1.5 py-0">
                {activeFilterCount}
              </Badge>
            )}
          </Button>

          <DialogRoot open={sheetOpen} onOpenChange={setSheetOpen}>
            <DialogContent
              title="Filtrlər"
              size="sm"
              footer={
                <>
                  {onClear && (
                    <Button
                      variant="ghost"
                      onClick={() => {
                        onClear();
                        setSheetOpen(false);
                      }}
                    >
                      Təmizlə
                    </Button>
                  )}
                  <Button variant="primary" onClick={() => setSheetOpen(false)}>
                    Tətbiq et
                  </Button>
                </>
              }
            >
              <div className="grid gap-4">{filters}</div>
            </DialogContent>
          </DialogRoot>
        </>
      )}

      {onClear && activeFilterCount > 0 && (
        <Button variant="ghost" size="sm" onClick={onClear} className="hidden lg:inline-flex">
          <X />
          Təmizlə
        </Button>
      )}

      {actions && <div className={cn("ml-auto flex items-center gap-2")}>{actions}</div>}
    </div>
  );
}

/** A labelled filter control sized for both the inline bar and the sheet. */
export function FilterControl({
  label,
  children,
  className,
}: {
  label: string;
  children: React.ReactNode;
  className?: string;
}) {
  return (
    <label className={cn("flex flex-col gap-1.5 lg:w-40", className)}>
      <span className="text-xs font-medium text-muted-foreground lg:sr-only">{label}</span>
      {children}
    </label>
  );
}
