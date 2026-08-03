import { useQuery } from "@tanstack/react-query";
import { Wallet } from "lucide-react";
import { incomes as incomesApi, vehicleAssignments as assignmentsApi } from "@/api/endpoints";
import type { Income, VehicleAssignment } from "@/api/types";
import { cn } from "@/lib/cn";
import { date, money, relativeDays } from "@/lib/format";
import { useResourceList } from "@/components/useResourceList";
import { Pagination } from "@/components/Pagination";
import { errorMessage } from "@/api/client";
import {
  Badge,
  Card,
  CardBody,
  CardHeader,
  CardTitle,
  EmptyState,
  ErrorState,
  Input,
  Skeleton,
} from "@/ui";

/** The driver's own earnings. The API scopes the list; no driver filter is offered. */
export function DriverEarningsPage() {
  const list = useResourceList<Income>({
    key: "incomes",
    fetcher: incomesApi.list,
    defaultSort: "incomeDate",
    pageSize: 25,
    filters: ["from", "to"],
  });

  const monthStart = new Date();
  monthStart.setDate(1);
  monthStart.setHours(0, 0, 0, 0);

  const month = useQuery({
    queryKey: ["incomes", "mine", "month-total", monthStart.toISOString()],
    queryFn: () => incomesApi.list({ from: monthStart.toISOString(), pageSize: 100 }),
    select: (page) => ({
      total: page.items.reduce((sum, row) => sum + row.amount, 0),
      count: page.totalCount,
    }),
  });

  const pageTotal = list.items.reduce((sum, row) => sum + row.amount, 0);

  return (
    <>
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Qazancım</h1>
        <p className="mt-0.5 text-sm text-muted-foreground">
          Sizin gətirdiyiniz gəlirlərin tam siyahısı.
        </p>
      </div>

      <Card>
        <CardBody className="flex flex-wrap items-end justify-between gap-4">
          <div>
            <p className="text-xs font-medium text-muted-foreground">Bu ay</p>
            {month.isLoading ? (
              <Skeleton className="mt-1.5 h-8 w-32" />
            ) : (
              <p className="mt-1 text-2xl font-semibold tracking-tight text-success tnum">
                {money(month.data?.total)}
              </p>
            )}
          </div>

          <Wallet className="size-5 text-success" />
        </CardBody>
      </Card>

      <div className="grid grid-cols-2 gap-2">
        <label className="flex flex-col gap-1.5">
          <span className="text-xs font-medium text-muted-foreground">Tarixdən</span>
          <Input
            type="date"
            value={list.value("from")}
            onChange={(event) => list.setParam("from", event.target.value)}
          />
        </label>

        <label className="flex flex-col gap-1.5">
          <span className="text-xs font-medium text-muted-foreground">Tarixə</span>
          <Input
            type="date"
            value={list.value("to")}
            onChange={(event) => list.setParam("to", event.target.value)}
          />
        </label>
      </div>

      {list.error ? (
        <Card>
          <ErrorState message={errorMessage(list.error)} onRetry={() => void list.refetch()} />
        </Card>
      ) : list.isLoading ? (
        <div className="space-y-2">
          {Array.from({ length: 6 }, (_, i) => (
            <Skeleton key={i} className="h-16 w-full" />
          ))}
        </div>
      ) : list.items.length === 0 ? (
        <Card>
          <EmptyState
            icon={<Wallet />}
            title={list.hasFilters ? "Bu aralıqda qeyd yoxdur" : "Hələ qazanc qeydi yoxdur"}
            description={
              list.hasFilters
                ? "Tarix aralığını dəyişib yenidən cəhd edin."
                : "Qeydlər menecer tərəfindən əlavə olunduqca burada görünəcək."
            }
          />
        </Card>
      ) : (
        <div className={cn("space-y-3", list.isFetching && "opacity-60")}>
          <Card>
            <CardHeader>
              <CardTitle>
                {list.result?.totalCount} qeyd
              </CardTitle>
              <span className="text-sm font-semibold text-success tnum">
                Bu səhifə: {money(pageTotal)}
              </span>
            </CardHeader>

            <ul className="divide-y divide-border">
              {list.items.map((income) => (
                <li key={income.id} className="flex items-center justify-between gap-3 px-4 py-3">
                  <div className="min-w-0">
                    <p className="truncate text-sm font-medium">{income.vehiclePlateNumber}</p>
                    <p className="truncate text-xs text-muted-foreground">
                      {income.vehicleName}
                      {income.description ? ` · ${income.description}` : ""}
                    </p>
                    <p className="mt-0.5 text-xs text-muted-foreground tnum">
                      {date(income.incomeDate)}
                    </p>
                  </div>

                  <span className="shrink-0 text-sm font-semibold text-success tnum">
                    {money(income.amount)}
                  </span>
                </li>
              ))}
            </ul>
          </Card>

          {list.result && <Pagination result={list.result} onPage={list.setPage} />}
        </div>
      )}

      <MyAssignments />
    </>
  );
}

function MyAssignments() {
  const { data, isLoading } = useQuery({
    queryKey: ["vehicleassignments", "mine", "all"],
    queryFn: () =>
      assignmentsApi.list({ pageSize: 20, sortBy: "assignedDate", descending: true }),
  });

  return (
    <Card>
      <CardHeader>
        <CardTitle>Təyinat tarixçəm</CardTitle>
      </CardHeader>

      {isLoading ? (
        <CardBody className="space-y-2">
          {Array.from({ length: 3 }, (_, i) => (
            <Skeleton key={i} className="h-12 w-full" />
          ))}
        </CardBody>
      ) : !data || data.items.length === 0 ? (
        <EmptyState title="Hələ maşın təyinatı olmayıb" />
      ) : (
        <ul className="divide-y divide-border">
          {data.items.map((assignment: VehicleAssignment) => (
            <li key={assignment.id} className="flex items-center justify-between gap-3 px-4 py-3">
              <div className="min-w-0">
                <p className="truncate text-sm font-medium">{assignment.vehiclePlateNumber}</p>
                <p className="truncate text-xs text-muted-foreground">
                  {assignment.vehicleName}
                </p>
                <p className="mt-0.5 text-xs text-muted-foreground tnum">
                  {date(assignment.assignedDate)}
                  {assignment.returnedDate ? ` — ${date(assignment.returnedDate)}` : ""}
                </p>
              </div>

              {assignment.returnedDate ? (
                <Badge tone="neutral">Qaytarılıb</Badge>
              ) : (
                <Badge tone="success" dot>
                  {relativeDays(assignment.assignedDate)}
                </Badge>
              )}
            </li>
          ))}
        </ul>
      )}
    </Card>
  );
}
