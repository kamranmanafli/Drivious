import { useEffect, useState } from "react";
import { Link, NavLink, Outlet, useLocation } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import {
  Bell,
  ChevronDown,
  KeyRound,
  LogOut,
  Menu as MenuIcon,
  Settings,
  ShieldCheck,
  X,
} from "lucide-react";
import { cn } from "@/lib/cn";
import { useAuth } from "@/auth/AuthContext";
import { navFor } from "../nav";
import { roleLabels } from "@/lib/enums";
import { notifications as notificationsApi } from "@/api/endpoints";
import { ThemeToggle } from "@/theme";
import { Avatar, Badge, Menu, MenuContent, MenuItem, MenuLabel, MenuSeparator, MenuTrigger } from "@/ui";
import { DEMO } from "@/api/demo/adapter";
import { ChangePasswordDialog } from "../ChangePasswordDialog";

/** Admin + Manager shell: a fixed sidebar on desktop, a drawer on mobile. */
export function ConsoleLayout() {
  const { user, roles, isAdmin, logout } = useAuth();
  const location = useLocation();
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [passwordOpen, setPasswordOpen] = useState(false);

  // Navigating should close the drawer; otherwise it stays over the new page.
  useEffect(() => setDrawerOpen(false), [location.pathname]);

  const items = navFor(roles);

  const { data: unread } = useQuery({
    queryKey: ["notifications", "unread-count"],
    queryFn: () => notificationsApi.list({ isRead: false, pageSize: 1 }),
    select: (page) => page.totalCount,
    refetchInterval: 120_000,
  });

  const sidebar = (
    <>
      <div className="flex h-14 items-center justify-between gap-2 border-b border-border px-4">
        <Link to="/" className="flex items-center gap-2 font-semibold tracking-tight">
          <span className="flex size-7 items-center justify-center rounded-md bg-primary text-primary-foreground">
            <svg viewBox="0 0 24 24" fill="none" className="size-4">
              <path
                d="M4 16.5V13l1.6-4.4A2 2 0 0 1 7.5 7.2h9a2 2 0 0 1 1.9 1.4L20 13v3.5"
                stroke="currentColor"
                strokeWidth="1.8"
                strokeLinecap="round"
                strokeLinejoin="round"
              />
              <circle cx="7.5" cy="16.5" r="1.6" stroke="currentColor" strokeWidth="1.8" />
              <circle cx="16.5" cy="16.5" r="1.6" stroke="currentColor" strokeWidth="1.8" />
            </svg>
          </span>
          Drivious
        </Link>

        <button
          onClick={() => setDrawerOpen(false)}
          className="rounded-md p-1.5 text-muted-foreground hover:bg-muted lg:hidden"
          aria-label="Menyunu bağla"
        >
          <X className="size-4" />
        </button>
      </div>

      <nav className="flex-1 space-y-0.5 overflow-y-auto p-3">
        {items.map((item) => (
          <div key={item.to}>
            {item.group && (
              <p className="px-2 pb-1 pt-4 text-[11px] font-medium uppercase tracking-wide text-muted-foreground first:pt-0">
                {item.group}
              </p>
            )}

            <NavLink
              to={item.to}
              end={item.to === "/"}
              className={({ isActive }) =>
                cn(
                  "flex items-center gap-2.5 rounded-md px-2 py-1.5 text-sm transition-colors",
                  isActive
                    ? "bg-primary-muted font-medium text-primary"
                    : "text-muted-foreground hover:bg-muted hover:text-foreground",
                )
              }
            >
              <item.icon className="size-4 shrink-0" />
              {item.label}
            </NavLink>
          </div>
        ))}

        {isAdmin && (
          <>
            <p className="px-2 pb-1 pt-4 text-[11px] font-medium uppercase tracking-wide text-muted-foreground">
              Administrator
            </p>
            <NavLink
              to="/admin/users"
              className={({ isActive }) =>
                cn(
                  "flex items-center gap-2.5 rounded-md px-2 py-1.5 text-sm transition-colors",
                  isActive
                    ? "bg-primary-muted font-medium text-primary"
                    : "text-muted-foreground hover:bg-muted hover:text-foreground",
                )
              }
            >
              <ShieldCheck className="size-4 shrink-0" />
              İstifadəçilər
            </NavLink>
          </>
        )}

        <NavLink
          to="/archive"
          className={({ isActive }) =>
            cn(
              "flex items-center gap-2.5 rounded-md px-2 py-1.5 text-sm transition-colors",
              isActive
                ? "bg-primary-muted font-medium text-primary"
                : "text-muted-foreground hover:bg-muted hover:text-foreground",
            )
          }
        >
          <Settings className="size-4 shrink-0" />
          Arxiv
        </NavLink>
      </nav>

      <div className="border-t border-border p-3">
        <ThemeToggle className="w-full justify-center" />
      </div>
    </>
  );

  return (
    <div className="min-h-dvh bg-background">
      {/* Desktop sidebar */}
      <aside className="fixed inset-y-0 left-0 z-30 hidden w-60 flex-col border-r border-border bg-surface lg:flex">
        {sidebar}
      </aside>

      {/* Mobile drawer */}
      {drawerOpen && (
        <>
          <div
            className="fixed inset-0 z-40 bg-black/40 lg:hidden"
            onClick={() => setDrawerOpen(false)}
            aria-hidden
          />
          <aside className="fixed inset-y-0 left-0 z-50 flex w-64 flex-col border-r border-border bg-surface lg:hidden">
            {sidebar}
          </aside>
        </>
      )}

      <div className="lg:pl-60">
        <header className="sticky top-0 z-20 flex h-14 items-center gap-3 border-b border-border bg-background/85 px-4 backdrop-blur">
          <button
            onClick={() => setDrawerOpen(true)}
            className="rounded-md p-1.5 text-muted-foreground hover:bg-muted lg:hidden"
            aria-label="Menyunu aç"
          >
            <MenuIcon className="size-5" />
          </button>

          {DEMO && (
            <Badge tone="warning" className="hidden sm:inline-flex">
              Demo rejimi
            </Badge>
          )}

          <div className="ml-auto flex items-center gap-1">
            <Link
              to="/notifications"
              className="relative flex size-9 items-center justify-center rounded-md text-muted-foreground hover:bg-muted hover:text-foreground"
              aria-label={`Bildirişlər${unread ? ` (${unread} oxunmamış)` : ""}`}
            >
              <Bell className="size-4" />
              {Boolean(unread) && (
                <span className="absolute right-1.5 top-1.5 flex min-w-4 items-center justify-center rounded-full bg-danger px-1 text-[10px] font-semibold leading-4 text-white">
                  {unread! > 99 ? "99+" : unread}
                </span>
              )}
            </Link>

            <Menu>
              <MenuTrigger className="flex items-center gap-2 rounded-md p-1 pr-2 hover:bg-muted">
                <Avatar name={user?.userName} className="size-7" />
                <span className="hidden text-sm font-medium sm:inline">{user?.userName}</span>
                <ChevronDown className="size-3.5 text-muted-foreground" />
              </MenuTrigger>

              <MenuContent>
                <MenuLabel>
                  <span className="block truncate font-normal text-foreground">{user?.email}</span>
                  <span className="mt-0.5 block">
                    {roles.map((role) => roleLabels[role] ?? role).join(", ")}
                  </span>
                </MenuLabel>

                <MenuSeparator />

                <MenuItem onSelect={() => setPasswordOpen(true)}>
                  <KeyRound />
                  Şifrəni dəyiş
                </MenuItem>

                <MenuItem danger onSelect={() => void logout()}>
                  <LogOut />
                  Çıxış
                </MenuItem>
              </MenuContent>
            </Menu>
          </div>
        </header>

        <main className="mx-auto max-w-[1400px] space-y-5 p-4 sm:p-6">
          <Outlet />
        </main>
      </div>

      <ChangePasswordDialog open={passwordOpen} onOpenChange={setPasswordOpen} />
    </div>
  );
}
