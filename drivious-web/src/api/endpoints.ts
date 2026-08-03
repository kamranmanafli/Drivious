import { api, toFormData, unwrap, unwrapMessage } from "./client";
import type {
  ApiResponse,
  AuthResponse,
  ChangePasswordRequest,
  CurrentUser,
  Dashboard,
  Driver,
  DriverQuery,
  Expense,
  ExpenseQuery,
  FuelLog,
  FuelLogQuery,
  Income,
  IncomeQuery,
  Insurance,
  InsuranceQuery,
  Maintenance,
  MaintenanceQuery,
  Notification,
  NotificationQuery,
  PagedResult,
  QueryParameters,
  RegisterRequest,
  Role,
  UserListItem,
  UserQuery,
  Vehicle,
  VehicleAssignment,
  VehicleAssignmentQuery,
  VehicleDocument,
  VehicleDocumentQuery,
  VehicleQuery,
} from "./types";

/**
 * Every resource exposes the same seven routes, so one factory covers all of
 * them. `multipart` marks the three that take an upload — the API binds those
 * with [FromForm] and rejects a JSON body.
 */
function resource<TItem, TQuery extends QueryParameters, TCreate, TUpdate>(
  path: string,
  { multipart = false } = {},
) {
  const encode = (values: TCreate | TUpdate) =>
    multipart ? toFormData(values as Record<string, unknown>) : values;

  return {
    path,

    list: (params?: TQuery) =>
      api
        .get<ApiResponse<PagedResult<TItem>>>(`/api/${path}`, { params })
        .then(unwrap<PagedResult<TItem>>),

    /** Archived rows. Manager and Admin only. */
    deleted: (params?: TQuery) =>
      api
        .get<ApiResponse<PagedResult<TItem>>>(`/api/${path}/deleted`, { params })
        .then(unwrap<PagedResult<TItem>>),

    get: (id: string) =>
      api.get<ApiResponse<TItem>>(`/api/${path}/${id}`).then(unwrap<TItem>),

    create: (values: TCreate) =>
      api.post<ApiResponse<TItem>>(`/api/${path}`, encode(values)).then(unwrapMessage),

    update: (id: string, values: TUpdate) =>
      api.patch<ApiResponse<TItem>>(`/api/${path}/${id}`, encode(values)).then(unwrapMessage),

    /** Archives or restores. This is the everyday delete. */
    toggle: (id: string) =>
      api.patch<ApiResponse<TItem>>(`/api/${path}/toggle/${id}`).then(unwrapMessage),

    /** Permanent, Admin only, and refused while the row still has history. */
    remove: (id: string) =>
      api.delete<ApiResponse<never>>(`/api/${path}/${id}`).then(unwrapMessage),
  };
}

/* ── Payload shapes ──────────────────────────────────────────────────────── */

export interface VehicleInput {
  image?: File;
  brand: string;
  model: string;
  year: number;
  plateNumber: string;
  vin: string;
  color: string;
  fuelType: number;
  mileage: number;
  status: number;
}

export interface DriverInput {
  image?: File;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  email: string;
  identityNumber: string;
  driverLicenseNumber: string;
  licenseExpireDate: string;
  birthDate: string;
  hireDate: string;
  address: string;
  isActive: boolean;
}

export interface AssignmentInput {
  vehicleId: string;
  driverId: string;
  assignedDate: string;
  returnedDate?: string | null;
  isActive: boolean;
  note?: string | null;
}

export interface ExpenseInput {
  vehicleId: string;
  category: number;
  amount: number;
  expenseDate: string;
  description: string;
}

export interface IncomeInput {
  vehicleId: string;
  driverId: string;
  amount: number;
  incomeDate: string;
  description?: string | null;
}

export interface FuelLogInput {
  vehicleId: string;
  liters: number;
  price: number;
  fuelDate: string;
  mileage: number;
  stationName: string;
}

export interface MaintenanceInput {
  vehicleId: string;
  serviceType: number;
  description?: string | null;
  cost: number;
  maintenanceDate: string;
  nextMaintenanceDate?: string | null;
  mileage: number;
  serviceCenter: string;
}

export interface InsuranceInput {
  vehicleId: string;
  companyName: string;
  policyNumber: string;
  startDate: string;
  endDate: string;
  price: number;
}

