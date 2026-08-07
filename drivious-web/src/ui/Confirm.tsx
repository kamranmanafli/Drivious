import { useState } from "react";
import { DialogContent, DialogRoot } from "./Dialog";
import { Button } from "./Button";

interface ConfirmProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  title: string;
  description: string;
  confirmLabel?: string;
  danger?: boolean;
  onConfirm: () => Promise<unknown> | void;
}

/**
 * A blocking yes/no. Kept as its own component because the destructive actions
 * here — permanent delete, archiving a vehicle with all its history — should
 * never be one stray click away.
 */
export function Confirm({
  open,
  onOpenChange,
  title,
  description,
  confirmLabel = "Təsdiqlə",
  danger,
  onConfirm,
}: ConfirmProps) {
  const [busy, setBusy] = useState(false);

  async function handle() {
    setBusy(true);
    try {
      await onConfirm();
      onOpenChange(false);
    } finally {
      setBusy(false);
    }
  }

  return (
    <DialogRoot open={open} onOpenChange={busy ? undefined : onOpenChange}>
      <DialogContent
        title={title}
        size="sm"
        footer={
          <>
            <Button variant="ghost" onClick={() => onOpenChange(false)} disabled={busy}>
              Ləğv et
            </Button>
            <Button variant={danger ? "danger" : "primary"} onClick={handle} loading={busy}>
              {confirmLabel}
            </Button>
          </>
        }
      >
        <p className="text-sm leading-relaxed text-muted-foreground">{description}</p>
      </DialogContent>
    </DialogRoot>
  );
}

/** Drives a Confirm from a list row without a piece of state per action. */
export function useConfirm<T>() {
  const [target, setTarget] = useState<T | null>(null);

  return {
    target,
    open: Boolean(target),
    ask: setTarget,
    close: () => setTarget(null),
    onOpenChange: (next: boolean) => {
      if (!next) setTarget(null);
    },
  };
}
