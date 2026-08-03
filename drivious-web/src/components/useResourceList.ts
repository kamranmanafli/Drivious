import { useCallback, useMemo } from "react";
import { useSearchParams } from "react-router-dom";
import { keepPreviousData, useQuery } from "@tanstack/react-query";
import type { PagedResult, QueryParameters } from "@/api/types";

/** Values the table controls itself; anything else is a resource filter. */
const RESERVED = new Set(["page", "pageSize", "search", "sortBy", "descending"]);

interface Options<T> {
  /** Cache key root — usually the resource path. */
  key: string;
  fetcher: (params: Record<string, string | number | boolean>) => Promise<PagedResult<T>>;
  defaultSort: string;
  defaultDescending?: boolean;
  pageSize?: number;
  /** Filters this list understands. Anything else in the URL is ignored. */
  filters?: readonly string[];
  /** Fixed values merged into every request, e.g. a vehicleId on a detail page. */
  fixed?: Record<string, string | number | boolean | undefined>;
  enabled?: boolean;
}

/**
 * Backs every list screen: reads paging, search, sort and filters from the URL,
 * writes them back, and fetches the matching page.
 *
 * Keeping the state in the URL rather than in component state means a filtered
 * view survives a refresh and can be pasted to a colleague — which is what a
 * fleet manager actually does with "every insurance expiring this month".
 */
export function useResourceList<T>({
  key,
  fetcher,
  defaultSort,
  defaultDescending = true,
  pageSize = 20,
  filters = [],
  fixed,
  enabled = true,
}: Options<T>) {
  const [searchParams, setSearchParams] = useSearchParams();

  const params = useMemo(() => {
    const query: QueryParameters & Record<string, string | number | boolean> = {
      page: Number(searchParams.get("page")) || 1,
      pageSize,
      sortBy: searchParams.get("sortBy") ?? defaultSort,
      descending: searchParams.get("descending")
        ? searchParams.get("descending") === "true"
        : defaultDescending,
    };

    const search = searchParams.get("search");
    if (search) query.search = search;

    for (const name of filters) {
      const value = searchParams.get(name);
      if (value !== null && value !== "") query[name] = value;
    }

    for (const [name, value] of Object.entries(fixed ?? {})) {
      if (value !== undefined && value !== null && value !== "") query[name] = value;
    }

    return query;
    // `fixed` is an object literal at most call sites; comparing its contents
    // keeps the query key stable across renders.
  }, [searchParams, pageSize, defaultSort, defaultDescending, filters, JSON.stringify(fixed)]);

  const query = useQuery({
    queryKey: [key, params],
    queryFn: () => fetcher(params as Record<string, string | number | boolean>),
    // Holds the previous page on screen while the next one loads, so the table
    // does not collapse to a spinner on every page change.
    placeholderData: keepPreviousData,
    enabled,
  });

  /** Writes one value into the URL. Changing anything but the page resets it to 1. */
  const setParam = useCallback(
    (name: string, value: string | number | boolean | null | undefined) => {
      setSearchParams(
        (previous) => {
          const next = new URLSearchParams(previous);

          if (value === null || value === undefined || value === "") next.delete(name);
          else next.set(name, String(value));

          if (name !== "page") next.delete("page");

          return next;
        },
        { replace: true },
      );
    },
    [setSearchParams],
  );

  const setSort = useCallback(
    (field: string) => {
      const current = searchParams.get("sortBy") ?? defaultSort;
      const descending = searchParams.get("descending")
        ? searchParams.get("descending") === "true"
        : defaultDescending;

      setSearchParams(
        (previous) => {
          const next = new URLSearchParams(previous);
          next.set("sortBy", field);
          // Same column toggles direction; a new column starts descending,
          // which is what people expect from dates and amounts.
          next.set("descending", String(field === current ? !descending : true));
          next.delete("page");
          return next;
        },
        { replace: true },
      );
    },
    [searchParams, setSearchParams, defaultSort, defaultDescending],
  );

  const clearFilters = useCallback(() => {
    setSearchParams(
      (previous) => {
        const next = new URLSearchParams(previous);
        for (const name of [...filters, "search"]) next.delete(name);
        next.delete("page");
        return next;
      },
      { replace: true },
    );
  }, [setSearchParams, filters]);

  const activeFilterCount = useMemo(
    () =>
      [...filters, "search"].filter((name) => {
        const value = searchParams.get(name);
        return value !== null && value !== "";
      }).length,
    [searchParams, filters],
  );

  return {
    ...query,
    result: query.data,
    items: query.data?.items ?? [],
    params,
    /** Reads a filter's current value for a controlled input. */
    value: (name: string) => searchParams.get(name) ?? "",
    setParam,
    setPage: (page: number) => setParam("page", page),
    setSort,
    sortBy: (searchParams.get("sortBy") ?? defaultSort) as string,
    descending: searchParams.get("descending")
      ? searchParams.get("descending") === "true"
      : defaultDescending,
    clearFilters,
    activeFilterCount,
    hasFilters: activeFilterCount > 0,
  };
}

export { RESERVED as RESERVED_QUERY_KEYS };