export interface DocumentInput {
  vehicleId: string;
  title: string;
  documentType: number;
  expiryDate?: string | null;
  file?: File;
}

export interface NotificationInput {
  title: string;
  message: string;
  type: number;
  isRead: boolean;
  notificationDate: string;
}

/* ── Resources ───────────────────────────────────────────────────────────── */

export const vehicles = resource<Vehicle, VehicleQuery, VehicleInput, Partial<VehicleInput>>(
  "vehicles",
  { multipart: true },
);

export const drivers = resource<Driver, DriverQuery, DriverInput, Partial<DriverInput>>(
  "drivers",
  { multipart: true },
);

export const vehicleDocuments = resource<
  VehicleDocument,
  VehicleDocumentQuery,
  DocumentInput,
  Partial<DocumentInput>
>("vehicledocuments", { multipart: true });

export const expenses = resource<Expense, ExpenseQuery, ExpenseInput, Partial<ExpenseInput>>(
  "expenses",
);

export const incomes = resource<Income, IncomeQuery, IncomeInput, Partial<IncomeInput>>("incomes");

export const fuelLogs = resource<FuelLog, FuelLogQuery, FuelLogInput, Partial<FuelLogInput>>(
  "fuellogs",
);

export const maintenances = resource<
  Maintenance,
  MaintenanceQuery,
  MaintenanceInput,
  Partial<MaintenanceInput>
>("maintenances");

export const insurances = resource<
  Insurance,
  InsuranceQuery,
  InsuranceInput,
  Partial<InsuranceInput>
>("insurances");

export const notifications = {
  ...resource<Notification, NotificationQuery, NotificationInput, Partial<NotificationInput>>(
    "notifications",
  ),

  /** Runs the expiry scan now instead of waiting for the background pass. */
  scan: () => api.post<ApiResponse<never>>("/api/notifications/scan").then(unwrapMessage),

  markRead: (id: string, isRead: boolean) =>
    api
      .patch<ApiResponse<Notification>>(`/api/notifications/${id}`, { isRead })
      .then(unwrapMessage),
};

export const vehicleAssignments = {
  ...resource<
    VehicleAssignment,
    VehicleAssignmentQuery,
    AssignmentInput,
    Partial<AssignmentInput>
  >("vehicleassignments"),

  /** Records a vehicle coming back. Omit the date to use now. */
  return: (id: string, returnedDate?: string) =>
    api
      .patch<ApiResponse<VehicleAssignment>>(
        `/api/vehicleassignments/return/${id}`,
        null,
        { params: returnedDate ? { returnedDate } : undefined },
      )
      .then(unwrapMessage),
};

export const dashboard = {
  get: () => api.get<ApiResponse<Dashboard>>("/api/dashboards").then(unwrap<Dashboard>),
};

export const auth = {
  register: (values: RegisterRequest) =>
    api.post<ApiResponse<never>>("/api/auths/register", values).then(unwrapMessage),

  login: (userName: string, password: string) =>
    api
      .post<ApiResponse<AuthResponse>>("/api/auths/login", { userName, password })
      .then(unwrap<AuthResponse>),

  logout: (refreshToken: string) =>
    api.post<ApiResponse<never>>("/api/auths/logout", { refreshToken }).then(unwrapMessage),

  me: () => api.get<ApiResponse<CurrentUser>>("/api/auths/me").then(unwrap<CurrentUser>),

  changePassword: (values: ChangePasswordRequest) =>
    api.post<ApiResponse<never>>("/api/auths/change-password", values).then(unwrapMessage),

  /** Admin only. Added to the API alongside this client. */
  users: (params?: UserQuery) =>
    api
      .get<ApiResponse<PagedResult<UserListItem>>>("/api/auths/users", { params })
      .then(unwrap<PagedResult<UserListItem>>),

  assignRole: (userName: string, role: Role) =>
    api.post<ApiResponse<never>>("/api/auths/assign-role", { userName, role }).then(unwrapMessage),

  linkDriver: (userName: string, driverId: string | null) =>
    api
      .post<ApiResponse<never>>("/api/auths/link-driver", { userName, driverId })
      .then(unwrapMessage),
};
