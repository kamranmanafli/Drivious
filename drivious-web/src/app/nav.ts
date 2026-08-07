import {
  Banknote,
  Car,
  ClipboardList,
  FileText,
  Fuel,
  LayoutDashboard,
  Receipt,
  ShieldCheck,
  Users,
  Wrench,
} from "lucide-react";
import { Role } from "@/api/types";

export interface NavItem {
  to: string;
  label: string;
  icon: typeof Car;
  /** Roles that may open it. The API enforces the same split. */
  allow: Role[];
  /** Marks the start of a labelled group in the sidebar. */
  group?: string;
}

const MANAGE = [Role.Admin, Role.Manager];
const ALL = [Role.Admin, Role.Manager, Role.Driver];

export const CONSOLE_NAV: NavItem[] = [
  { to: "/", label: "İdarə paneli", icon: LayoutDashboard, allow: MANAGE, group: "Ümumi" },

  { to: "/vehicles", label: "Maşınlar", icon: Car, allow: ALL, group: "Filo" },
  { to: "/drivers", label: "Sürücülər", icon: Users, allow: ALL },
  { to: "/assignments", label: "Təyinatlar", icon: ClipboardList, allow: ALL },

  { to: "/incomes", label: "Gəlirlər", icon: Banknote, allow: ALL, group: "Maliyyə" },
  { to: "/expenses", label: "Xərclər", icon: Receipt, allow: ALL },
  { to: "/fuel", label: "Yanacaq", icon: Fuel, allow: ALL },

  { to: "/maintenance", label: "Servis", icon: Wrench, allow: ALL, group: "Qeydiyyat" },
  { to: "/insurance", label: "Sığorta", icon: ShieldCheck, allow: ALL },
  { to: "/documents", label: "Sənədlər", icon: FileText, allow: ALL },
];

export function navFor(roles: Role[]): NavItem[] {
  return CONSOLE_NAV.filter((item) => item.allow.some((role) => roles.includes(role)));
}
