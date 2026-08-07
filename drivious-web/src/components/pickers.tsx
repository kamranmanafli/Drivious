import { useQuery } from "@tanstack/react-query";
import { drivers as driversApi, vehicles as vehiclesApi } from "@/api/endpoints";
import { NativeSelect } from "@/ui";

/**
 * Reference pickers.
 *
 * Both lists are capped at the API's maximum page size. A fleet larger than 100
 * vehicles would need a searchable picker backed by the list endpoint's own
 * `search` — noted here rather than pretended away.
 */
const PAGE_SIZE = 100;

export function useVehicleOptions() {
  return useQuery({
    queryKey: ["vehicles", "options"],
    queryFn: () => vehiclesApi.list({ pageSize: PAGE_SIZE, sortBy: "plateNumber", descending: false }),
    staleTime: 5 * 60_000,
    select: (page) =>
      page.items.map((v) => ({
        value: v.id,
        label: `${v.plateNumber} — ${v.brand} ${v.model}`,
      })),
  });
}

export function useDriverOptions() {
  return useQuery({
    queryKey: ["drivers", "options"],
    queryFn: () => driversApi.list({ pageSize: PAGE_SIZE, sortBy: "firstName", descending: false }),
    staleTime: 5 * 60_000,
    select: (page) =>
      page.items.map((d) => ({
        value: d.id,
        label: d.fullName ?? `${d.firstName} ${d.lastName}`,
      })),
  });
}

interface PickerProps extends React.SelectHTMLAttributes<HTMLSelectElement> {
  /** Label for the empty option — "Bütün maşınlar" in a filter, "Seçin" in a form. */
  placeholder?: string;
}

export function VehiclePicker({ placeholder = "Maşın seçin", ...props }: PickerProps) {
  const { data, isLoading } = useVehicleOptions();

  return (
    <NativeSelect disabled={isLoading} {...props}>
      <option value="">{isLoading ? "Yüklənir…" : placeholder}</option>
      {data?.map((option) => (
        <option key={option.value} value={option.value}>
          {option.label}
        </option>
      ))}
    </NativeSelect>
  );
}

export function DriverPicker({ placeholder = "Sürücü seçin", ...props }: PickerProps) {
  const { data, isLoading } = useDriverOptions();

  return (
    <NativeSelect disabled={isLoading} {...props}>
      <option value="">{isLoading ? "Yüklənir…" : placeholder}</option>
      {data?.map((option) => (
        <option key={option.value} value={option.value}>
          {option.label}
        </option>
      ))}
    </NativeSelect>
  );
}

/** Renders an enum's options from the shared label tables. */
export function EnumOptions({
  entries,
  placeholder,
}: {
  entries: Array<{ value: number; label: string }>;
  placeholder?: string;
}) {
  return (
    <>
      {placeholder !== undefined && <option value="">{placeholder}</option>}
      {entries.map((entry) => (
        <option key={entry.value} value={entry.value}>
          {entry.label}
        </option>
      ))}
    </>
  );
}
