import { useState } from "react";
import {
  AlertTriangle,
  Archive,
  BellRing,
  CheckCheck,
  Info,
  Pencil,
  RefreshCw,
  Trash2,
  TriangleAlert,
  XCircle,
} from "lucide-react";
import { notifications as notificationsApi, type NotificationInput } from "@/api/endpoints";
import { NotificationType, type Notification } from "@/api/types";
import { notificationTypes } from "@/lib/enums";
import { cn } from "@/lib/cn";
import { dateTime, relativeDays, toDateInput } from "@/lib/format";
import { useAuth } from "@/auth/AuthContext";
import { useResourceList } from "@/components/useResourceList";
import { PageHeader } from "@/components/PageHeader";
import { FilterControl, Toolbar } from "@/components/Toolbar";
import { RowActions } from "@/components/RowActions";
import { Pagination } from "@/components/Pagination";
import { FormDialog, useResourceMutation } from "@/components/FormDialog";
import { EnumOptions } from "@/components/pickers";
import { errorMessage } from "@/api/client";
import {
  Badge,
  Button,
  Confirm,
  EmptyState,
  ErrorState,
  Field,
  Input,
  MenuItem,
  MenuSeparator,
  NativeSelect,
  Skeleton,
  Textarea,
  useConfirm,
} from "@/ui";

const FILTERS = ["type", "isRead", "from", "to"] as const;

const ICONS = {
  [NotificationType.Info]: Info,
  [NotificationType.Warning]: TriangleAlert,
  [NotificationType.Success]: CheckCheck,
  [NotificationType.Error]: XCircle,
} as const;

const ICON_TONES = {
  [NotificationType.Info]: "bg-info-muted text-info",
  [NotificationType.Warning]: "bg-warning-muted text-warning",
  [NotificationType.Success]: "bg-success-muted text-success",
  [NotificationType.Error]: "bg-danger-muted text-danger",
} as const;

/**
 * Rendered as a feed rather than a table: a notification is a sentence, and a
 * sentence in a table cell is either truncated or blows the row height out.
 */
