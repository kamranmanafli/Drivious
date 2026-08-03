/**
 * An axios adapter that answers from memory instead of the network.
 *
 * It reimplements the parts of the API a person can observe: the response
 * envelope, paging, the per-resource filters and sort allow-lists, the role
 * rules, the driver scoping on incomes and assignments, soft delete with
 * cascade, and the dashboard aggregation. Everything above it — the client,
 * the interceptors, the error handling, the screens — runs unchanged.
 *
 * It is not a second source of truth for business rules: when the API and this
 * file disagree, the API is right. Its job is to let the interface be opened
 * and reviewed without a SQL Server instance.
 */

import type { AxiosAdapter, AxiosResponse, InternalAxiosRequestConfig } from "axios";
import {
  Role,
  VehicleStatus,
  type ApiResponse,
  type Dashboard,
  type MonthlyTotal,
  type PagedResult,
} from "../types";
import * as seed from "./data";

export const DEMO: boolean = String(import.meta.env.VITE_DEMO) === "true";

/* ── Mutable store ───────────────────────────────────────────────────────── */

const db = {
  vehicles: [...seed.vehicles],
  drivers: [...seed.drivers],
  vehicleassignments: [...seed.assignments],
  expenses: [...seed.expenses],
  incomes: [...seed.incomes],
  fuellogs: [...seed.fuelLogs],
  maintenances: [...seed.maintenances],
  insurances: [...seed.insurances],
  vehicledocuments: [...seed.documents],
  notifications: [...seed.notifications],
};

type ResourceName = keyof typeof db;

const users = [...seed.users];

/* ── Session ─────────────────────────────────────────────────────────────── */

interface Session {
  userId: string;
  userName: string;
  email: string;
  driverId: string | null;
  roles: Role[];
}

/**
 * Demo tokens carry the account id in the string itself — `demo.access.<id>.<nonce>`
 * — rather than being looked up in a Map.
 *
 * A Map lives only as long as the tab: reloading the page would leave a valid
 * token in localStorage that the adapter no longer recognised, and every reload
 * would bounce the user back to the login screen. Reading the id back out of the
 * token keeps a demo session alive across reloads, the way a real JWT does.
 *
 * The dot separator is safe because no account id contains one.
 */
const revoked = new Set<string>();

function userIdFromToken(token: string, kind: "access" | "refresh"): string | null {
  if (revoked.has(token)) return null;

  const parts = token.split(".");
  if (parts.length !== 4 || parts[0] !== "demo" || parts[1] !== kind) return null;

  return parts[2] || null;
}

function sessionFor(config: InternalAxiosRequestConfig): Session | null {
  const header = String(config.headers?.Authorization ?? "");
  const token = header.startsWith("Bearer ") ? header.slice(7) : null;
  if (!token) return null;

  const userId = userIdFromToken(token, "access");
  if (!userId) return null;

  const user = users.find((u) => u.id === userId);
  if (!user) return null;

  return {
    userId: user.id,
    userName: user.userName,
    email: user.email,
    driverId: user.driverId,
    roles: user.roles,
  };
}

const isDriverOnly = (s: Session) =>
  s.roles.includes(Role.Driver) &&
  !s.roles.includes(Role.Admin) &&
  !s.roles.includes(Role.Manager);

const canManage = (s: Session) => s.roles.includes(Role.Admin) || s.roles.includes(Role.Manager);
const isAdmin = (s: Session) => s.roles.includes(Role.Admin);

function issueTokens(userId: string) {
  const nonce = () => Math.random().toString(36).slice(2);
  return {
    access: `demo.access.${userId}.${nonce()}`,
    refresh: `demo.refresh.${userId}.${nonce()}`,
  };
}

/* ── Response helpers ────────────────────────────────────────────────────── */

function respond<T>(
  config: InternalAxiosRequestConfig,
  status: number,
  body: ApiResponse<T>,
): AxiosResponse<ApiResponse<T>> {
  return {
    data: body,
    status,
    statusText: status === 200 ? "OK" : String(status),
    headers: {},
    config,
    request: null,
  };
}

function ok<T>(config: InternalAxiosRequestConfig, message: string, data?: T) {
  return respond(config, 200, { success: true, message, data });
}

function created<T>(config: InternalAxiosRequestConfig, message: string, data?: T) {
  return respond(config, 201, { success: true, message, data });
}

function fail(config: InternalAxiosRequestConfig, status: number, message: string) {
  const response = respond(config, status, { success: false, message });
  const error = Object.assign(new Error(message), {
    isAxiosError: true,
    config,
    response,
    toJSON: () => ({ message }),
  });
  return Promise.reject(error);
}

