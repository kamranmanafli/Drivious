import * as RadixDialog from "@radix-ui/react-dialog";
import { X } from "lucide-react";
import { cn } from "@/lib/cn";

export const DialogRoot = RadixDialog.Root;
export const DialogTrigger = RadixDialog.Trigger;
export const DialogClose = RadixDialog.Close;

interface DialogProps {
  title: string;
  description?: string;
  /** Wider shell for forms with two columns. */
  size?: "sm" | "md" | "lg";
  children: React.ReactNode;
  footer?: React.ReactNode;
}

/**
 * A centred panel on a wide screen and a bottom sheet on a narrow one — the
 * same component either way, because a dialog anchored to the top of a phone
 * puts its actions out of thumb reach.
 */
export function DialogContent({ title, description, size = "md", children, footer }: DialogProps) {
  const width = { sm: "sm:max-w-md", md: "sm:max-w-2xl", lg: "sm:max-w-4xl" }[size];

  return (
    <RadixDialog.Portal>
      <RadixDialog.Overlay className="fixed inset-0 z-50 bg-black/40 backdrop-blur-[2px] data-[state=open]:animate-in data-[state=open]:fade-in" />

      <RadixDialog.Content
        className={cn(
          "fixed z-50 flex flex-col border border-border bg-surface",
          // Phone: full-width sheet pinned to the bottom.
          "inset-x-0 bottom-0 max-h-[92dvh] rounded-t-xl",
          // Desktop: centred panel.
          "sm:inset-x-auto sm:bottom-auto sm:top-1/2 sm:left-1/2 sm:max-h-[86dvh] sm:w-full",
          "sm:-translate-x-1/2 sm:-translate-y-1/2 sm:rounded-lg",
          width,
        )}
      >
        <div className="flex items-start justify-between gap-4 border-b border-border px-5 py-4">
          <div className="min-w-0">
            <RadixDialog.Title className="text-base font-semibold tracking-tight">
              {title}
            </RadixDialog.Title>
            {description && (
              <RadixDialog.Description className="mt-0.5 text-xs text-muted-foreground">
                {description}
              </RadixDialog.Description>
            )}
          </div>

          <RadixDialog.Close
            className="-mr-1 -mt-1 rounded-md p-1.5 text-muted-foreground hover:bg-muted hover:text-foreground"
            aria-label="Bağla"
          >
            <X className="size-4" />
          </RadixDialog.Close>
        </div>

        <div className="min-h-0 flex-1 overflow-y-auto px-5 py-4">{children}</div>

        {footer && (
          <div className="flex flex-wrap items-center justify-end gap-2 border-t border-border px-5 py-3 pb-safe sm:pb-3">
            {footer}
          </div>
        )}
      </RadixDialog.Content>
    </RadixDialog.Portal>
  );
}
