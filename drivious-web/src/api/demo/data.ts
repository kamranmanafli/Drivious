/**
 * Sample fleet for demo mode.
 *
 * Generated from a fixed seed, so the numbers on screen do not change between
 * reloads and a screenshot stays reproducible. Dates are relative to today, so
 * the expiry warnings stay meaningful however long after this was written the
 * demo is opened.
 */

import {
  DocumentType,
  ExpenseCategory,
  FuelType,
  MaintenanceType,
  NotificationType,
  Role,
  VehicleStatus,
  type Driver,
  type Expense,
  type FuelLog,
  type Income,
  type Insurance,
  type Maintenance,
  type Notification,
  type Vehicle,
  type VehicleAssignment,
  type VehicleDocument,
} from "../types";

/* ── Deterministic randomness ────────────────────────────────────────────── */

function mulberry32(seed: number) {
  return function () {
    seed |= 0;
    seed = (seed + 0x6d2b79f5) | 0;
    let t = Math.imul(seed ^ (seed >>> 15), 1 | seed);
    t = (t + Math.imul(t ^ (t >>> 7), 61 | t)) ^ t;
    return ((t ^ (t >>> 14)) >>> 0) / 4294967296;
  };
}

const rand = mulberry32(20260802);

const pick = <T>(items: readonly T[]): T => items[Math.floor(rand() * items.length)];
const int = (min: number, max: number) => Math.floor(rand() * (max - min + 1)) + min;
const money = (min: number, max: number) => Math.round((rand() * (max - min) + min) * 100) / 100;

const uuid = (() => {
  let n = 0;
  return () => {
    n += 1;
    const hex = n.toString(16).padStart(12, "0");
    return `00000000-0000-4000-8000-${hex}`;
  };
})();

/* ── Dates ───────────────────────────────────────────────────────────────── */

const TODAY = new Date();
TODAY.setHours(0, 0, 0, 0);

function shift(days: number, from: Date = TODAY): string {
  const d = new Date(from);
  d.setDate(d.getDate() + days);
  return d.toISOString();
}

const daysAgo = (n: number) => shift(-n);
const daysAhead = (n: number) => shift(n);

/* ── Vehicle & driver source material ────────────────────────────────────── */

const MODELS: Array<[string, string, FuelType]> = [
  ["Toyota", "Corolla", FuelType.Gasoline],
  ["Toyota", "Camry", FuelType.Hybrid],
  ["Hyundai", "Elantra", FuelType.Gasoline],
  ["Hyundai", "Tucson", FuelType.Diesel],
  ["Kia", "Sportage", FuelType.Diesel],
  ["Kia", "Cerato", FuelType.Gasoline],
  ["Mercedes-Benz", "Sprinter", FuelType.Diesel],
  ["Ford", "Transit", FuelType.Diesel],
  ["Volkswagen", "Passat", FuelType.Diesel],
  ["Nissan", "Qashqai", FuelType.Gasoline],
  ["BYD", "Song Plus", FuelType.Electric],
  ["Chevrolet", "Cobalt", FuelType.Gasoline],
];

const COLORS = ["Ağ", "Qara", "Gümüşü", "Boz", "Göy", "Qırmızı"];

const FIRST_NAMES = [
  "Elvin", "Rəşad", "Nicat", "Kamran", "Orxan", "Tural", "Vüqar",
  "Samir", "Fərid", "Anar", "Emil", "Ramil",
];

const LAST_NAMES = [
  "Məmmədov", "Əliyev", "Hüseynov", "Quliyev", "Həsənov", "İsmayılov",
  "Rzayev", "Cəfərov", "Abbasov", "Kərimov",
];

const STATIONS = ["SOCAR", "Lukoil", "Azpetrol", "Azərnəqliyyat", "NP Petrol"];
const SERVICE_CENTERS = ["AutoLux Servis", "Master Auto", "Bakı Avto Servis", "TexnoCar"];
const INSURERS = ["PAŞA Sığorta", "AzSığorta", "Ateşgah Sığorta", "Mega Sığorta"];

