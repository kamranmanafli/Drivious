/**
 * Mirrors the Drivious API contract one-for-one.
 *
 * Two naming details are copied from the server rather than normalised, because
 * changing them here would just mean the field arrives undefined:
 *
 *  - `Vehicle.ImageURL` serialises as `imageURL`, while `Driver.ImageUrl`
 *    serialises as `imageUrl`. System.Text.Json's camelCase policy only
 *    lowercases the leading uppercase run, so the two differ.
 *  - `Vehicle.VIN` serialises as `vin` (the whole run is uppercase, so all of
 *    it is lowercased).
 */

/* ── Enums ──────────────────────────────────────────────────────────────────
   The API sends and expects integers. The numbers here are the C# declaration
   order — reordering Drivious.Enums without changing these silently mislabels
   every row. */

export const FuelType = {
  Gasoline: 0,
  Diesel: 1,
  Hybrid: 2,
  Electric: 3,
} as const;
export type FuelType = (typeof FuelType)[keyof typeof FuelType];

export const VehicleStatus = {
  Active: 0,
  InService: 1,
  Inactive: 2,
} as const;
export type VehicleStatus = (typeof VehicleStatus)[keyof typeof VehicleStatus];

export const ExpenseCategory = {
  Fuel: 0,
  Oil: 1,
  Service: 2,
  Repair: 3,
  Tire: 4,
  Fine: 5,
  Other: 6,
} as const;
export type ExpenseCategory =
  (typeof ExpenseCategory)[keyof typeof ExpenseCategory];

export const MaintenanceType = {
  OilChange: 0,
  TireChange: 1,
  BrakeService: 2,
  EngineRepair: 3,
  BatteryReplacement: 4,
  FilterReplacement: 5,
  Other: 6,
} as const;
export type MaintenanceType =
  (typeof MaintenanceType)[keyof typeof MaintenanceType];

export const DocumentType = {
  Registration: 0,
  Insurance: 1,
  Contract: 2,
  Inspection: 3,
  Other: 4,
} as const;
export type DocumentType = (typeof DocumentType)[keyof typeof DocumentType];

export const NotificationType = {
  Info: 0,
  Warning: 1,
  Success: 2,
  Error: 3,
} as const;
export type NotificationType =
  (typeof NotificationType)[keyof typeof NotificationType];

export const Role = {
  Admin: "Admin",
  Manager: "Manager",
  Driver: "Driver",
} as const;
export type Role = (typeof Role)[keyof typeof Role];

/* ── Envelopes ───────────────────────────────────────────────────────────── */

export interface ApiResponse<T = unknown> {
  success: boolean;
  message: string;
  data?: T;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

/** Paging, search and sort accepted by every list endpoint. */
export interface QueryParameters {
  page?: number;
  /** Server caps this at 100. */
  pageSize?: number;
  search?: string;
  sortBy?: string;
  descending?: boolean;
}

/** Every entity carries the soft-delete bookkeeping from BaseEntity. */
interface Audited {
  id: string;
  createdAt: string;
  updatedAt?: string | null;
  deletedAt?: string | null;
  isDeleted: boolean;
}

/* ── Auth ────────────────────────────────────────────────────────────────── */

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: string;
  userName: string;
  email: string;
  roles: Role[];
}

export interface CurrentUser {
  id: string;
  userName: string;
  email: string;
  /** Set only for accounts linked to a driver record. */
  driverId?: string | null;
  roles: Role[];
}

export interface LoginRequest {
  userName: string;
  password: string;
}

export interface RegisterRequest {
  userName: string;
  email: string;
  password: string;
  confirmPassword: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}

/** Returned by GET /api/auths/users (added to the API alongside this client). */
export interface UserListItem {
  id: string;
  userName: string;
  email: string;
  driverId?: string | null;
  driverFullName?: string | null;
  roles: Role[];
}

export interface UserQuery extends QueryParameters {
  role?: Role;
}

/* ── Vehicle ─────────────────────────────────────────────────────────────── */

export interface Vehicle extends Audited {
  image?: string | null;
  /** Note the casing — see the file header. */
  imageURL?: string | null;
  brand: string;
  model: string;
  year: number;
  plateNumber: string;
  vin: string;
  color: string;
  fuelType: FuelType;
  mileage: number;
  status: VehicleStatus;
}

export interface VehicleQuery extends QueryParameters {
  status?: VehicleStatus;
  fuelType?: FuelType;
  brand?: string;
  minYear?: number;
  maxYear?: number;
}

/* ── Driver ──────────────────────────────────────────────────────────────── */

export interface Driver extends Audited {
  firstName: string;
  lastName: string;
  fullName?: string | null;
  phoneNumber: string;
  email: string;
  identityNumber: string;
  driverLicenseNumber: string;
  licenseExpireDate: string;
  birthDate: string;
  hireDate: string;
  address?: string | null;
  image?: string | null;
  imageUrl?: string | null;
  isActive: boolean;
}

export interface DriverQuery extends QueryParameters {
  isActive?: boolean;
  licenseExpiresBefore?: string;
}

