import { BellRing, CheckCheck, Info, TriangleAlert, XCircle } from "lucide-react";
import { notifications as notificationsApi } from "@/api/endpoints";
import { NotificationType, type Notification } from "@/api/types";
import { notificationTypes } from "@/lib/enums";
import { cn } from "@/lib/cn";
import { dateTime, relativeDays } from "@/lib/format";
import { errorMessage } from "@/api/client";
import { useResourceList } from "@/components/useResourceList";
import { Pagination } from "@/components/Pagination";
import { Badge, Card, EmptyState, ErrorState, NativeSelect, Skeleton } from "@/ui";
import { EnumOptions } from "@/components/pickers";

const ICONS = {
  [NotificationType.Info]: Info,
  [NotificationType.Warning]: TriangleAlert,
  [NotificationType.Success]: CheckCheck,
  [NotificationType.Error]: XCircle,
} as const;

const TONES = {
  [NotificationType.Info]: "bg-info-muted text-info",
  [NotificationType.Warning]: "bg-warning-muted text-warning",
  [NotificationType.Success]: "bg-success-muted text-success",
  [NotificationType.Error]: "bg-danger-muted text-danger",
} as const;

/**
 * Read-only feed. A driver can list notifications but not change them — marking
 * one read is a Manager action on the API, and it would apply to everyone
 * anyway, since notifications are not addressed to a particular account.
 */
export function DriverNotificationsPage() {
  const list = useResourceList<Notification>({
    key: "notifications",
    fetcher: notificationsApi.list,
    defaultSort: "notificationDate",
    pageSize: 20,
    filters: ["type"],
  });

  return (
    <>
      <div className="flex flex-wrap items-end justify-between gap-3">
        <div>
          <h1 className="text-xl font-semibold tracking-tight">Bildirişlər</h1>
          <p className="mt-0.5 text-sm text-muted-foreground">
            Filo üzrə xəbərdarlıqlar və bitmə tarixləri.
          </p>
        </div>

        <NativeSelect
          value={list.value("type")}
          onChange={(event) => list.setParam("type", event.target.value)}
          className="w-36"
        >
          <EnumOptions entries={notificationTypes.list} placeholder="Bütün növlər" />
        </NativeSelect>
      </div>

      {list.error ? (
        <Card>
          <ErrorState message={errorMessage(list.error)} onRetry={() => void list.refetch()} />
        </Card>
      ) : list.isLoading ? (
        <div className="space-y-2">
          {Array.from({ length: 5 }, (_, i) => (
            <Skeleton key={i} className="h-20 w-full" />
          ))}
        </div>
      ) : list.items.length === 0 ? (
        <Card>
          <EmptyState
            icon={<BellRing />}
            title="Bildiriş yoxdur"
            description="Bitmə tarixləri yaxınlaşdıqca xəbərdarlıqlar burada görünəcək."
          />
        </Card>
      ) : (
        <div className={cn("space-y-2", list.isFetching && "opacity-60")}>
          {list.items.map((item) => {
            const Icon = ICONS[item.type] ?? Info;

            return (
              <article key={item.id} className="flex gap-3 rounded-lg border border-border bg-surface p-3.5">
                <span
                  className={cn(
                    "flex size-8 shrink-0 items-center justify-center rounded-full",
                    TONES[item.type] ?? TONES[NotificationType.Info],
                  )}
                >
                  <Icon className="size-4" />
                </span>

                <div className="min-w-0 flex-1">
                  <div className="flex flex-wrap items-center gap-2">
                    <h2 className="text-sm font-medium">{item.title}</h2>
                    <Badge tone={notificationTypes.tone(item.type)}>
                      {notificationTypes.label(item.type)}
                    </Badge>
                  </div>

                  <p className="mt-1 text-sm leading-relaxed text-muted-foreground">
                    {item.message}
                  </p>

                  <p className="mt-1.5 text-xs text-muted-foreground">
                    {dateTime(item.notificationDate)} · {relativeDays(item.notificationDate)}
                  </p>
                </div>
              </article>
            );
          })}

          {list.result && <Pagination result={list.result} onPage={list.setPage} />}
        </div>
      )}
    </>
  );
}
