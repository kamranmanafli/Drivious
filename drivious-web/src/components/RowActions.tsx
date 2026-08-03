import { MoreHorizontal } from "lucide-react";
import { Menu, MenuContent, MenuTrigger } from "@/ui";

/** The "…" button every list row ends with. */
export function RowActions({ children }: { children: React.ReactNode }) {
  return (
    <Menu>
      <MenuTrigger
        aria-label="Əməliyyatlar"
        className="flex size-8 items-center justify-center rounded-md text-muted-foreground hover:bg-muted hover:text-foreground data-[state=open]:bg-muted"
      >
        <MoreHorizontal className="size-4" />
      </MenuTrigger>

      <MenuContent>{children}</MenuContent>
    </Menu>
  );
}