/* ── Vehicle assignment ──────────────────────────────────────────────────── */

export interface VehicleAssignment extends Audited {
  vehicleId: string;
  vehiclePlateNumber?: string | null;
  vehicleName?: string | null;
  driverId: string;
  driverFullName?: string | null;
  assignedDate: string;
  returnedDate?: string | null;
  isActive: boolean;
  note?: string | null;
}

export interface VehicleAssignmentQuery extends QueryParameters {
  vehicleId?: string;
  driverId?: string;
  isActive?: boolean;
  /** true → only handovers with no return recorded. */
  isOpen?: boolean;
}

/* ── Expense ─────────────────────────────────────────────────────────────── */

export interface Expense extends Audited {
  vehicleId: string;
  vehiclePlateNumber?: string | null;
  vehicleName?: string | null;
  category: ExpenseCategory;
  amount: number;
  expenseDate: string;
  description: string;
}

export interface ExpenseQuery extends QueryParameters {
  vehicleId?: string;
  category?: ExpenseCategory;
  from?: string;
  to?: string;
  minAmount?: number;
  maxAmount?: number;
}

/* ── Income ──────────────────────────────────────────────────────────────── */

export interface Income extends Audited {
  vehicleId: string;
  vehiclePlateNumber?: string | null;
  vehicleName?: string | null;
  driverId: string;
  driverFullName?: string | null;
  amount: number;
  incomeDate: string;
  description?: string | null;
}

export interface IncomeQuery extends QueryParameters {
  vehicleId?: string;
  driverId?: string;
  from?: string;
  to?: string;
  minAmount?: number;
  maxAmount?: number;
}

/* ── Fuel log ────────────────────────────────────────────────────────────── */

export interface FuelLog extends Audited {
  vehicleId: string;
  vehiclePlateNumber?: string | null;
  vehicleName?: string | null;
  liters: number;
  price: number;
  fuelDate: string;
  mileage: number;
  stationName: string;
}

export interface FuelLogQuery extends QueryParameters {
  vehicleId?: string;
  from?: string;
  to?: string;
}

/* ── Maintenance ─────────────────────────────────────────────────────────── */

export interface Maintenance extends Audited {
  vehicleId: string;
  vehiclePlateNumber?: string | null;
  vehicleName?: string | null;
  serviceType: MaintenanceType;
  description?: string | null;
  cost: number;
  maintenanceDate: string;
  nextMaintenanceDate?: string | null;
  mileage: number;
  serviceCenter: string;
}

export interface MaintenanceQuery extends QueryParameters {
  vehicleId?: string;
  serviceType?: MaintenanceType;
  from?: string;
  to?: string;
  dueBefore?: string;
}

/* ── Insurance ───────────────────────────────────────────────────────────── */

export interface Insurance extends Audited {
  vehicleId: string;
  vehiclePlateNumber?: string | null;
  vehicleName?: string | null;
  companyName: string;
  policyNumber: string;
  startDate: string;
  endDate: string;
  price: number;
}

export interface InsuranceQuery extends QueryParameters {
  vehicleId?: string;
  expiresBefore?: string;
  activeOn?: string;
}

/* ── Vehicle document ────────────────────────────────────────────────────── */

export interface VehicleDocument extends Audited {
  vehicleId: string;
  vehiclePlateNumber?: string | null;
  vehicleName?: string | null;
  title: string;
  documentType: DocumentType;
  fileName: string;
  fileUrl: string;
  uploadDate: string;
  /** Null for a document that does not expire. */
  expiryDate?: string | null;
}

export interface VehicleDocumentQuery extends QueryParameters {
  vehicleId?: string;
  documentType?: DocumentType;
}

/* ── Notification ────────────────────────────────────────────────────────── */

export interface Notification extends Audited {
  title: string;
  message: string;
  type: NotificationType;
  isRead: boolean;
  notificationDate: string;
}

export interface NotificationQuery extends QueryParameters {
  type?: NotificationType;
  isRead?: boolean;
  from?: string;
  to?: string;
}

/* ── Dashboard ───────────────────────────────────────────────────────────── */

export interface Upcoming {
  withinDays: number;
  expiringInsurances: number;
  expiringLicenses: number;
  dueMaintenances: number;
  expiringDocuments: number;
  overdueTotal: number;
}

export interface MonthlyTotal {
  year: number;
  month: number;
  /** "2026-08" — ready to use as a chart label. */
  label: string;
  income: number;
  expense: number;
  profit: number;
}

export interface VehicleCost {
  vehicleId: string;
  plateNumber: string;
  vehicleName: string;
  totalCost: number;
}

export interface Dashboard {
  totalVehicles: number;
  totalDrivers: number;
  activeVehicles: number;
  activeDrivers: number;
  totalIncome: number;
  totalExpense: number;
  totalNotifications: number;
  profit: number;
  unreadNotifications: number;
  assignedVehicles: number;
  totalFuelCost: number;
  totalMaintenanceCost: number;
  upcoming: Upcoming;
  monthlyTotals: MonthlyTotal[];
  topSpendingVehicles: VehicleCost[];
}