/* ── Query mechanics ─────────────────────────────────────────────────────── */

type AnyRow = Record<string, unknown> & { id: string; isDeleted: boolean };

interface ResourceSpec {
  search: (row: AnyRow) => Array<string | null | undefined>;
  sortable: Record<string, (row: AnyRow) => string | number>;
  defaultSort: string;
  filter?: (row: AnyRow, p: Record<string, string>) => boolean;
  /** Restricts the set for an account that only holds the Driver role. */
  scope?: (row: AnyRow, driverId: string | null) => boolean;
}

const num = (v: string | undefined) => (v == null || v === "" ? undefined : Number(v));
const bool = (v: string | undefined) =>
  v == null || v === "" ? undefined : v === "true" || v === "True";
const time = (v: unknown) => (v ? new Date(String(v)).getTime() : 0);

const onOrAfter = (value: unknown, from: string | undefined) =>
  from == null || from === "" || time(value) >= new Date(from).setHours(0, 0, 0, 0);

const onOrBefore = (value: unknown, to: string | undefined) =>
  to == null || to === "" || time(value) <= new Date(to).setHours(23, 59, 59, 999);

const SPECS: Record<ResourceName, ResourceSpec> = {
  vehicles: {
    search: (r) => [r.brand as string, r.model as string, r.plateNumber as string, r.vin as string],
    sortable: {
      brand: (r) => String(r.brand),
      model: (r) => String(r.model),
      year: (r) => Number(r.year),
      plateNumber: (r) => String(r.plateNumber),
      mileage: (r) => Number(r.mileage),
      status: (r) => Number(r.status),
      createdAt: (r) => time(r.createdAt),
    },
    defaultSort: "createdAt",
    filter: (r, p) =>
      (num(p.status) === undefined || r.status === num(p.status)) &&
      (num(p.fuelType) === undefined || r.fuelType === num(p.fuelType)) &&
      (!p.brand || String(r.brand).toLowerCase().includes(p.brand.toLowerCase())) &&
      (num(p.minYear) === undefined || Number(r.year) >= num(p.minYear)!) &&
      (num(p.maxYear) === undefined || Number(r.year) <= num(p.maxYear)!),
  },

  drivers: {
    search: (r) => [
      r.firstName as string,
      r.lastName as string,
      r.phoneNumber as string,
      r.email as string,
    ],
    sortable: {
      firstName: (r) => String(r.firstName),
      lastName: (r) => String(r.lastName),
      licenseExpireDate: (r) => time(r.licenseExpireDate),
      hireDate: (r) => time(r.hireDate),
      isActive: (r) => (r.isActive ? 1 : 0),
      createdAt: (r) => time(r.createdAt),
    },
    defaultSort: "createdAt",
    filter: (r, p) =>
      (bool(p.isActive) === undefined || r.isActive === bool(p.isActive)) &&
      onOrBefore(r.licenseExpireDate, p.licenseExpiresBefore),
  },

  vehicleassignments: {
    search: (r) => [
      r.note as string,
      r.vehiclePlateNumber as string,
      r.driverFullName as string,
    ],
    sortable: {
      assignedDate: (r) => time(r.assignedDate),
      returnedDate: (r) => time(r.returnedDate),
      isActive: (r) => (r.isActive ? 1 : 0),
      plateNumber: (r) => String(r.vehiclePlateNumber),
      driver: (r) => String(r.driverFullName),
      createdAt: (r) => time(r.createdAt),
    },
    defaultSort: "assignedDate",
    filter: (r, p) =>
      (!p.vehicleId || r.vehicleId === p.vehicleId) &&
      (!p.driverId || r.driverId === p.driverId) &&
      (bool(p.isActive) === undefined || r.isActive === bool(p.isActive)) &&
      (bool(p.isOpen) === undefined ||
        (bool(p.isOpen) ? r.returnedDate == null : r.returnedDate != null)),
    scope: (r, driverId) => r.driverId === driverId,
  },

  expenses: {
    search: (r) => [r.description as string, r.vehiclePlateNumber as string],
    sortable: {
      amount: (r) => Number(r.amount),
      category: (r) => Number(r.category),
      expenseDate: (r) => time(r.expenseDate),
      plateNumber: (r) => String(r.vehiclePlateNumber),
      createdAt: (r) => time(r.createdAt),
    },
    defaultSort: "expenseDate",
    filter: (r, p) =>
      (!p.vehicleId || r.vehicleId === p.vehicleId) &&
      (num(p.category) === undefined || r.category === num(p.category)) &&
      onOrAfter(r.expenseDate, p.from) &&
      onOrBefore(r.expenseDate, p.to) &&
      (num(p.minAmount) === undefined || Number(r.amount) >= num(p.minAmount)!) &&
      (num(p.maxAmount) === undefined || Number(r.amount) <= num(p.maxAmount)!),
  },

  incomes: {
    search: (r) => [r.description as string, r.vehiclePlateNumber as string],
    sortable: {
      amount: (r) => Number(r.amount),
      incomeDate: (r) => time(r.incomeDate),
      plateNumber: (r) => String(r.vehiclePlateNumber),
      driver: (r) => String(r.driverFullName),
      createdAt: (r) => time(r.createdAt),
    },
    defaultSort: "incomeDate",
    filter: (r, p) =>
      (!p.vehicleId || r.vehicleId === p.vehicleId) &&
      (!p.driverId || r.driverId === p.driverId) &&
      onOrAfter(r.incomeDate, p.from) &&
      onOrBefore(r.incomeDate, p.to) &&
      (num(p.minAmount) === undefined || Number(r.amount) >= num(p.minAmount)!) &&
      (num(p.maxAmount) === undefined || Number(r.amount) <= num(p.maxAmount)!),
    scope: (r, driverId) => r.driverId === driverId,
  },

  fuellogs: {
    search: (r) => [r.stationName as string, r.vehiclePlateNumber as string],
    sortable: {
      fuelDate: (r) => time(r.fuelDate),
      liters: (r) => Number(r.liters),
      price: (r) => Number(r.price),
      mileage: (r) => Number(r.mileage),
      stationName: (r) => String(r.stationName),
      plateNumber: (r) => String(r.vehiclePlateNumber),
      createdAt: (r) => time(r.createdAt),
    },
    defaultSort: "fuelDate",
    filter: (r, p) =>
      (!p.vehicleId || r.vehicleId === p.vehicleId) &&
      onOrAfter(r.fuelDate, p.from) &&
      onOrBefore(r.fuelDate, p.to),
  },

  maintenances: {
    search: (r) => [
      r.serviceCenter as string,
      r.description as string,
      r.vehiclePlateNumber as string,
    ],
    sortable: {
      serviceType: (r) => Number(r.serviceType),
      cost: (r) => Number(r.cost),
      maintenanceDate: (r) => time(r.maintenanceDate),
      nextMaintenanceDate: (r) => time(r.nextMaintenanceDate),
      mileage: (r) => Number(r.mileage),
      serviceCenter: (r) => String(r.serviceCenter),
      plateNumber: (r) => String(r.vehiclePlateNumber),
      createdAt: (r) => time(r.createdAt),
    },
    defaultSort: "maintenanceDate",
    filter: (r, p) =>
      (!p.vehicleId || r.vehicleId === p.vehicleId) &&
      (num(p.serviceType) === undefined || r.serviceType === num(p.serviceType)) &&
      onOrAfter(r.maintenanceDate, p.from) &&
      onOrBefore(r.maintenanceDate, p.to) &&
      (!p.dueBefore || (r.nextMaintenanceDate != null && onOrBefore(r.nextMaintenanceDate, p.dueBefore))),
  },

  insurances: {
    search: (r) => [
      r.companyName as string,
      r.policyNumber as string,
      r.vehiclePlateNumber as string,
    ],
    sortable: {
      companyName: (r) => String(r.companyName),
      policyNumber: (r) => String(r.policyNumber),
      startDate: (r) => time(r.startDate),
      endDate: (r) => time(r.endDate),
      price: (r) => Number(r.price),
      plateNumber: (r) => String(r.vehiclePlateNumber),
      createdAt: (r) => time(r.createdAt),
    },
    defaultSort: "endDate",
    filter: (r, p) =>
      (!p.vehicleId || r.vehicleId === p.vehicleId) &&
      onOrBefore(r.endDate, p.expiresBefore) &&
      (!p.activeOn ||
        (time(r.startDate) <= new Date(p.activeOn).getTime() &&
          time(r.endDate) >= new Date(p.activeOn).getTime())),
  },

  vehicledocuments: {
    search: (r) => [r.title as string, r.vehiclePlateNumber as string],
    sortable: {
      title: (r) => String(r.title),
      documentType: (r) => Number(r.documentType),
      uploadDate: (r) => time(r.uploadDate),
      expiryDate: (r) => time(r.expiryDate),
      plateNumber: (r) => String(r.vehiclePlateNumber),
      createdAt: (r) => time(r.createdAt),
    },
    defaultSort: "uploadDate",
    filter: (r, p) =>
      (!p.vehicleId || r.vehicleId === p.vehicleId) &&
      (num(p.documentType) === undefined || r.documentType === num(p.documentType)),
  },

  notifications: {
    search: (r) => [r.title as string, r.message as string],
    sortable: {
      title: (r) => String(r.title),
      type: (r) => Number(r.type),
      isRead: (r) => (r.isRead ? 1 : 0),
      notificationDate: (r) => time(r.notificationDate),
      createdAt: (r) => time(r.createdAt),
    },
    defaultSort: "notificationDate",
    filter: (r, p) =>
      (num(p.type) === undefined || r.type === num(p.type)) &&
      (bool(p.isRead) === undefined || r.isRead === bool(p.isRead)) &&
      onOrAfter(r.notificationDate, p.from) &&
      onOrBefore(r.notificationDate, p.to),
  },
};

