import * as RadixTabs from "@radix-ui/react-tabs";
import { cn } from "@/lib/cn";

export const Tabs = RadixTabs.Root;

export function TabsList({ className, ...props }: RadixTabs.TabsListProps) {
  return (
    <RadixTabs.List
      className={cn(
        // Scrolls sideways rather than wrapping — a vehicle detail page has
        // seven tabs, and wrapping them shifts the content below on every load.
        "-mb-px flex gap-1 overflow-x-auto border-b border-border",
        "[scrollbar-width:none] [&::-webkit-scrollbar]:hidden",
        className,
      )}
      {...props}
    />
  );
}

export function TabsTrigger({ className, ...props }: RadixTabs.TabsTriggerProps) {
  return (
    <RadixTabs.Trigger
      className={cn(
        "relative whitespace-nowrap border-b-2 border-transparent px-3 py-2 text-sm font-medium",
        "text-muted-foreground transition-colors hover:text-foreground",
        "data-[state=active]:border-primary data-[state=active]:text-foreground",
        className,
      )}
      {...props}
    />
  );
}

export function TabsContent({ className, ...props }: RadixTabs.TabsContentProps) {
  return <RadixTabs.Content className={cn("pt-5 outline-none", className)} {...props} />;
}
