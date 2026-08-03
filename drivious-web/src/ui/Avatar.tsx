import * as RadixAvatar from "@radix-ui/react-avatar";
import { cn } from "@/lib/cn";
import { initials } from "@/lib/format";

interface AvatarProps {
  src?: string | null;
  name?: string | null;
  className?: string;
  /** Square for vehicles, round for people. */
  shape?: "circle" | "square";
}

export function Avatar({ src, name, className, shape = "circle" }: AvatarProps) {
  return (
    <RadixAvatar.Root
      className={cn(
        "relative flex size-9 shrink-0 overflow-hidden bg-muted",
        shape === "circle" ? "rounded-full" : "rounded-md",
        className,
      )}
    >
      {src && (
        <RadixAvatar.Image src={src} alt={name ?? ""} className="size-full object-cover" />
      )}
      <RadixAvatar.Fallback
        // Only after the image has had a chance to load, so a slow network does
        // not flash the initials for every row on the page.
        delayMs={src ? 300 : 0}
        className="flex size-full items-center justify-center text-xs font-semibold text-muted-foreground"
      >
        {initials(name)}
      </RadixAvatar.Fallback>
    </RadixAvatar.Root>
  );
}
