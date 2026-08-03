import { ChevronLeft } from "lucide-react";
import { Link } from "react-router-dom";
import { cn } from "@/lib/cn";

interface PageHeaderProps {
  title: string;
  description?: string;
  /** Renders a back link above the title. */
  back?: { to: string; label: string };
  actions?: React.ReactNode;
  className?: string;
}

export function PageHeader({ title, description, back, actions, className }: PageHeaderProps) {
  return (
    <header className={cn("flex flex-wrap items-end justify-between gap-3", className)}>
      <div className="min-w-0">
        {back && (
          <Link
            to={back.to}
            className="mb-1.5 -ml-1 inline-flex items-center gap-1 rounded text-xs text-muted-foreground hover:text-foreground"
          >
            <ChevronLeft className="size-3.5" />
            {back.label}
          </Link>
        )}

        <h1 className="truncate text-xl font-semibold tracking-tight">{title}</h1>

        {description && (
          <p className="mt-1 text-sm text-muted-foreground">{description}</p>
        )}
      </div>

      {actions && <div className="flex shrink-0 items-center gap-2">{actions}</div>}
    </header>
  );
}
