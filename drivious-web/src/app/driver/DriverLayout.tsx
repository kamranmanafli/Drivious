import { useState } from "react";
import { NavLink, Outlet } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { Bell, Car, Home, KeyRound, LogOut, User, Wallet } from "lucide-react";
import { cn } from "@/lib/cn";
import { useAuth } from "@/auth/AuthContext";
import { notifications as notificationsApi } from "@/api/endpoints";
import { DEMO } from "@/api/demo/adapter";
import { ThemeToggle } from "@/theme";
import { Avatar, Badge, Menu, MenuContent, MenuItem, MenuLabel, MenuSeparator, MenuTrigger } from "@/ui";
import { ChangePasswordDialog } from "../ChangePasswordDialog";

const TABS = [
  { to: "/", label: "Ana səhifə", icon: Home, end: true },
  { to: "/my/earnings", label: "Qazancım", icon: Wallet, end: false },
  { to: "/my/fleet", label: "Filo", icon: Car, end: false },
  { to: "/my/notifications", label: "Bildiriş", icon: Bell, end: false },
  { to: "/my/profile", label: "Profil", icon: User, end: false },
];

/**
 * Driver shell — built phone-first, because a driver opens this standing next
 * to the car, not at a desk. On a wide screen the same tabs move to a top bar
 * rather than becoming a second, differently-shaped app.
 */
export function DriverLayout() {
  const { user, logout } = useAuth();
  const [passwordOpen, setPasswordOpen] = useState(false);

  const { data: unread } = useQuery({
    queryKey: ["notifications", "unread-count"],
    queryFn: () => notificationsApi.list({ isRead: false, pageSize: 1 }),
    select: (page) => page.totalCount,
    refetchInterval: 120_000,
  });

  return (
    <div className="min-h-dvh bg-background">
      <header className="sticky top-0 z-20 border-b border-border bg-background/85 backdrop-blur">
        <div className="mx-auto flex h-14 max-w-3xl items-center gap-3 px-4">
          <div className="flex items-center gap-2 font-semibold tracking-tight">
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
          </div>

          {DEMO && (
            <Badge tone="warning" className="hidden sm:inline-flex">
              Demo
            </Badge>
          )}

          {/* Wide screens get the tabs up here instead of at the bottom. */}
          <nav className="ml-4 hidden items-center gap-1 sm:flex">
            {TABS.map((tab) => (
              <NavLink
                key={tab.to}
                to={tab.to}
                end={tab.end}
                className={({ isActive }) =>
                  cn(
                    "rounded-md px-2.5 py-1.5 text-sm transition-colors",
                    isActive
                      ? "bg-primary-muted font-medium text-primary"
                      : "text-muted-foreground hover:bg-muted hover:text-foreground",
                  )
                }
              >
                {tab.label}
              </NavLink>
            ))}
          </nav>

          <div className="ml-auto flex items-center gap-1">
            <ThemeToggle className="hidden sm:inline-flex" />

            <Menu>
              <MenuTrigger className="flex items-center gap-2 rounded-md p-1 hover:bg-muted">
                <Avatar name={user?.userName} className="size-7" />
              </MenuTrigger>

              <MenuContent>
                <MenuLabel>
                  <span className="block truncate font-normal text-foreground">
                    {user?.userName}
                  </span>
                  <span className="mt-0.5 block truncate">{user?.email}</span>
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
        </div>
      </header>

      {/* Bottom padding clears the tab bar so the last row is never trapped under it. */}
      <main className="mx-auto max-w-3xl space-y-4 px-4 pb-28 pt-4 sm:pb-8">
        <Outlet />
      </main>

      <nav className="fixed inset-x-0 bottom-0 z-20 border-t border-border bg-surface/95 backdrop-blur sm:hidden">
        <div className="flex">
          {TABS.map((tab) => (
            <NavLink
              key={tab.to}
              to={tab.to}
              end={tab.end}
              className={({ isActive }) =>
                cn(
                  "relative flex flex-1 flex-col items-center gap-0.5 py-2 text-[11px] transition-colors",
                  isActive ? "text-primary" : "text-muted-foreground",
                )
              }
            >
              {({ isActive }) => (
                <>
                  <span className="relative">
                    <tab.icon className={cn("size-5", isActive && "stroke-[2.25]")} />
                    {tab.to === "/my/notifications" && Boolean(unread) && (
                      <span className="absolute -right-1.5 -top-1 flex min-w-3.5 items-center justify-center rounded-full bg-danger px-1 text-[9px] font-semibold leading-[14px] text-white">
                        {unread! > 9 ? "9+" : unread}
                      </span>
                    )}
                  </span>
                  {tab.label}
                </>
              )}
            </NavLink>
          ))}
        </div>
        <div className="h-safe-bottom" />
      </nav>

      <ChangePasswordDialog open={passwordOpen} onOpenChange={setPasswordOpen} />
    </div>
  );
}