/** A tinted gradient stands in for a photo — self-contained and clearly a placeholder. */
function placeholder(label: string, hue: number): string {
  const svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 400 260">
<defs><linearGradient id="g" x1="0" y1="0" x2="1" y2="1">
<stop offset="0" stop-color="hsl(${hue} 42% 62%)"/>
<stop offset="1" stop-color="hsl(${(hue + 40) % 360} 38% 40%)"/>
</linearGradient></defs>
<rect width="400" height="260" fill="url(#g)"/>
<text x="200" y="142" font-family="Inter,sans-serif" font-size="34" font-weight="600"
 fill="rgba(255,255,255,.92)" text-anchor="middle">${label}</text>
</svg>`;
  return `data:image/svg+xml;utf8,${encodeURIComponent(svg)}`;
}

const audit = (createdDaysAgo: number) => ({
  createdAt: daysAgo(createdDaysAgo),
  updatedAt: null,
  deletedAt: null,
  isDeleted: false,
});

/* ── Vehicles ────────────────────────────────────────────────────────────── */

const PLATE_LETTERS = ["AB", "CD", "EL", "GH", "KM", "NR", "SV", "TZ"];

export const vehicles: Vehicle[] = MODELS.map(([brand, model, fuelType], i) => {
  const plate = `${10 + i}-${PLATE_LETTERS[i % PLATE_LETTERS.length]}-${100 + i * 37}`;

  // One vehicle in service, one inactive, the rest active — enough variety to
  // exercise every badge without making the fleet look broken.
  const status =
    i === 3 ? VehicleStatus.InService : i === 9 ? VehicleStatus.Inactive : VehicleStatus.Active;

  return {
    id: uuid(),
    ...audit(int(200, 900)),
    image: "demo.svg",
    imageURL: placeholder(`${brand}`, (i * 37) % 360),
    brand,
    model,
    year: int(2016, 2024),
    plateNumber: plate,
    vin: `JT${String(i).padStart(2, "0")}${"ABCDEFGHJKLMNPRSTUVWXYZ0123456789"
      .split("")
      .sort(() => rand() - 0.5)
      .slice(0, 13)
      .join("")}`.slice(0, 17),
    color: pick(COLORS),
    fuelType,
    mileage: int(35_000, 240_000),
    status,
  };
});

/* ── Drivers ─────────────────────────────────────────────────────────────── */

export const drivers: Driver[] = Array.from({ length: 9 }, (_, i) => {
  const firstName = FIRST_NAMES[i % FIRST_NAMES.length];
  const lastName = LAST_NAMES[i % LAST_NAMES.length];

  // Two licences land inside the 30-day warning window and one is already past,
  // so the dashboard's expiry tiles and the notification list have real content.
  const licenceDays = i === 0 ? 12 : i === 1 ? 26 : i === 2 ? -9 : int(200, 1400);

  return {
    id: uuid(),
    ...audit(int(150, 800)),
    firstName,
    lastName,
    fullName: `${firstName} ${lastName}`,
    phoneNumber: `+9945${int(0, 9)}${String(int(1000000, 9999999))}`.slice(0, 13),
    email: `${firstName.toLowerCase()}.${lastName.toLowerCase()}@drivious.az`
      .replace(/ə/g, "e")
      .replace(/ü/g, "u")
      .replace(/ı/g, "i")
      .replace(/ö/g, "o")
      .replace(/ç/g, "c")
      .replace(/ş/g, "s")
      .replace(/ğ/g, "g"),
    identityNumber: `${String(int(1000000, 9999999))}`,
    driverLicenseNumber: `AZ${int(100000, 999999)}`,
    licenseExpireDate: shift(licenceDays),
    birthDate: shift(-int(8000, 17000)),
    hireDate: shift(-int(100, 2000)),
    address: `Bakı, ${pick(["Nəsimi", "Yasamal", "Xətai", "Nizami", "Sabunçu"])} r., ${int(1, 90)}-cı ev`,
    imageUrl: null,
    image: null,
    isActive: i !== 7,
  };
});

/* ── Assignments ─────────────────────────────────────────────────────────── */

export const assignments: VehicleAssignment[] = [];

// Six open handovers — one vehicle per driver, which is what the API enforces.
for (let i = 0; i < 6; i++) {
  const vehicle = vehicles[i];
  const driver = drivers[i];
  const assigned = int(20, 180);

  assignments.push({
    id: uuid(),
    ...audit(assigned),
    vehicleId: vehicle.id,
    vehiclePlateNumber: vehicle.plateNumber,
    vehicleName: `${vehicle.brand} ${vehicle.model}`,
    driverId: driver.id,
    driverFullName: driver.fullName,
    assignedDate: daysAgo(assigned),
    returnedDate: null,
    isActive: true,
    note: i === 0 ? "Uzunmüddətli təyinat" : null,
  });
}

// Plus some closed history, so the list is not all open rows.
for (let i = 0; i < 8; i++) {
  const vehicle = pick(vehicles);
  const driver = pick(drivers);
  const start = int(250, 700);
  const end = start - int(30, 200);

  assignments.push({
    id: uuid(),
    ...audit(start),
    vehicleId: vehicle.id,
    vehiclePlateNumber: vehicle.plateNumber,
    vehicleName: `${vehicle.brand} ${vehicle.model}`,
    driverId: driver.id,
    driverFullName: driver.fullName,
    assignedDate: daysAgo(start),
    returnedDate: daysAgo(Math.max(end, 1)),
    isActive: false,
    note: null,
  });
}

/* ── Money ───────────────────────────────────────────────────────────────── */

const link = (v: Vehicle) => ({
  vehicleId: v.id,
  vehiclePlateNumber: v.plateNumber,
  vehicleName: `${v.brand} ${v.model}`,
});

const EXPENSE_NOTES: Record<number, string[]> = {
  [ExpenseCategory.Fuel]: ["Yanacaq doldurma", "Marşrut üçün yanacaq"],
  [ExpenseCategory.Oil]: ["Mühərrik yağı alışı", "Yağ və filtr dəsti"],
  [ExpenseCategory.Service]: ["Planlı texniki baxış", "Diaqnostika"],
  [ExpenseCategory.Repair]: ["Ön amortizator dəyişimi", "Kondisioner təmiri"],
  [ExpenseCategory.Tire]: ["Qış təkərləri dəsti", "Təkər balansı"],
  [ExpenseCategory.Fine]: ["Sürət həddi cəriməsi", "Parklanma cəriməsi"],
  [ExpenseCategory.Other]: ["Yuma və salon təmizliyi", "Avtomobil aksesuarı"],
};

export const expenses: Expense[] = Array.from({ length: 140 }, () => {
  const vehicle = pick(vehicles);
  const category = pick(Object.values(ExpenseCategory)) as ExpenseCategory;
  const age = int(0, 200);

  return {
    id: uuid(),
    ...audit(age),
    ...link(vehicle),
    category,
    amount: money(25, category === ExpenseCategory.Repair ? 1400 : 320),
    expenseDate: daysAgo(age),
    description: pick(EXPENSE_NOTES[category]),
  };
});

function makeIncome(assignment: VehicleAssignment, age: number): Income {
  const vehicle = vehicles.find((v) => v.id === assignment.vehicleId)!;
  const driver = drivers.find((d) => d.id === assignment.driverId)!;

  return {
    id: uuid(),
    ...audit(age),
    ...link(vehicle),
    driverId: driver.id,
    driverFullName: driver.fullName,
    amount: money(80, 950),
    incomeDate: daysAgo(age),
    description: pick(["Sifariş gəliri", "Korporativ marşrut", "Günlük dövriyyə", null]),
  };
}

export const incomes: Income[] = [
  ...Array.from({ length: 190 }, () => makeIncome(pick(assignments), int(0, 200))),

  // Spread over the last two weeks across the open handovers, so "this month"
  // on the driver's home screen is not empty on the third of the month.
  ...Array.from({ length: 24 }, (_, i) =>
    makeIncome(assignments[i % 6], int(0, Math.min(13, TODAY.getDate() + 6))),
  ),
];

export const fuelLogs: FuelLog[] = Array.from({ length: 110 }, () => {
  const vehicle = pick(vehicles);
  const age = int(0, 200);
  const liters = money(18, 62);

  return {
    id: uuid(),
    ...audit(age),
    ...link(vehicle),
    liters,
    price: Math.round(liters * (rand() * 0.5 + 1.1) * 100) / 100,
    fuelDate: daysAgo(age),
    mileage: vehicle.mileage - int(0, 12_000),
    stationName: pick(STATIONS),
  };
});

export const maintenances: Maintenance[] = Array.from({ length: 46 }, (_, i) => {
  const vehicle = pick(vehicles);
  const age = int(5, 400);

  // A few next-service dates fall inside the warning window, and two are overdue.
  const nextDays = i < 3 ? int(3, 28) : i < 5 ? -int(2, 20) : int(60, 400);

  return {
    id: uuid(),
    ...audit(age),
    ...link(vehicle),
    serviceType: pick(Object.values(MaintenanceType)) as MaintenanceType,
    description: pick(["Planlı servis", "Zəmanət işi", "Sürücü şikayəti üzrə", null]),
    cost: money(60, 1800),
    maintenanceDate: daysAgo(age),
    nextMaintenanceDate: shift(nextDays),
    mileage: vehicle.mileage - int(0, 20_000),
    serviceCenter: pick(SERVICE_CENTERS),
  };
});

export const insurances: Insurance[] = vehicles.flatMap((vehicle, i) => {
  // Two expiring soon, one already lapsed, the rest comfortably in force.
  const endDays = i === 0 ? 9 : i === 1 ? 24 : i === 2 ? -6 : int(70, 340);
  const startDays = endDays - 365;

  return [
    {
      id: uuid(),
      ...audit(Math.abs(startDays)),
      ...link(vehicle),
      companyName: pick(INSURERS),
      policyNumber: `POL-${2025 + (i % 2)}-${int(10000, 99999)}`,
      startDate: shift(startDays),
      endDate: shift(endDays),
      price: money(180, 620),
    },
  ];
});

export const documents: VehicleDocument[] = vehicles.slice(0, 10).flatMap((vehicle, i) => {
  const type = (i % 5) as DocumentType;
  const expiryDays = i === 0 ? 17 : i === 1 ? -4 : i % 3 === 0 ? int(90, 500) : null;

  return [
    {
      id: uuid(),
      ...audit(int(30, 500)),
      ...link(vehicle),
      title: ["Texniki pasport", "Sığorta şəhadətnaməsi", "İcarə müqaviləsi", "Texniki baxış aktı", "Digər sənəd"][i % 5],
      documentType: type,
      fileName: `document-${i + 1}.pdf`,
      fileUrl: "#demo",
      uploadDate: daysAgo(int(30, 500)),
      expiryDate: expiryDays === null ? null : shift(expiryDays),
    },
  ];
});

/* ── Notifications ───────────────────────────────────────────────────────── */

/** Mirrors NotificationGenerator: a date still ahead warns, a passed one errors. */
function describe(dateIso: string) {
  const target = new Date(dateIso);
  target.setHours(0, 0, 0, 0);
  const days = Math.round((target.getTime() - TODAY.getTime()) / 86_400_000);

  return days < 0
    ? { state: "expired", type: NotificationType.Error, phrase: `${-days} gün əvvəl bitib` }
    : { state: "expiring", type: NotificationType.Warning, phrase: `${days} gün sonra bitir` };
}

const LEAD_DAYS = 30;

function withinWindow(dateIso: string | null | undefined): boolean {
  if (!dateIso) return false;
  const target = new Date(dateIso);
  target.setHours(0, 0, 0, 0);
  const days = Math.round((target.getTime() - TODAY.getTime()) / 86_400_000);
  return days >= -LEAD_DAYS && days <= LEAD_DAYS;
}

export const notifications: Notification[] = [];

for (const insurance of insurances) {
  if (!withinWindow(insurance.endDate)) continue;
  const { state, type, phrase } = describe(insurance.endDate);

  notifications.push({
    id: uuid(),
    ...audit(0),
    title: state === "expired" ? "Sığorta bitib" : "Sığorta bitmək üzrədir",
    message: `${insurance.vehiclePlateNumber} üçün ${insurance.companyName} polisi (${insurance.policyNumber}) ${phrase}.`,
    type,
    isRead: false,
    notificationDate: insurance.endDate,
  });
}

for (const driver of drivers) {
  if (!withinWindow(driver.licenseExpireDate)) continue;
  const { state, type, phrase } = describe(driver.licenseExpireDate);

  notifications.push({
    id: uuid(),
    ...audit(0),
    title: state === "expired" ? "Vəsiqə bitib" : "Vəsiqə bitmək üzrədir",
    message: `${driver.fullName} sürücülük vəsiqəsi (${driver.driverLicenseNumber}) ${phrase}.`,
    type,
    isRead: false,
    notificationDate: driver.licenseExpireDate,
  });
}

for (const maintenance of maintenances) {
  if (!withinWindow(maintenance.nextMaintenanceDate)) continue;
  const { state, type, phrase } = describe(maintenance.nextMaintenanceDate!);

  notifications.push({
    id: uuid(),
    ...audit(0),
    title: state === "expired" ? "Servis gecikib" : "Servis vaxtı yaxınlaşır",
    message: `${maintenance.vehiclePlateNumber} üçün planlı servis ${phrase.replace("bitib", "keçib").replace("bitir", "başlayır")}.`,
    type,
    isRead: false,
    notificationDate: maintenance.nextMaintenanceDate!,
  });
}

for (const document of documents) {
  if (!withinWindow(document.expiryDate)) continue;
  const { state, type, phrase } = describe(document.expiryDate!);

  notifications.push({
    id: uuid(),
    ...audit(0),
    title: state === "expired" ? "Sənəd bitib" : "Sənəd bitmək üzrədir",
    message: `${document.vehiclePlateNumber} üçün "${document.title}" ${phrase}.`,
    type,
    isRead: false,
    notificationDate: document.expiryDate!,
  });
}

notifications.push(
  {
    id: uuid(),
    ...audit(2),
    title: "Filoya yeni maşın əlavə edildi",
    message: `${vehicles[11].plateNumber} — ${vehicles[11].brand} ${vehicles[11].model} sistemə qeydə alındı.`,
    type: NotificationType.Success,
    isRead: true,
    notificationDate: daysAgo(2),
  },
  {
    id: uuid(),
    ...audit(6),
    title: "Aylıq hesabat hazırdır",
    message: "Keçən ayın gəlir və xərc hesabatı yaradıldı.",
    type: NotificationType.Info,
    isRead: true,
    notificationDate: daysAgo(6),
  },
);

notifications.sort(
  (a, b) => new Date(b.notificationDate).getTime() - new Date(a.notificationDate).getTime(),
);

/* ── Accounts ────────────────────────────────────────────────────────────── */

export interface DemoUser {
  id: string;
  userName: string;
  email: string;
  password: string;
  driverId: string | null;
  roles: Role[];
}

export const users: DemoUser[] = [
  {
    id: "u-admin",
    userName: "admin",
    email: "admin@drivious.az",
    password: "Admin123!",
    driverId: null,
    roles: [Role.Admin],
  },
  {
    id: "u-manager",
    userName: "menecer",
    email: "menecer@drivious.az",
    password: "Menecer123!",
    driverId: null,
    roles: [Role.Manager],
  },
  {
    id: "u-driver",
    userName: "surucu",
    email: "surucu@drivious.az",
    password: "Surucu123!",
    driverId: drivers[0].id,
    roles: [Role.Driver],
  },
];

export const LEAD_DAYS_SETTING = LEAD_DAYS;
export const today = TODAY;
export { daysAhead, daysAgo, uuid };