export function NotificationsPage() {
  const { canManage, isAdmin } = useAuth();

  const list = useResourceList<Notification>({
    key: "notifications",
    fetcher: notificationsApi.list,
    defaultSort: "notificationDate",
    pageSize: 25,
    filters: FILTERS,
  });

  const [editing, setEditing] = useState<Notification | "new" | null>(null);
  const archive = useConfirm<Notification>();
  const destroy = useConfirm<Notification>();

  const invalidate = ["notifications", "dashboard"];

  const markRead = useResourceMutation(
    ({ id, isRead }: { id: string; isRead: boolean }) => notificationsApi.markRead(id, isRead),
    { invalidate },
  );

  const scan = useResourceMutation(() => notificationsApi.scan(), { invalidate });
  const toggle = useResourceMutation((row: Notification) => notificationsApi.toggle(row.id), {
    invalidate,
  });
  const remove = useResourceMutation((row: Notification) => notificationsApi.remove(row.id), {
    invalidate,
  });

  return (
    <>
      <PageHeader
        title="Bildirişlər"
        description="Sığorta, vəsiqə, servis və sənəd tarixləri üzrə avtomatik xəbərdarlıqlar."
        actions={
          canManage && (
            <>
              <Button
                variant="secondary"
                onClick={() => scan.mutate(undefined as never)}
                loading={scan.isPending}
              >
                <RefreshCw />
                <span className="hidden sm:inline">Skan et</span>
              </Button>

              <Button variant="primary" onClick={() => setEditing("new")}>
                <BellRing />
                <span className="hidden sm:inline">Yeni bildiriş</span>
              </Button>
            </>
          )
        }
      />

      <Toolbar
        search={list.value("search")}
        onSearch={(value) => list.setParam("search", value)}
        searchPlaceholder="Başlıq və ya mətn…"
        activeFilterCount={list.activeFilterCount}
        onClear={list.clearFilters}
        filters={
          <>
            <FilterControl label="Növ">
              <NativeSelect
                value={list.value("type")}
                onChange={(event) => list.setParam("type", event.target.value)}
              >
                <EnumOptions entries={notificationTypes.list} placeholder="Bütün növlər" />
              </NativeSelect>
            </FilterControl>

            <FilterControl label="Oxunub">
              <NativeSelect
                value={list.value("isRead")}
                onChange={(event) => list.setParam("isRead", event.target.value)}
              >
                <option value="">Hamısı</option>
                <option value="false">Oxunmamış</option>
                <option value="true">Oxunmuş</option>
              </NativeSelect>
            </FilterControl>

            <FilterControl label="Tarixdən">
              <Input
                type="date"
                value={list.value("from")}
                onChange={(event) => list.setParam("from", event.target.value)}
              />
            </FilterControl>

            <FilterControl label="Tarixə">
              <Input
                type="date"
                value={list.value("to")}
                onChange={(event) => list.setParam("to", event.target.value)}
              />
            </FilterControl>
          </>
        }
      />

      {list.error ? (
        <div className="rounded-lg border border-border bg-surface">
          <ErrorState message={errorMessage(list.error)} onRetry={() => void list.refetch()} />
        </div>
      ) : list.isLoading ? (
        <div className="space-y-2">
          {Array.from({ length: 6 }, (_, i) => (
            <Skeleton key={i} className="h-20 w-full" />
          ))}
        </div>
      ) : list.items.length === 0 ? (
        <div className="rounded-lg border border-border bg-surface">
          <EmptyState
            icon={<BellRing />}
            title={list.hasFilters ? "Uyğun bildiriş tapılmadı" : "Bildiriş yoxdur"}
            description={
              list.hasFilters
                ? "Filtrləri dəyişib yenidən cəhd edin."
                : "Bitmə tarixləri yaxınlaşdıqca xəbərdarlıqlar burada görünəcək."
            }
          />
        </div>
      ) : (
        <div className={cn("space-y-2 transition-opacity", list.isFetching && "opacity-60")}>
          {list.items.map((item) => {
            const Icon = ICONS[item.type] ?? Info;

            return (
              <article
                key={item.id}
                className={cn(
                  "flex gap-3 rounded-lg border bg-surface p-3.5",
                  item.isRead ? "border-border" : "border-border-strong",
                )}
              >
                <span
                  className={cn(
                    "flex size-8 shrink-0 items-center justify-center rounded-full",
                    ICON_TONES[item.type] ?? ICON_TONES[NotificationType.Info],
                  )}
                >
                  <Icon className="size-4" />
                </span>

                <div className="min-w-0 flex-1">
                  <div className="flex flex-wrap items-center gap-2">
                    <h3 className={cn("text-sm", item.isRead ? "font-medium" : "font-semibold")}>
                      {item.title}
                    </h3>

                    {!item.isRead && (
                      <span className="size-1.5 rounded-full bg-primary" aria-label="Oxunmamış" />
                    )}

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

                {canManage && (
                  <div className="shrink-0">
                    <RowActions>
                      <MenuItem
                        onSelect={() => markRead.mutate({ id: item.id, isRead: !item.isRead })}
                      >
                        <CheckCheck />
                        {item.isRead ? "Oxunmamış kimi işarələ" : "Oxundu kimi işarələ"}
                      </MenuItem>

                      <MenuItem onSelect={() => setEditing(item)}>
                        <Pencil />
                        Redaktə et
                      </MenuItem>

                      <MenuSeparator />

                      <MenuItem onSelect={() => archive.ask(item)}>
                        <Archive />
                        Arxivə göndər
                      </MenuItem>

                      {isAdmin && (
                        <MenuItem danger onSelect={() => destroy.ask(item)}>
                          <Trash2 />
                          Həmişəlik sil
                        </MenuItem>
                      )}
                    </RowActions>
                  </div>
                )}
              </article>
            );
          })}

          {list.result && <Pagination result={list.result} onPage={list.setPage} />}
        </div>
      )}

      {canManage && (
        <p className="flex items-start gap-2 px-1 text-xs text-muted-foreground">
          <AlertTriangle className="mt-px size-3.5 shrink-0" />
          Bildirişlər bütün istifadəçilər üçün ortaqdır — «oxundu» işarəsi hamıya təsir edir.
        </p>
      )}

      {editing && (
        <NotificationFormDialog
          notification={editing === "new" ? null : editing}
          onClose={() => setEditing(null)}
        />
      )}

      <Confirm
        open={archive.open}
        onOpenChange={archive.onOpenChange}
        title="Bildirişi arxivə göndər"
        description={`"${archive.target?.title}" arxivə göndəriləcək. Arxivdən geri qaytara bilərsiniz.`}
        confirmLabel="Arxivə göndər"
        onConfirm={() => toggle.mutateAsync(archive.target!)}
      />

      <Confirm
        open={destroy.open}
        onOpenChange={destroy.onOpenChange}
        title="Həmişəlik sil"
        description={
          `"${destroy.target?.title}" bazadan tamamilə silinəcək. ` +
          "Avtomatik yaradılmış xəbərdarlıq növbəti skanda yenidən yarana bilər."
        }
        confirmLabel="Həmişəlik sil"
        danger
        onConfirm={() => remove.mutateAsync(destroy.target!)}
      />
    </>
  );
}

