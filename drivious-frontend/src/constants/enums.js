/**
 * The API serialises enums as integers. These mirror Drivious.Enums - changing
 * a value here without changing it there silently mislabels every row.
 */

export const ROLES = {
  Admin: "Admin",
  Manager: "Manager",
  Driver: "Driver",
};

export const FUEL_TYPE = {
  0: "Benzin",
  1: "Dizel",
  2: "Hibrid",
  3: "Elektrik",
};

export const VEHICLE_STATUS = {
  0: "Aktiv",
  1: "Servisdə",
  2: "Deaktiv",
};

export const EXPENSE_CATEGORY = {
  0: "Yanacaq",
  1: "Yağ",
  2: "Servis",
  3: "Təmir",
  4: "Təkər",
  5: "Cərimə",
  6: "Digər",
};

export const MAINTENANCE_TYPE = {
  0: "Yağ dəyişimi",
  1: "Təkər dəyişimi",
  2: "Əyləc servisi",
  3: "Mühərrik təmiri",
  4: "Akkumulyator dəyişimi",
  5: "Filtr dəyişimi",
  6: "Digər",
};

export const DOCUMENT_TYPE = {
  0: "Texniki pasport",
  1: "Sığorta",
  2: "Müqavilə",
  3: "Texniki baxış",
  4: "Digər",
};

export const NOTIFICATION_TYPE = {
  0: "Məlumat",
  1: "Xəbərdarlıq",
  2: "Uğurlu",
  3: "Xəta",
};

/** Colour hint per notification type, for badges. */
export const NOTIFICATION_TONE = {
  0: "info",
  1: "warning",
  2: "success",
  3: "error",
};

/** Turns a lookup object into [{ value, label }] for a <select>. */
export function options(lookup) {
  return Object.entries(lookup).map(([value, label]) => ({
    value: Number(value),
    label,
  }));
}
