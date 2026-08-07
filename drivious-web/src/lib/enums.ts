import {
  DocumentType,
  ExpenseCategory,
  FuelType,
  MaintenanceType,
  NotificationType,
  VehicleStatus,
} from "@/api/types";

/** The tone a badge uses. Kept separate from the label so the two can differ. */
export type Tone = "neutral" | "primary" | "success" | "warning" | "danger" | "info";

export interface EnumEntry<T extends number> {
  value: T;
  label: string;
  tone: Tone;
}

function build<T extends number>(entries: Array<[T, string, Tone]>) {
  const list = entries.map(([value, label, tone]) => ({ value, label, tone }));
  const byValue = new Map(list.map((e) => [e.value, e]));

  return {
    list,
    /** Falls back rather than rendering "undefined" if the API adds a member. */
    label: (value: T | null | undefined) =>
      value == null ? "—" : (byValue.get(value)?.label ?? `#${value}`),
    tone: (value: T | null | undefined): Tone =>
      value == null ? "neutral" : (byValue.get(value)?.tone ?? "neutral"),
  };
}

export const fuelTypes = build<FuelType>([
  [FuelType.Gasoline, "Benzin", "neutral"],
  [FuelType.Diesel, "Dizel", "neutral"],
  [FuelType.Hybrid, "Hibrid", "info"],
  [FuelType.Electric, "Elektrik", "success"],
]);

export const vehicleStatuses = build<VehicleStatus>([
  [VehicleStatus.Active, "Aktiv", "success"],
  [VehicleStatus.InService, "Servisdə", "warning"],
  [VehicleStatus.Inactive, "Deaktiv", "neutral"],
]);

export const expenseCategories = build<ExpenseCategory>([
  [ExpenseCategory.Fuel, "Yanacaq", "info"],
  [ExpenseCategory.Oil, "Yağ", "neutral"],
  [ExpenseCategory.Service, "Servis", "warning"],
  [ExpenseCategory.Repair, "Təmir", "warning"],
  [ExpenseCategory.Tire, "Təkər", "neutral"],
  [ExpenseCategory.Fine, "Cərimə", "danger"],
  [ExpenseCategory.Other, "Digər", "neutral"],
]);

export const maintenanceTypes = build<MaintenanceType>([
  [MaintenanceType.OilChange, "Yağ dəyişimi", "neutral"],
  [MaintenanceType.TireChange, "Təkər dəyişimi", "neutral"],
  [MaintenanceType.BrakeService, "Əyləc servisi", "warning"],
  [MaintenanceType.EngineRepair, "Mühərrik təmiri", "danger"],
  [MaintenanceType.BatteryReplacement, "Akkumulyator dəyişimi", "neutral"],
  [MaintenanceType.FilterReplacement, "Filtr dəyişimi", "neutral"],
  [MaintenanceType.Other, "Digər", "neutral"],
]);

export const documentTypes = build<DocumentType>([
  [DocumentType.Registration, "Texniki pasport", "neutral"],
  [DocumentType.Insurance, "Sığorta", "info"],
  [DocumentType.Contract, "Müqavilə", "neutral"],
  [DocumentType.Inspection, "Texniki baxış", "warning"],
  [DocumentType.Other, "Digər", "neutral"],
]);

export const notificationTypes = build<NotificationType>([
  [NotificationType.Info, "Məlumat", "info"],
  [NotificationType.Warning, "Xəbərdarlıq", "warning"],
  [NotificationType.Success, "Uğurlu", "success"],
  [NotificationType.Error, "Xəta", "danger"],
]);

export const roleLabels: Record<string, string> = {
  Admin: "Administrator",
  Manager: "Menecer",
  Driver: "Sürücü",
};
