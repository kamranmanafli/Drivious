import { useQuery } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import {
  Area,
  AreaChart,
  Bar,
  BarChart,
  CartesianGrid,
  Cell,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from "recharts";
import {
  AlertTriangle,
  Banknote,
  Car,
  ClipboardList,
  FileText,
  Fuel,
  Receipt,
  ShieldCheck,
  TrendingUp,
  Users,
  Wrench,
} from "lucide-react";
import { dashboard as dashboardApi } from "@/api/endpoints";
import { errorMessage } from "@/api/client";
import { money, moneyShort, monthLabel, number } from "@/lib/format";
import { PageHeader } from "@/components/PageHeader";
import { StatCard } from "@/components/StatCard";
import { Badge, Card, CardBody, CardHeader, CardTitle, ErrorState, Skeleton } from "@/ui";
import type { Upcoming } from "@/api/types";

export function DashboardPage() {
  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ["dashboard"],
    queryFn: dashboardApi.get,
  });

  if (error) {
    return (
      <Card>
        <ErrorState message={errorMessage(error)} onRetry={() => void refetch()} />
      </Card>
    );
  }

  const profitPositive = (data?.profit ?? 0) >= 0;

  return (
    <>
      <PageHeader
        title="İdarə paneli"
        description="Filonun ümumi vəziyyəti, aylıq dinamika və yaxınlaşan bitmə tarixləri."
      />

      {/* Money first — it is the question the page is opened to answer. */}
      <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
        <StatCard
          label="Ümumi gəlir"
          value={money(data?.totalIncome)}
          icon={<Banknote />}
          tone="success"
          loading={isLoading}
          to="/incomes"
        />
        <StatCard
          label="Ümumi xərc"
          value={money(data?.totalExpense)}
          icon={<Receipt />}
          loading={isLoading}
          to="/expenses"
        />
        <StatCard
          label="Mənfəət"
          value={money(data?.profit)}
          hint={profitPositive ? "Gəlir xərcdən çoxdur" : "Xərc gəliri üstələyir"}
          icon={<TrendingUp />}
          tone={profitPositive ? "success" : "danger"}
          loading={isLoading}
        />
        <StatCard
          label="Yanacaq + servis"
          value={money((data?.totalFuelCost ?? 0) + (data?.totalMaintenanceCost ?? 0))}
          hint={
            data
              ? `Yanacaq ${money(data.totalFuelCost)} · Servis ${money(data.totalMaintenanceCost)}`
              : undefined
          }
          icon={<Fuel />}
          loading={isLoading}
        />
      </div>

      <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
        <StatCard
          label="Maşınlar"
          value={number(data?.totalVehicles)}
          hint={data ? `${data.activeVehicles} aktiv` : undefined}
          icon={<Car />}
          loading={isLoading}
          to="/vehicles"
        />
        <StatCard
          label="Sürücülər"
          value={number(data?.totalDrivers)}
          hint={data ? `${data.activeDrivers} aktiv` : undefined}
          icon={<Users />}
          loading={isLoading}
          to="/drivers"
        />
        <StatCard
          label="Təyin olunmuş maşın"
          value={number(data?.assignedVehicles)}
          hint={
            data ? `${data.totalVehicles - data.assignedVehicles} maşın boşdadır` : undefined
          }
          icon={<ClipboardList />}
          loading={isLoading}
          to="/assignments?isOpen=true"
        />
        <StatCard
          label="Oxunmamış bildiriş"
          value={number(data?.unreadNotifications)}
          hint={data ? `${data.totalNotifications} bildirişdən` : undefined}
          icon={<AlertTriangle />}
          tone={data?.unreadNotifications ? "warning" : "neutral"}
          loading={isLoading}
          to="/notifications?isRead=false"
        />
      </div>

      <div className="grid gap-4 lg:grid-cols-3">
        <Card className="lg:col-span-2">
          <CardHeader>
            <div>
              <CardTitle>Gəlir və xərc</CardTitle>
              <p className="mt-0.5 text-xs text-muted-foreground">Son 6 ay</p>
            </div>

            <div className="flex items-center gap-3 text-xs text-muted-foreground">
              <span className="flex items-center gap-1.5">
                <span className="size-2 rounded-full bg-success" />
                Gəlir
              </span>
              <span className="flex items-center gap-1.5">
                <span className="size-2 rounded-full bg-danger" />
                Xərc
              </span>
            </div>
          </CardHeader>

          <CardBody>
            {isLoading ? (
              <Skeleton className="h-64 w-full" />
            ) : (
              <ResponsiveContainer width="100%" height={260}>
                <AreaChart
                  data={data?.monthlyTotals ?? []}
                  margin={{ top: 8, right: 8, bottom: 0, left: -12 }}
                >
                  <defs>
                    <linearGradient id="income" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="0%" stopColor="var(--success)" stopOpacity={0.28} />
                      <stop offset="100%" stopColor="var(--success)" stopOpacity={0} />
                    </linearGradient>
                    <linearGradient id="expense" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="0%" stopColor="var(--danger)" stopOpacity={0.22} />
                      <stop offset="100%" stopColor="var(--danger)" stopOpacity={0} />
                    </linearGradient>
                  </defs>

                  <CartesianGrid stroke="var(--border)" vertical={false} />

                  <XAxis
                    dataKey="label"
                    tickFormatter={monthLabel}
                    tickLine={false}
                    axisLine={false}
                    tick={{ fill: "var(--muted-foreground)", fontSize: 11 }}
                  />
                  <YAxis
                    tickFormatter={moneyShort}
                    tickLine={false}
                    axisLine={false}
                    width={60}
                    tick={{ fill: "var(--muted-foreground)", fontSize: 11 }}
                  />

                  <Tooltip
                    cursor={{ stroke: "var(--border-strong)" }}
                    contentStyle={{
                      background: "var(--surface)",
                      border: "1px solid var(--border)",
                      borderRadius: 8,
                      fontSize: 12,
                    }}
                    labelStyle={{ color: "var(--muted-foreground)" }}
                    labelFormatter={(label: string) => `${monthLabel(label)} ${label.split("-")[0]}`}
                    formatter={(value: number, key: string) => [
                      money(value),
                      key === "income" ? "Gəlir" : "Xərc",
                    ]}
                  />

                  <Area
                    type="monotone"
                    dataKey="income"
                    stroke="var(--success)"
                    strokeWidth={2}
                    fill="url(#income)"
                  />
                  <Area
                    type="monotone"
                    dataKey="expense"
                    stroke="var(--danger)"
                    strokeWidth={2}
                    fill="url(#expense)"
                  />
                </AreaChart>
              </ResponsiveContainer>
            )}
          </CardBody>
        </Card>

        <UpcomingCard upcoming={data?.upcoming} loading={isLoading} />
      </div>

      <Card>
        <CardHeader>
          <div>
            <CardTitle>Ən çox xərc çəkən maşınlar</CardTitle>
            <p className="mt-0.5 text-xs text-muted-foreground">
              Xərc, yanacaq və servis birlikdə
            </p>
          </div>
        </CardHeader>

        <CardBody>
          {isLoading ? (
            <Skeleton className="h-56 w-full" />
          ) : data?.topSpendingVehicles.length ? (
            <ResponsiveContainer width="100%" height={Math.max(160, data.topSpendingVehicles.length * 44)}>
              <BarChart
                data={data.topSpendingVehicles}
                layout="vertical"
                margin={{ top: 0, right: 16, bottom: 0, left: 8 }}
              >
                <CartesianGrid stroke="var(--border)" horizontal={false} />
                <XAxis
                  type="number"
                  tickFormatter={moneyShort}
                  tickLine={false}
                  axisLine={false}
                  tick={{ fill: "var(--muted-foreground)", fontSize: 11 }}
                />
                <YAxis
                  type="category"
                  dataKey="plateNumber"
                  width={92}
                  tickLine={false}
                  axisLine={false}
                  tick={{ fill: "var(--muted-foreground)", fontSize: 11 }}
                />
                <Tooltip
                  cursor={{ fill: "var(--muted)" }}
                  contentStyle={{
                    background: "var(--surface)",
                    border: "1px solid var(--border)",
                    borderRadius: 8,
                    fontSize: 12,
                  }}
                  formatter={(value: number, _key, item) => [
                    money(value),
                    (item?.payload as { vehicleName?: string })?.vehicleName ?? "Ümumi xərc",
                  ]}
                />
                <Bar dataKey="totalCost" radius={[0, 4, 4, 0]} barSize={18}>
                  {data.topSpendingVehicles.map((entry, index) => (
                    <Cell
                      key={entry.vehicleId}
                      // The leader is the point of the chart; the rest recede.
                      fill={index === 0 ? "var(--primary)" : "var(--border-strong)"}
                    />
                  ))}
                </Bar>
              </BarChart>
            </ResponsiveContainer>
          ) : (
            <p className="py-8 text-center text-sm text-muted-foreground">
              Hələ xərc qeydiyyatı yoxdur.
            </p>
          )}
        </CardBody>
      </Card>
    </>
  );
}

