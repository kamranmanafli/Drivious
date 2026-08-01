import { api, toFormData, unwrap } from "./client";

/**
 * Every resource exposes the same seven routes, so one factory covers all of
 * them. `multipart` marks the three that take file uploads.
 */
function resource(path, { multipart = false } = {}) {
  const body = (values) => (multipart ? toFormData(values) : values);

  return {
    /** @returns {Promise<{items: any[], page: number, pageSize: number, totalCount: number, totalPages: number, hasPrevious: boolean, hasNext: boolean}>} */
    list: (params) => api.get(`/api/${path}`, { params }).then(unwrap),

    deleted: (params) => api.get(`/api/${path}/deleted`, { params }).then(unwrap),

    get: (id) => api.get(`/api/${path}/${id}`).then(unwrap),

    create: (values) => api.post(`/api/${path}`, body(values)).then((r) => r.data),

    update: (id, values) => api.patch(`/api/${path}/${id}`, body(values)).then((r) => r.data),

    /** Archives or restores. This is the everyday delete. */
    toggle: (id) => api.patch(`/api/${path}/toggle/${id}`).then((r) => r.data),

    /** Permanent, Admin only, and refused when the row still has history. */
    remove: (id) => api.delete(`/api/${path}/${id}`).then((r) => r.data),
  };
}

export const vehicles = resource("vehicles", { multipart: true });
export const drivers = resource("drivers", { multipart: true });
export const vehicleDocuments = resource("vehicledocuments", { multipart: true });

export const expenses = resource("expenses");
export const incomes = resource("incomes");
export const maintenances = resource("maintenances");
export const insurances = resource("insurances");
export const fuelLogs = resource("fuellogs");
export const notifications = resource("notifications");

export const vehicleAssignments = {
  ...resource("vehicleassignments"),

  /** Records a vehicle coming back. Omit the date to use now. */
  return: (id, returnedDate) =>
    api
      .patch(`/api/vehicleassignments/return/${id}`, null, {
        params: returnedDate ? { returnedDate } : undefined,
      })
      .then((r) => r.data),
};

export const dashboard = {
  get: () => api.get("/api/dashboards").then(unwrap),
};

export const notificationScan = {
  /** Runs the expiry scan now instead of waiting for the background pass. */
  run: () => api.post("/api/notifications/scan").then((r) => r.data),
};

export const auth = {
  register: (values) => api.post("/api/auths/register", values).then((r) => r.data),

  login: (userName, password) =>
    api.post("/api/auths/login", { userName, password }).then(unwrap),

  logout: (refreshToken) =>
    api.post("/api/auths/logout", { refreshToken }).then((r) => r.data),

  me: () => api.get("/api/auths/me").then(unwrap),

  changePassword: (values) =>
    api.post("/api/auths/change-password", values).then((r) => r.data),

  assignRole: (userName, role) =>
    api.post("/api/auths/assign-role", { userName, role }).then((r) => r.data),

  linkDriver: (userName, driverId) =>
    api.post("/api/auths/link-driver", { userName, driverId }).then((r) => r.data),
};
