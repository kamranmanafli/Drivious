import { useCallback, useEffect, useRef, useState } from "react";
import { errorMessage } from "../api/client";

/**
 * Drives one paged list endpoint. Every list route in the API takes the same
 * query parameters and answers the same PagedResult, so this hook does not know
 * or care which resource it is pointed at.
 *
 * @param fetcher  a `list`/`deleted` function from src/api/endpoints
 * @param filters  resource specific query parameters, e.g. { vehicleId, category }
 */
export function useResourceList(fetcher, filters = {}, pageSize = 20) {
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [sortBy, setSortBy] = useState(null);
  const [descending, setDescending] = useState(false);

  const [result, setResult] = useState({ items: [], totalCount: 0, totalPages: 0 });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  // Compared by value: a filters object rebuilt on every render would restart
  // the effect forever.
  const filterKey = JSON.stringify(filters);

  // Only the newest response may write state, so a slow first request cannot
  // overwrite the result of a faster second one.
  const requestId = useRef(0);

  const load = useCallback(async () => {
    const id = ++requestId.current;

    setLoading(true);
    setError(null);

    try {
      const data = await fetcher({
        page,
        pageSize,
        search: search || undefined,
        sortBy: sortBy || undefined,
        descending,
        ...JSON.parse(filterKey),
      });

      if (id === requestId.current) setResult(data);
    } catch (err) {
      if (id === requestId.current) setError(errorMessage(err));
    } finally {
      if (id === requestId.current) setLoading(false);
    }
  }, [fetcher, page, pageSize, search, sortBy, descending, filterKey]);

  useEffect(() => {
    load();
  }, [load]);

  // Page 4 of the old filter is rarely a page of the new one.
  useEffect(() => {
    setPage(1);
  }, [search, filterKey]);

  const toggleSort = useCallback(
    (field) => {
      if (sortBy === field) {
        setDescending((value) => !value);
      } else {
        setSortBy(field);
        setDescending(false);
      }
    },
    [sortBy]
  );

  return {
    ...result,
    loading,
    error,
    page,
    setPage,
    search,
    setSearch,
    sortBy,
    descending,
    toggleSort,
    /** Call after a create, update or toggle to pull the page again. */
    reload: load,
  };
}
