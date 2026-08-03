import { useState } from "react";
import { Link2, Link2Off, ShieldCheck, UserCog } from "lucide-react";
import { auth as authApi } from "@/api/endpoints";
import { Role, type UserListItem } from "@/api/types";
import { roleLabels } from "@/lib/enums";
import { useResourceList } from "@/components/useResourceList";
import { DataTable, type Column } from "@/components/DataTable";
import { PageHeader } from "@/components/PageHeader";
import { FilterControl, Toolbar } from "@/components/Toolbar";
import { RowActions } from "@/components/RowActions";
import { FormDialog, useResourceMutation } from "@/components/FormDialog";
import { DriverPicker } from "@/components/pickers";
import { Avatar, Badge, Field, MenuItem, MenuSeparator, NativeSelect } from "@/ui";

const ROLE_TONES = {
  [Role.Admin]: "danger",
  [Role.Manager]: "primary",
  [Role.Driver]: "neutral",
} as const;

/**
 * Accounts and their roles.
 *
 * The user list comes from `GET /api/auths/users`, which was added to the API
 * for this screen — the original surface only offered assign-role and
 * link-driver by username, with no way to discover which accounts exist.
 */
export function UsersPage() {
  const list = useResourceList<UserListItem>({
    key: "users",
    fetcher: authApi.users,
    defaultSort: "userName",
    defaultDescending: false,
    filters: ["role"],
  });

  const [roleTarget, setRoleTarget] = useState<UserListItem | null>(null);
  const [linkTarget, setLinkTarget] = useState<UserListItem | null>(null);

  const unlink = useResourceMutation(
    (user: UserListItem) => authApi.linkDriver(user.userName, null),
    { invalidate: ["users"] },
  );

  const columns: Array<Column<UserListItem>> = [
    {
      key: "userName",
      header: "İstifadəçi",
      mobile: "title",
      cell: (user) => (
        <div className="flex items-center gap-2.5">
          <Avatar name={user.userName} className="size-8" />
          <div className="min-w-0">
            <p className="truncate font-medium">{user.userName}</p>
            <p className="truncate text-xs text-muted-foreground">{user.email}</p>
          </div>
        </div>
      ),
    },
    {
      key: "roles",
      header: "Rol",
      mobile: "trailing",
      cell: (user) => (
        <div className="flex flex-wrap gap-1">
          {user.roles.length === 0 ? (
            <Badge tone="warning">Rol yoxdur</Badge>
          ) : (
            user.roles.map((role) => (
              <Badge key={role} tone={ROLE_TONES[role] ?? "neutral"}>
                {roleLabels[role] ?? role}
              </Badge>
            ))
          )}
        </div>
      ),
    },
    {
      key: "driver",
      header: "Bağlı sürücü",
      mobile: "meta",
      cell: (user) =>
        user.driverFullName ? (
          <span className="inline-flex items-center gap-1.5">
            <Link2 className="size-3.5 text-muted-foreground" />
            {user.driverFullName}
          </span>
        ) : (
          <span className="text-muted-foreground">—</span>
        ),
    },
  ];

  return (
    <>
      <PageHeader
        title="İstifadəçilər"
        description="Hesabların rolları və sürücü kartına bağlantısı."
      />

      <div className="rounded-lg border border-border bg-surface-raised p-3.5">
        <p className="flex items-start gap-2 text-xs leading-relaxed text-muted-foreground">
          <ShieldCheck className="mt-px size-4 shrink-0 text-primary" />
          <span>
            Hər hesabın <strong className="font-medium text-foreground">bir</strong> rolu olur —
            yeni rol verildikdə köhnəsi silinir. Rol dəyişdikdə həmin istifadəçinin bütün açıq
            sessiyaları bağlanır və yenidən giriş tələb olunur.
            <br />
            <strong className="font-medium text-foreground">Sürücü</strong> rolundakı hesab, sürücü
            kartına bağlanmayana qədər heç bir gəlir və təyinat görmür.
          </span>
        </p>
      </div>

      <Toolbar
        search={list.value("search")}
        onSearch={(value) => list.setParam("search", value)}
        searchPlaceholder="İstifadəçi adı və ya e-poçt…"
        activeFilterCount={list.activeFilterCount}
        onClear={list.clearFilters}
        filters={
          <FilterControl label="Rol">
            <NativeSelect
              value={list.value("role")}
              onChange={(event) => list.setParam("role", event.target.value)}
            >
              <option value="">Bütün rollar</option>
              {Object.values(Role).map((role) => (
                <option key={role} value={role}>
                  {roleLabels[role] ?? role}
                </option>
              ))}
            </NativeSelect>
          </FilterControl>
        }
      />

      <DataTable
        columns={columns}
        result={list.result}
        isLoading={list.isLoading}
        isFetching={list.isFetching}
        error={list.error}
        onRetry={() => void list.refetch()}
        sortBy={list.sortBy}
        descending={list.descending}
        onSort={list.setSort}
        onPage={list.setPage}
        rowKey={(user) => user.id}
        emptyTitle="İstifadəçi tapılmadı"
        emptyDescription="Yeni hesablar /api/auths/register vasitəsilə qeydiyyatdan keçir və avtomatik Sürücü rolu alır."
        actions={(user) => (
          <RowActions>
            <MenuItem onSelect={() => setRoleTarget(user)}>
              <UserCog />
              Rolu dəyiş
            </MenuItem>

            <MenuSeparator />

            <MenuItem onSelect={() => setLinkTarget(user)}>
              <Link2 />
              Sürücüyə bağla
            </MenuItem>

            {user.driverId && (
              <MenuItem danger onSelect={() => unlink.mutate(user)}>
                <Link2Off />
                Bağlantını sil
              </MenuItem>
            )}
          </RowActions>
        )}
      />

      {roleTarget && (
        <RoleDialog user={roleTarget} onClose={() => setRoleTarget(null)} />
      )}

      {linkTarget && (
        <LinkDriverDialog user={linkTarget} onClose={() => setLinkTarget(null)} />
      )}
    </>
  );
}

