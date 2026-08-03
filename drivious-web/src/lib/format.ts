/**
 * Formatting for Azerbaijani.
 *
 * Deliberately does not go through Intl with an "az-AZ" locale. Not every
 * browser ships Azerbaijani data, and the ones that do not fall back to the
 * root locale — which renders a date as "2026-08-15" and a distance as
 * "170,933 km" instead of "15.08.2026" and "170 935 km". Doing the grouping and
 * the month names here means the app reads the same everywhere.
 *
 * Conventions: `1 234,56` — space between thousands, comma before the decimals.
 */

/** Narrow no-break space: groups digits without letting them wrap apart. */
const GROUP = " ";

const MONTHS_SHORT = [
  "Yan", "Fev", "Mar", "Apr", "May", "İyn",
  "İyl", "Avq", "Sen", "Okt", "Noy", "Dek",
];

const MONTHS_LONG = [
  "yanvar", "fevral", "mart", "aprel", "may", "iyun",
  "iyul", "avqust", "sentyabr", "oktyabr", "noyabr", "dekabr",
];

const WEEKDAYS = [
  "bazar", "bazar ertəsi", "çərşənbə axşamı", "çərşənbə",
  "cümə axşamı", "cümə", "şənbə",
];

/** Groups the integer part and switches the decimal point to a comma. */
function group(value: number, decimals: number): string {
  const fixed = Math.abs(value).toFixed(decimals);
  const [whole, fraction] = fixed.split(".");

  const grouped = whole.replace(/\B(?=(\d{3})+(?!\d))/g, GROUP);
  const sign = value < 0 ? "-" : "";

  return fraction ? `${sign}${grouped},${fraction}` : `${sign}${grouped}`;
}

/** Manat. Decimals only when the amount actually has them. */
export function money(value: number | null | undefined): string {
  if (value == null || Number.isNaN(value)) return "—";
  return `${group(value, value % 1 === 0 ? 0 : 2)} ₼`;
}

/** Compact form for chart axes, where the full number would not fit. */
export function moneyShort(value: number): string {
  const abs = Math.abs(value);
  if (abs >= 1_000_000) return `${group(value / 1_000_000, 1)}M ₼`;
  if (abs >= 1_000) return `${group(Math.round(value / 1000), 0)}k ₼`;
  return `${group(value, 0)} ₼`;
}

export function number(value: number | null | undefined): string {
  if (value == null || Number.isNaN(value)) return "—";
  return group(value, 0);
}

export function liters(value: number | null | undefined): string {
  if (value == null) return "—";
  return `${group(value, value % 1 === 0 ? 0 : 2)} L`;
}

export function km(value: number | null | undefined): string {
  if (value == null) return "—";
  return `${group(value, 0)} km`;
}

function parse(value: string | Date | null | undefined): Date | null {
  if (!value) return null;
  const date = value instanceof Date ? value : new Date(value);
  return Number.isNaN(date.getTime()) ? null : date;
}

const pad = (n: number) => String(n).padStart(2, "0");

/** 02.08.2026 */
export function date(value: string | Date | null | undefined): string {
  const d = parse(value);
  if (!d) return "—";
  return `${pad(d.getDate())}.${pad(d.getMonth() + 1)}.${d.getFullYear()}`;
}

/** 02.08.2026, 14:30 */
export function dateTime(value: string | Date | null | undefined): string {
  const d = parse(value);
  if (!d) return "—";
  return `${date(d)}, ${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

/** "3 avqust, bazar ertəsi" — the driver app's greeting line. */
export function longDate(value: string | Date | null | undefined): string {
  const d = parse(value);
  if (!d) return "—";
  return `${d.getDate()} ${MONTHS_LONG[d.getMonth()]}, ${WEEKDAYS[d.getDay()]}`;
}

/** "3 gün sonra" / "2 gün əvvəl" — for expiry dates, where the gap is the point. */
export function relativeDays(value: string | Date | null | undefined): string {
  const days = daysUntil(value);
  if (days === null) return "—";

  if (days === 0) return "bu gün";
  if (days === 1) return "sabah";
  if (days === -1) return "dünən";
  if (days > 0) return `${days} gün sonra`;
  return `${Math.abs(days)} gün əvvəl`;
}

/** Days until a date; negative once it has passed. */
export function daysUntil(value: string | Date | null | undefined): number | null {
  const d = parse(value);
  if (!d) return null;

  const today = new Date();
  today.setHours(0, 0, 0, 0);
  const target = new Date(d);
  target.setHours(0, 0, 0, 0);

  return Math.round((target.getTime() - today.getTime()) / 86_400_000);
}

/** "2026-08" from the dashboard trend → "Avq" for a chart tick. */
export function monthLabel(label: string): string {
  const month = Number(label.split("-")[1]);
  return MONTHS_SHORT[month - 1] ?? label;
}

/** <input type="date"> wants yyyy-MM-dd and nothing else. */
export function toDateInput(value: string | Date | null | undefined): string {
  const d = parse(value);
  if (!d) return "";
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
}

export function initials(name: string | null | undefined): string {
  if (!name) return "?";
  return name
    .trim()
    .split(/\s+/)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase() ?? "")
    .join("");
}