function paginate(
  name: ResourceName,
  params: Record<string, string>,
  deleted: boolean,
  session: Session,
): PagedResult<AnyRow> {
  const spec = SPECS[name];
  const search = (params.search ?? "").trim().toLowerCase();

  let rows = (db[name] as unknown as AnyRow[]).filter((r) => r.isDeleted === deleted);

  if (spec.scope && isDriverOnly(session)) {
    rows = rows.filter((r) => spec.scope!(r, session.driverId));
  }

  if (spec.filter) rows = rows.filter((r) => spec.filter!(r, params));

  if (search) {
    rows = rows.filter((r) =>
      spec.search(r).some((field) => field && String(field).toLowerCase().includes(search)),
    );
  }

  const sortKey =
    params.sortBy && spec.sortable[params.sortBy] ? params.sortBy : spec.defaultSort;
  const selector = spec.sortable[sortKey];
  const descending = params.descending === "true";

  rows = [...rows].sort((a, b) => {
    const x = selector(a);
    const y = selector(b);
    const cmp = typeof x === "number" && typeof y === "number" ? x - y : String(x).localeCompare(String(y), "az");
    return descending ? -cmp : cmp;
  });

  const page = Math.max(1, Number(params.page) || 1);
  const pageSize = Math.min(100, Math.max(1, Number(params.pageSize) || 20));
  const totalCount = rows.length;
  const totalPages = pageSize > 0 ? Math.ceil(totalCount / pageSize) : 0;

  return {
    items: rows.slice((page - 1) * pageSize, page * pageSize),
    page,
    pageSize,
    totalCount,
    totalPages,
    hasPrevious: page > 1,
    hasNext: page < totalPages,
  };
}