function RoleDialog({ user, onClose }: { user: UserListItem; onClose: () => void }) {
  const [role, setRole] = useState<Role>(user.roles[0] ?? Role.Driver);

  const save = useResourceMutation(() => authApi.assignRole(user.userName, role), {
    invalidate: ["users"],
    onSuccess: onClose,
  });

  return (
    <FormDialog
      open
      onOpenChange={(open) => !open && onClose()}
      title="Rolu dəyiş"
      description={`${user.userName} · ${user.email}`}
      size="sm"
      submitLabel="Rolu ver"
      submitting={save.isPending}
      onSubmit={() => save.mutate(undefined as never)}
    >
      <Field label="Rol" required className="sm:col-span-2">
        {(props) => (
          <NativeSelect
            {...props}
            value={role}
            onChange={(event) => setRole(event.target.value as Role)}
          >
            {Object.values(Role).map((value) => (
              <option key={value} value={value}>
                {roleLabels[value] ?? value}
              </option>
            ))}
          </NativeSelect>
        )}
      </Field>

      <div className="space-y-2 rounded-md bg-surface-raised p-3 text-xs leading-relaxed text-muted-foreground sm:col-span-2">
        <p>
          <strong className="font-medium text-foreground">Administrator</strong> — hər şey, o
          cümlədən həmişəlik silmə və rol idarəçiliyi.
        </p>
        <p>
          <strong className="font-medium text-foreground">Menecer</strong> — filo məlumatlarını
          yaradır, dəyişir və arxivləyir; arxivi görür.
        </p>
        <p>
          <strong className="font-medium text-foreground">Sürücü</strong> — filonu oxuyur; gəlir və
          təyinatlarda yalnız özünə aid qeydləri görür.
        </p>
      </div>
    </FormDialog>
  );
}

function LinkDriverDialog({ user, onClose }: { user: UserListItem; onClose: () => void }) {
  const [driverId, setDriverId] = useState(user.driverId ?? "");

  const save = useResourceMutation(
    () => authApi.linkDriver(user.userName, driverId || null),
    { invalidate: ["users"], onSuccess: onClose },
  );

  return (
    <FormDialog
      open
      onOpenChange={(open) => !open && onClose()}
      title="Sürücü kartına bağla"
      description={`${user.userName} · ${user.email}`}
      size="sm"
      submitLabel="Yadda saxla"
      submitting={save.isPending}
      onSubmit={() => save.mutate(undefined as never)}
    >
      <Field
        label="Sürücü"
        hint="Boş buraxsanız, mövcud bağlantı silinir. Bir sürücü yalnız bir hesaba bağlana bilər."
        className="sm:col-span-2"
      >
        {(props) => (
          <DriverPicker
            {...props}
            placeholder="Bağlantı yoxdur"
            value={driverId}
            onChange={(event) => setDriverId(event.target.value)}
          />
        )}
      </Field>
    </FormDialog>
  );
}