const UPCOMING_ROWS: Array<{
  key: keyof Omit<Upcoming, "withinDays" | "overdueTotal">;
  label: string;
  icon: typeof ShieldCheck;
  to: string;
}> = [
  { key: "expiringInsurances", label: "Sığorta", icon: ShieldCheck, to: "/insurance" },
  { key: "expiringLicenses", label: "Sürücülük vəsiqəsi", icon: Users, to: "/drivers" },
  { key: "dueMaintenances", label: "Planlı servis", icon: Wrench, to: "/maintenance" },
  { key: "expiringDocuments", label: "Sənəd", icon: FileText, to: "/documents" },
];

function UpcomingCard({ upcoming, loading }: { upcoming?: Upcoming; loading: boolean }) {
  return (
    <Card>
      <CardHeader>
        <div>
          <CardTitle>Bitmək üzrədir</CardTitle>
          <p className="mt-0.5 text-xs text-muted-foreground">
            {upcoming ? `Növbəti ${upcoming.withinDays} gün` : "Yaxın tarixlər"}
          </p>
        </div>
      </CardHeader>

      <CardBody className="p-0">
        {loading ? (
          <div className="space-y-3 p-4">
            {Array.from({ length: 4 }, (_, i) => (
              <Skeleton key={i} className="h-9 w-full" />
            ))}
          </div>
        ) : (
          <>
            <ul className="divide-y divide-border">
              {UPCOMING_ROWS.map((row) => {
                const count = upcoming?.[row.key] ?? 0;

                return (
                  <li key={row.key}>
                    <Link
                      to={row.to}
                      className="flex items-center justify-between gap-3 px-4 py-2.5 hover:bg-surface-raised"
                    >
                      <span className="flex items-center gap-2.5 text-sm">
                        <row.icon className="size-4 text-muted-foreground" />
                        {row.label}
                      </span>

                      <Badge tone={count > 0 ? "warning" : "neutral"}>{count}</Badge>
                    </Link>
                  </li>
                );
              })}
            </ul>

            {Boolean(upcoming?.overdueTotal) && (
              <Link
                to="/notifications?type=3"
                className="flex items-center justify-between gap-3 border-t border-border bg-danger-muted px-4 py-3 text-danger"
              >
                <span className="flex items-center gap-2 text-sm font-medium">
                  <AlertTriangle className="size-4" />
                  Vaxtı keçmiş
                </span>
                <span className="text-sm font-semibold tnum">{upcoming!.overdueTotal}</span>
              </Link>
            )}
          </>
        )}
      </CardBody>
    </Card>
  );
}
