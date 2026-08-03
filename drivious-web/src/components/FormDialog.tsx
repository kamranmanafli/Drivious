import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import { errorMessage } from "@/api/client";
import { Button, DialogContent, DialogRoot } from "@/ui";

interface FormDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  title: string;
  description?: string;
  size?: "sm" | "md" | "lg";
  submitLabel?: string;
  submitting?: boolean;
  onSubmit: () => void;
  children: React.ReactNode;
}

/** Dialog + form + footer, so the submit wiring is written once. */
export function FormDialog({
  open,
  onOpenChange,
  title,
  description,
  size = "md",
  submitLabel = "Yadda saxla",
  submitting,
  onSubmit,
  children,
}: FormDialogProps) {
  return (
    <DialogRoot open={open} onOpenChange={submitting ? undefined : onOpenChange}>
      <DialogContent
        title={title}
        description={description}
        size={size}
        footer={
          <>
            <Button variant="ghost" onClick={() => onOpenChange(false)} disabled={submitting}>
              Ləğv et
            </Button>
            <Button
              variant="primary"
              type="submit"
              form="dialog-form"
              loading={submitting}
              onClick={onSubmit}
            >
              {submitLabel}
            </Button>
          </>
        }
      >
        <form
          id="dialog-form"
          onSubmit={(event) => {
            event.preventDefault();
            onSubmit();
          }}
          className="grid gap-4 sm:grid-cols-2"
          noValidate
        >
          {children}
        </form>
      </DialogContent>
    </DialogRoot>
  );
}

/**
 * A write against the API: reports the outcome, refreshes the affected lists,
 * and hands the caller a `submitting` flag.
 *
 * Every endpoint here returns its confirmation as a translated string, so the
 * success toast is the API's own words rather than a generic "Saved".
 */
export function useResourceMutation<TArgs>(
  action: (args: TArgs) => Promise<string>,
  {
    invalidate,
    onSuccess,
  }: {
    /** Query key roots to refetch — usually the resource path plus "dashboard". */
    invalidate: string[];
    onSuccess?: () => void;
  },
) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: action,
    onSuccess: (message) => {
      toast.success(message);
      for (const key of invalidate) {
        void queryClient.invalidateQueries({ queryKey: [key] });
      }
      onSuccess?.();
    },
    onError: (error) => toast.error(errorMessage(error)),
  });
}
