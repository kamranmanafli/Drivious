import "./DataTable.css";

/**
 * One table for every list endpoint in the API.
 *
 * Pass the object returned by useResourceList as `list`, and describe the
 * columns:
 *
 *   const list = useResourceList(vehicles.list);
 *
 *   <DataTable
 *     list={list}
 *     columns={[
 *       { key: "plateNumber", title: "Nömrə", sortable: true },
 *       { key: "brand",       title: "Marka", render: (row) => `${row.brand} ${row.model}` },
 *       { key: "status",      title: "Status", render: (row) => VEHICLE_STATUS[row.status] },
 *     ]}
 *     actions={(row) => <button onClick={() => edit(row)}>Redaktə</button>}
 *   />
 *
 * `key` doubles as the sort field sent to the API. Only fields the endpoint
 * declares as sortable have any effect - anything else quietly falls back to
 * the endpoint's default order, so a wrong key shows no error, just no change.
 */
export function DataTable({ list, columns, actions, emptyText = "Məlumat yoxdur." }) {
  const { items, loading, error, page, totalPages, totalCount, setPage, search, setSearch, sortBy, descending, toggleSort } = list;

  return (
    <div className="dt">
      <div className="dt-toolbar">
        <input
          className="dt-search"
          type="search"
          value={search}
          placeholder="Axtar…"
          onChange={(event) => setSearch(event.target.value)}
        />
        <span className="dt-count">{totalCount} nəticə</span>
      </div>

      {error && <p className="dt-error">{error}</p>}

      <div className="dt-scroll">
        <table>
          <thead>
            <tr>
              {columns.map((column) => (
                <th
                  key={column.key}
                  className={column.sortable ? "dt-sortable" : undefined}
                  onClick={column.sortable ? () => toggleSort(column.key) : undefined}
                >
                  {column.title}
                  {sortBy === column.key && (descending ? " ↓" : " ↑")}
                </th>
              ))}
              {actions && <th />}
            </tr>
          </thead>

          <tbody>
            {loading && (
              <tr>
                <td colSpan={columns.length + (actions ? 1 : 0)}>Yüklənir…</td>
              </tr>
            )}

            {!loading && items.length === 0 && (
              <tr>
                <td colSpan={columns.length + (actions ? 1 : 0)}>{emptyText}</td>
              </tr>
            )}

            {!loading &&
              items.map((row) => (
                <tr key={row.id}>
                  {columns.map((column) => (
                    <td key={column.key}>
                      {column.render ? column.render(row) : row[column.key]}
                    </td>
                  ))}
                  {actions && <td className="dt-actions">{actions(row)}</td>}
                </tr>
              ))}
          </tbody>
        </table>
      </div>

      <div className="dt-pager">
        <button disabled={page <= 1} onClick={() => setPage(page - 1)}>
          ← Əvvəlki
        </button>

        <span>
          {page} / {totalPages || 1}
        </span>

        <button disabled={page >= totalPages} onClick={() => setPage(page + 1)}>
          Növbəti →
        </button>
      </div>
    </div>
  );
}
