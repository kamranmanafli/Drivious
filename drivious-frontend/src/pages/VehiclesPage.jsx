import { useCallback, useState } from "react";
import { vehicles } from "../api/endpoints";
import { errorMessage } from "../api/client";
import { DataTable } from "../components/DataTable";
import { useResourceList } from "../components/useResourceList";
import { FUEL_TYPE, VEHICLE_STATUS, options } from "../constants/enums";
import { useAuth } from "../auth/AuthContext";

/**
 * The worked example. Every other list page is this file with a different
 * endpoint, different columns and different filters - the table, the paging and
 * the search all come from the two shared pieces.
 */
export default function VehiclesPage() {
  const { canManage } = useAuth();

  const [status, setStatus] = useState("");

  // Memoised so the hook does not see a new function on every render.
  const fetcher = useCallback((params) => vehicles.list(params), []);

  const list = useResourceList(fetcher, {
    status: status === "" ? undefined : Number(status),
  });

  async function archive(row) {
    if (!confirm(`"${row.plateNumber}" arxivə atılsın?`)) return;

    try {
      await vehicles.toggle(row.id);
      list.reload();
    } catch (err) {
      alert(errorMessage(err));
    }
  }

  return (
    <>
      <h2>Maşınlar</h2>

      <div style={{ marginBottom: 16 }}>
        <select value={status} onChange={(event) => setStatus(event.target.value)}>
          <option value="">Bütün statuslar</option>
          {options(VEHICLE_STATUS).map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
      </div>

      <DataTable
        list={list}
        columns={[
          { key: "plateNumber", title: "Nömrə", sortable: true },
          {
            key: "brand",
            title: "Maşın",
            sortable: true,
            render: (row) => `${row.brand} ${row.model}`,
          },
          { key: "year", title: "İl", sortable: true },
          {
            key: "fuelType",
            title: "Yanacaq",
            render: (row) => FUEL_TYPE[row.fuelType],
          },
          {
            key: "mileage",
            title: "Yürüş",
            sortable: true,
            render: (row) => `${row.mileage.toLocaleString("az")} km`,
          },
          {
            key: "status",
            title: "Status",
            sortable: true,
            render: (row) => VEHICLE_STATUS[row.status],
          },
        ]}
        actions={
          canManage
            ? (row) => <button onClick={() => archive(row)}>Arxivə at</button>
            : undefined
        }
      />
    </>
  );
}