/* ── Writes ──────────────────────────────────────────────────────────────── */

/** Reads a JSON body or a FormData body into a plain object. */
function readBody(config: InternalAxiosRequestConfig): Record<string, unknown> {
  const raw = config.data;
  if (!raw) return {};

  if (typeof FormData !== "undefined" && raw instanceof FormData) {
    const out: Record<string, unknown> = {};
    raw.forEach((value, key) => {
      out[key] = value instanceof File ? value.name : value;
    });
    return out;
  }

  if (typeof raw === "string") {
    try {
      return JSON.parse(raw) as Record<string, unknown>;
    } catch {
      return {};
    }
  }

  return raw as Record<string, unknown>;
}

/** Copies the display fields the API projects from the related rows. */
function decorate(name: ResourceName, row: AnyRow): AnyRow {
  if (row.vehicleId) {
    const vehicle = db.vehicles.find((v) => v.id === row.vehicleId);
    if (vehicle) {
      row.vehiclePlateNumber = vehicle.plateNumber;
      row.vehicleName = `${vehicle.brand} ${vehicle.model}`;
    }
  }

  if (row.driverId) {
    const driver = db.drivers.find((d) => d.id === row.driverId);
    if (driver) row.driverFullName = `${driver.firstName} ${driver.lastName}`;
  }

  if (name === "drivers") {
    row.fullName = `${row.firstName} ${row.lastName}`;
  }

  return row;
}

/** Numeric and boolean fields arrive as strings from multipart bodies. */
const NUMERIC_FIELDS = new Set([
  "year", "mileage", "status", "fuelType", "category", "amount", "liters",
  "price", "cost", "serviceType", "documentType", "type",
]);
const BOOLEAN_FIELDS = new Set(["isActive", "isRead"]);

function coerce(values: Record<string, unknown>): Record<string, unknown> {
  const out: Record<string, unknown> = {};

  for (const [key, value] of Object.entries(values)) {
    if (value === undefined || value === null || value === "") continue;
    if (NUMERIC_FIELDS.has(key)) out[key] = Number(value);
    else if (BOOLEAN_FIELDS.has(key)) out[key] = value === true || value === "true";
    else out[key] = value;
  }

  return out;
}