function NotificationFormDialog({
  notification,
  onClose,
}: {
  notification: Notification | null;
  onClose: () => void;
}) {
  const isNew = notification === null;

  const [values, setValues] = useState({
    title: notification?.title ?? "",
    message: notification?.message ?? "",
    type: String(notification?.type ?? NotificationType.Info),
    isRead: String(notification?.isRead ?? false),
    notificationDate: toDateInput(notification?.notificationDate ?? new Date()),
  });

  const [errors, setErrors] = useState<Partial<Record<keyof typeof values, string>>>({});

  const save = useResourceMutation(
    (payload: NotificationInput | Partial<NotificationInput>) =>
      isNew
        ? notificationsApi.create(payload as NotificationInput)
        : notificationsApi.update(notification.id, payload),
    { invalidate: ["notifications", "dashboard"], onSuccess: onClose },
  );

  const set =
    (key: keyof typeof values) =>
    (event: React.ChangeEvent<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement>) =>
      setValues((previous) => ({ ...previous, [key]: event.target.value }));

  function submit() {
    const next: typeof errors = {};

    if (values.title.trim().length < 3) next.title = "Ən azı 3 simvol.";
    if (values.message.trim().length < 5) next.message = "Ən azı 5 simvol.";
    if (values.notificationDate > toDateInput(new Date())) {
      next.notificationDate = "Gələcək tarix ola bilməz.";
    }

    setErrors(next);
    if (Object.keys(next).length > 0) return;

    save.mutate({
      title: values.title.trim(),
      message: values.message.trim(),
      type: Number(values.type),
      isRead: values.isRead === "true",
      notificationDate: new Date(values.notificationDate).toISOString(),
    });
  }

  return (
    <FormDialog
      open
      onOpenChange={(open) => !open && onClose()}
      title={isNew ? "Yeni bildiriş" : "Bildirişi redaktə et"}
      description="Əl ilə yaradılan bildiriş avtomatik skanla təkrarlanmır."
      submitting={save.isPending}
      onSubmit={submit}
    >
      <Field label="Başlıq" required error={errors.title} className="sm:col-span-2">
        {(props) => (
          <Input {...props} value={values.title} onChange={set("title")} maxLength={100} />
        )}
      </Field>

      <Field label="Növ" required>
        {(props) => (
          <NativeSelect {...props} value={values.type} onChange={set("type")}>
            <EnumOptions entries={notificationTypes.list} />
          </NativeSelect>
        )}
      </Field>

      <Field label="Tarix" required error={errors.notificationDate}>
        {(props) => (
          <Input
            {...props}
            type="date"
            max={toDateInput(new Date())}
            value={values.notificationDate}
            onChange={set("notificationDate")}
          />
        )}
      </Field>

      <Field
        label="Mətn"
        required
        error={errors.message}
        hint={errors.message ? undefined : "5–1000 simvol"}
        className="sm:col-span-2"
      >
        {(props) => (
          <Textarea
            {...props}
            value={values.message}
            onChange={set("message")}
            maxLength={1000}
            className="min-h-28"
          />
        )}
      </Field>

      <Field label="Vəziyyət" className="sm:col-span-2">
        {(props) => (
          <NativeSelect {...props} value={values.isRead} onChange={set("isRead")}>
            <option value="false">Oxunmamış</option>
            <option value="true">Oxunmuş</option>
          </NativeSelect>
        )}
      </Field>
    </FormDialog>
  );
}
