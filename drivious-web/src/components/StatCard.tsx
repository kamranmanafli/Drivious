import { Link } from "react-router-dom";
import { cn } from "@/lib/cn";
import { Skeleton } from "@/ui";

interface StatCardProps {
  label: string;
  value: string;
  /** Second line — a breakdown, a share, or a comparison. */
  hint?: string;
  icon?: React.ReactNode;
  tone?: "neutral" | "success" | "warning" | "danger";
  to?: string;
  loading?: boolean;
}

const TONES = {
  neutral: "text-muted-foreground",
  success: "text-success",
  warning: "text-warning",
  danger: "text-danger",
} as const;

export function StatCard({ label, value, hint, icon, tone = "neutral", to, loading }: StatCardProps) {
  const body = (
    <>
      <div className="flex items-start justify-between gap-2">
        <p className="text-xs font-medium text-muted-foreground">{label}</p>
        {icon && (
          <span className={cn("[&_svg]:size-4", TONES[tone])} aria-hidden>
            {icon}
          </span>
        )}
      </div>

      {loading ? (
        <Skeleton className="mt-2 h-7 w-24" />
      ) : (
        <p className="mt-1.5 text-2xl font-semibold tracking-tight tnum">{value}</p>
      )}

      {hint && !loading && (
        <p className={cn("mt-1 text-xs", TONES[tone])}>{hint}</p>
      )}
    </>
  );

  const className = cn(
    "rounded-lg border border-border bg-surface p-4",
    to && "transition-colors hover:border-border-strong hover:bg-surface-raised",
  );

  return to ? (
    <Link to={to} className={cn(className, "block")}>
      {body}
    </Link>
  ) : (
    <div className={className}>{body}</div>
  );
}