const VEHICLE_CHILDREN: ResourceName[] = [
  "expenses", "incomes", "maintenances", "insurances",
  "fuellogs", "vehicledocuments", "vehicleassignments",
];

function cascade(name: ResourceName, id: string, isDeleted: boolean) {
  const targets =
    name === "vehicles"
      ? VEHICLE_CHILDREN.map((child) => ({
          child,
          match: (r: AnyRow) => r.vehicleId === id,
        }))
      : name === "drivers"
        ? (["incomes", "vehicleassignments"] as ResourceName[]).map((child) => ({
            child,
            match: (r: AnyRow) => r.driverId === id,
          }))
        : [];

  for (const { child, match } of targets) {
    for (const row of db[child] as unknown as AnyRow[]) {
      if (!match(row)) continue;

      // Deleting takes the live rows; restoring takes back only what this
      // cascade removed, so a child archived deliberately stays archived.
      if (isDeleted ? !row.isDeleted : row.isDeleted && row.deletedByCascade) {
        row.isDeleted = isDeleted;
        row.deletedAt = isDeleted ? new Date().toISOString() : null;
        row.deletedByCascade = isDeleted;
      }
    }
  }
}

function countChildren(name: "vehicles" | "drivers", id: string): number {
  const children = name === "vehicles" ? VEHICLE_CHILDREN : (["incomes", "vehicleassignments"] as ResourceName[]);
  const key = name === "vehicles" ? "vehicleId" : "driverId";

  return children.reduce(
    (total, child) => total + (db[child] as unknown as AnyRow[]).filter((r) => r[key] === id).length,
    0,
  );
}

/* ── Dashboard ───────────────────────────────────────────────────────────── */

const live = <T extends { isDeleted: boolean }>(rows: T[]) => rows.filter((r) => !r.isDeleted);
const sum = <T>(rows: T[], get: (r: T) => number) => rows.reduce((t, r) => t + get(r), 0);

function daysFromToday(value: string | null | undefined): number | null {
  if (!value) return null;
  const target = new Date(value);
  target.setHours(0, 0, 0, 0);
  return Math.round((target.getTime() - seed.today.getTime()) / 86_400_000);
}

const withinAhead = (value: string | null | undefined, lead: number) => {
  const d = daysFromToday(value);
  return d !== null && d >= 0 && d <= lead;
};

const isPast = (value: string | null | undefined) => {
  const d = daysFromToday(value);
  return d !== null && d < 0;
};

function buildDashboard(): Dashboard {
  const lead = seed.LEAD_DAYS_SETTING;

  const incomes = live(db.incomes);
  const expenses = live(db.expenses);
  const totalIncome = sum(incomes, (r) => r.amount);
  const totalExpense = sum(expenses, (r) => r.amount);

  const monthlyTotals: MonthlyTotal[] = [];
  const start = new Date(seed.today.getFullYear(), seed.today.getMonth() - 5, 1);

  for (let i = 0; i < 6; i++) {
    const month = new Date(start.getFullYear(), start.getMonth() + i, 1);
    const inMonth = (iso: string) => {
      const d = new Date(iso);
      return d.getFullYear() === month.getFullYear() && d.getMonth() === month.getMonth();
    };

    const income = sum(incomes.filter((r) => inMonth(r.incomeDate)), (r) => r.amount);
    const expense = sum(expenses.filter((r) => inMonth(r.expenseDate)), (r) => r.amount);

    monthlyTotals.push({
      year: month.getFullYear(),
      month: month.getMonth() + 1,
      label: `${month.getFullYear()}-${String(month.getMonth() + 1).padStart(2, "0")}`,
      income: Math.round(income),
      expense: Math.round(expense),
      profit: Math.round(income - expense),
    });
  }

  const perVehicle = new Map<string, number>();
  const add = (id: string, amount: number) =>
    perVehicle.set(id, (perVehicle.get(id) ?? 0) + amount);

  expenses.forEach((r) => add(r.vehicleId, r.amount));
  live(db.fuellogs).forEach((r) => add(r.vehicleId, r.price));
  live(db.maintenances).forEach((r) => add(r.vehicleId, r.cost));

  const topSpendingVehicles = [...perVehicle.entries()]
    .filter(([, total]) => total > 0)
    .sort((a, b) => b[1] - a[1])
    .slice(0, 5)
    .map(([vehicleId, totalCost]) => {
      const vehicle = db.vehicles.find((v) => v.id === vehicleId)!;
      return {
        vehicleId,
        plateNumber: vehicle.plateNumber,
        vehicleName: `${vehicle.brand} ${vehicle.model}`,
        totalCost: Math.round(totalCost),
      };
    });

  return {
    totalVehicles: live(db.vehicles).length,
    totalDrivers: live(db.drivers).length,
    activeVehicles: live(db.vehicles).filter((v) => v.status === VehicleStatus.Active).length,
    activeDrivers: live(db.drivers).filter((d) => d.isActive).length,
    totalIncome: Math.round(totalIncome),
    totalExpense: Math.round(totalExpense),
    totalNotifications: live(db.notifications).length,
    profit: Math.round(totalIncome - totalExpense),
    unreadNotifications: live(db.notifications).filter((n) => !n.isRead).length,
    assignedVehicles: new Set(
      live(db.vehicleassignments)
        .filter((a) => a.isActive && a.returnedDate == null)
        .map((a) => a.vehicleId),
    ).size,
    totalFuelCost: Math.round(sum(live(db.fuellogs), (r) => r.price)),
    totalMaintenanceCost: Math.round(sum(live(db.maintenances), (r) => r.cost)),
    upcoming: {
      withinDays: lead,
      expiringInsurances: live(db.insurances).filter((r) => withinAhead(r.endDate, lead)).length,
      expiringLicenses: live(db.drivers).filter((r) => withinAhead(r.licenseExpireDate, lead)).length,
      dueMaintenances: live(db.maintenances).filter((r) => withinAhead(r.nextMaintenanceDate, lead)).length,
      expiringDocuments: live(db.vehicledocuments).filter((r) => withinAhead(r.expiryDate, lead)).length,
      overdueTotal:
        live(db.insurances).filter((r) => isPast(r.endDate)).length +
        live(db.drivers).filter((r) => isPast(r.licenseExpireDate)).length +
        live(db.maintenances).filter((r) => isPast(r.nextMaintenanceDate)).length +
        live(db.vehicledocuments).filter((r) => isPast(r.expiryDate)).length,
    },
    monthlyTotals,
    topSpendingVehicles,
  };
}

