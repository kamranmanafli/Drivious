import * as RadixMenu from "@radix-ui/react-dropdown-menu";
import { cn } from "@/lib/cn";

export const Menu = RadixMenu.Root;
export const MenuTrigger = RadixMenu.Trigger;

export function MenuContent({
  className,
  align = "end",
  children,
}: {
  className?: string;
  align?: "start" | "center" | "end";
  children: React.ReactNode;
}) {
  return (
    <RadixMenu.Portal>
      <RadixMenu.Content
        align={align}
        sideOffset={6}
        className={cn(
          "z-50 min-w-44 overflow-hidden rounded-md border border-border bg-surface p-1",
          "shadow-lg shadow-black/5 dark:shadow-black/40",
          className,
        )}
      >
        {children}
      </RadixMenu.Content>
    </RadixMenu.Portal>
  );
}

export function MenuItem({
  className,
  danger,
  ...props
}: RadixMenu.DropdownMenuItemProps & { danger?: boolean }) {
  return (
    <RadixMenu.Item
      className={cn(
        "flex cursor-pointer select-none items-center gap-2 rounded-sm px-2 py-1.5 text-sm outline-none",
        "data-[highlighted]:bg-muted data-[disabled]:pointer-events-none data-[disabled]:opacity-50",
        "[&_svg]:size-4 [&_svg]:shrink-0 [&_svg]:text-muted-foreground",
        danger && "text-danger data-[highlighted]:bg-danger-muted [&_svg]:text-danger",
        className,
      )}
      {...props}
    />
  );
}

export function MenuSeparator() {
  return <RadixMenu.Separator className="my-1 h-px bg-border" />;
}

export function MenuLabel({ children }: { children: React.ReactNode }) {
  return (
    <RadixMenu.Label className="px-2 py-1.5 text-xs font-medium text-muted-foreground">
      {children}
    </RadixMenu.Label>
  );
}