/* ── Routing ─────────────────────────────────────────────────────────────── */

const RESOURCES = Object.keys(db) as ResourceName[];

/** Human label used in the confirmation the API would have written. */
const LABEL: Record<ResourceName, string> = {
  vehicles: "Vehicle",
  drivers: "Driver",
  vehicleassignments: "Vehicle assignment",
  expenses: "Expense",
  incomes: "Income",
  fuellogs: "Fuel log",
  maintenances: "Maintenance",
  insurances: "Insurance",
  vehicledocuments: "Vehicle document",
  notifications: "Notification",
};

const latency = () => new Promise((r) => setTimeout(r, 90 + Math.random() * 160));

export const demoAdapter: AxiosAdapter = async (config) => {
  await latency();

  const raw = config.url ?? "";
  const path = raw
    .replace(String(config.baseURL ?? ""), "")
    .replace(/^https?:\/\/[^/]+/, "")
    .split("?")[0];

  const method = (config.method ?? "get").toLowerCase();
  const params: Record<string, string> = {};

  for (const [key, value] of Object.entries(config.params ?? {})) {
    if (value !== undefined && value !== null && value !== "") params[key] = String(value);
  }

  const body = coerce(readBody(config));
  const segments = path.split("/").filter(Boolean); // ["api", resource, ...]

  if (segments[0] !== "api") return fail(config, 404, "Not found.");

  /* ── Auth ─────────────────────────────────────────────────────────────── */

  if (segments[1] === "auths") {
    const action = segments[2];

    if (action === "login") {
      const user = users.find((u) => u.userName === String(body.userName ?? ""));
      if (!user || user.password !== String(body.password ?? "")) {
        return fail(config, 401, "Username or password is incorrect.");
      }

      const { access, refresh } = issueTokens(user.id);
      return ok(config, "Login successful.", {
        accessToken: access,
        refreshToken: refresh,
        accessTokenExpiresAt: new Date(Date.now() + 86_400_000).toISOString(),
        userName: user.userName,
        email: user.email,
        roles: user.roles,
      });
    }

    if (action === "refresh") {
      const presented = String(body.refreshToken ?? "");
      const userId = userIdFromToken(presented, "refresh");
      if (!userId) return fail(config, 401, "Refresh token is invalid or has expired.");

      const user = users.find((u) => u.id === userId);
      if (!user) return fail(config, 401, "Refresh token is invalid or has expired.");

      // Rotation, same as the API: the presented token is retired as its
      // replacement is issued, so a leaked one cannot be reused.
      revoked.add(presented);

      const { access, refresh } = issueTokens(user.id);

      return ok(config, "Token refreshed.", {
        accessToken: access,
        refreshToken: refresh,
        accessTokenExpiresAt: new Date(Date.now() + 86_400_000).toISOString(),
        userName: user.userName,
        email: user.email,
        roles: user.roles,
      });
    }

    if (action === "logout") {
      revoked.add(String(body.refreshToken ?? ""));
      return ok(config, "Logged out successfully.");
    }

    if (action === "register") {
      if (users.some((u) => u.userName === body.userName)) {
        return fail(config, 400, "Username already exists.");
      }
      if (users.some((u) => u.email === body.email)) {
        return fail(config, 400, "Email already exists.");
      }
      if (body.password !== body.confirmPassword) {
        return fail(config, 400, "Passwords do not match.");
      }

      users.push({
        id: `u-${users.length + 1}`,
        userName: String(body.userName),
        email: String(body.email),
        password: String(body.password),
        driverId: null,
        roles: [Role.Driver],
      });

      return ok(config, "User registered successfully.");
    }

    const session = sessionFor(config);
    if (!session) return fail(config, 401, "Unauthorized.");

    if (action === "me") {
      return ok(config, "User retrieved successfully.", {
        id: session.userId,
        userName: session.userName,
        email: session.email,
        driverId: session.driverId,
        roles: session.roles,
      });
    }

    if (action === "change-password") {
      const user = users.find((u) => u.id === session.userId)!;
      if (user.password !== String(body.currentPassword ?? "")) {
        return fail(config, 400, "Incorrect password.");
      }
      if (body.newPassword !== body.confirmNewPassword) {
        return fail(config, 400, "New passwords do not match.");
      }
      user.password = String(body.newPassword);
      return ok(config, "Password changed successfully.");
    }

    if (!isAdmin(session)) return fail(config, 403, "Forbidden.");

    if (action === "users") {
      const search = (params.search ?? "").toLowerCase();
      let rows = users.map((u) => ({
        id: u.id,
        userName: u.userName,
        email: u.email,
        driverId: u.driverId,
        driverFullName: db.drivers.find((d) => d.id === u.driverId)?.fullName ?? null,
        roles: u.roles,
      }));

      if (params.role) rows = rows.filter((u) => u.roles.includes(params.role as Role));
      if (search) {
        rows = rows.filter(
          (u) =>
            u.userName.toLowerCase().includes(search) || u.email.toLowerCase().includes(search),
        );
      }

      const page = Math.max(1, Number(params.page) || 1);
      const pageSize = Math.min(100, Math.max(1, Number(params.pageSize) || 20));
      const totalPages = Math.ceil(rows.length / pageSize);

      return ok(config, "Users retrieved successfully.", {
        items: rows.slice((page - 1) * pageSize, page * pageSize),
        page,
        pageSize,
        totalCount: rows.length,
        totalPages,
        hasPrevious: page > 1,
        hasNext: page < totalPages,
      });
    }

    if (action === "assign-role") {
      const user = users.find((u) => u.userName === body.userName);
      if (!user) return fail(config, 400, "User not found.");
      if (user.roles.includes(body.role as Role)) {
        return fail(config, 400, `User is already in the ${body.role} role.`);
      }
      user.roles = [body.role as Role];
      return ok(config, `User assigned to the ${body.role} role.`);
    }

    if (action === "link-driver") {
      const user = users.find((u) => u.userName === body.userName);
      if (!user) return fail(config, 400, "User not found.");

      const driverId = body.driverId ? String(body.driverId) : null;

      if (driverId) {
        if (!db.drivers.some((d) => d.id === driverId && !d.isDeleted)) {
          return fail(config, 400, "Driver not found.");
        }
        if (users.some((u) => u.driverId === driverId && u.id !== user.id)) {
          return fail(config, 400, "That driver is already linked to another account.");
        }
      }

      user.driverId = driverId;
      return ok(config, driverId ? "Account linked to driver." : "Account unlinked from driver.");
    }

    return fail(config, 404, "Not found.");
  }

  /* ── Everything below needs a session ─────────────────────────────────── */

  const session = sessionFor(config);
  if (!session) return fail(config, 401, "Unauthorized.");

  if (segments[1] === "dashboards") {
    if (!canManage(session)) return fail(config, 403, "Forbidden.");
    return ok(config, "Dashboard retrieved successfully.", buildDashboard());
  }

  const name = segments[1] as ResourceName;
  if (!RESOURCES.includes(name)) return fail(config, 404, "Not found.");

  const table = db[name] as unknown as AnyRow[];
  const label = LABEL[name];

  // POST /api/notifications/scan — nothing new to raise, the seed already holds
  // every warning that applies today.
  if (method === "post" && name === "notifications" && segments[2] === "scan") {
    if (!canManage(session)) return fail(config, 403, "Forbidden.");
    return ok(config, "Scan complete. 0 notification(s) created.");
  }

  if (method === "get") {
    if (segments[2] === "deleted") {
      if (!canManage(session)) return fail(config, 403, "Forbidden.");
      return ok(config, `Deleted ${name} retrieved successfully.`, paginate(name, params, true, session));
    }

    if (segments[2]) {
      const row = table.find((r) => r.id === segments[2]);
      if (!row) return fail(config, 404, `${label} not found.`);
      return ok(config, `${label} retrieved successfully.`, row);
    }

    return ok(config, `${label}s retrieved successfully.`, paginate(name, params, false, session));
  }

  if (!canManage(session)) return fail(config, 403, "Forbidden.");

  if (method === "post") {
    if (name === "vehicles") {
      if (db.vehicles.some((v) => !v.isDeleted && v.vin === body.vin)) {
        return fail(config, 400, "A vehicle with this VIN already exists.");
      }
      if (db.vehicles.some((v) => !v.isDeleted && v.plateNumber === body.plateNumber)) {
        return fail(config, 400, "A vehicle with this plate number already exists.");
      }
    }

    if (name === "vehicleassignments") {
      const open = db.vehicleassignments.filter(
        (a) => !a.isDeleted && a.isActive && a.returnedDate == null,
      );
      if (open.some((a) => a.vehicleId === body.vehicleId)) {
        return fail(config, 400, "This vehicle is already assigned to a driver. Return it first.");
      }
      if (open.some((a) => a.driverId === body.driverId)) {
        return fail(config, 400, "This driver already holds a vehicle. Return it first.");
      }
    }

    const row = decorate(name, {
      ...body,
      id: seed.uuid(),
      createdAt: new Date().toISOString(),
      updatedAt: null,
      deletedAt: null,
      isDeleted: false,
      ...(name === "vehicledocuments" ? { fileUrl: "#demo", uploadDate: new Date().toISOString() } : {}),
    } as AnyRow);

    table.unshift(row);
    return created(config, `${label} created successfully.`, row);
  }

  if (method === "patch") {
    // PATCH /api/vehicleassignments/return/{id}
    if (segments[2] === "return") {
      const row = table.find((r) => r.id === segments[3]);
      if (!row) return fail(config, 404, `${label} not found.`);
      if (row.returnedDate) return fail(config, 400, "This vehicle has already been returned.");

      row.returnedDate = params.returnedDate ?? new Date().toISOString();
      row.isActive = false;
      row.updatedAt = new Date().toISOString();

      return ok(config, "Vehicle returned successfully.", row);
    }

    if (segments[2] === "toggle") {
      const row = table.find((r) => r.id === segments[3]);
      if (!row) return fail(config, 404, `${label} not found.`);

      row.isDeleted = !row.isDeleted;
      row.deletedAt = row.isDeleted ? new Date().toISOString() : null;
      row.deletedByCascade = false;
      row.updatedAt = new Date().toISOString();

      if (name === "vehicles" || name === "drivers") cascade(name, row.id, row.isDeleted);

      return ok(config, `${label} status changed successfully.`, row);
    }

    const row = table.find((r) => r.id === segments[2]);
    if (!row) return fail(config, 404, `${label} not found.`);

    Object.assign(row, body, { updatedAt: new Date().toISOString() });
    decorate(name, row);

    return ok(config, `${label} updated successfully.`, row);
  }

  if (method === "delete") {
    if (!isAdmin(session)) return fail(config, 403, "Forbidden.");

    const index = table.findIndex((r) => r.id === segments[2]);
    if (index === -1) return fail(config, 404, `${label} not found.`);

    if (name === "vehicles" || name === "drivers") {
      const children = countChildren(name, segments[2]);
      if (children > 0) {
        return fail(
          config,
          400,
          `This ${name === "vehicles" ? "vehicle" : "driver"} has ${children} related record(s) ` +
            `and cannot be permanently deleted. Use the toggle endpoint to archive it instead.`,
        );
      }
    }

    table.splice(index, 1);
    return ok(config, `${label} deleted successfully.`);
  }

  return fail(config, 405, "Method not allowed.");
};

/** Shown on the login screen so the demo accounts are not a guessing game. */
export const DEMO_ACCOUNTS = seed.users.map((u) => ({
  userName: u.userName,
  password: u.password,
  role: u.roles[0],
}));
