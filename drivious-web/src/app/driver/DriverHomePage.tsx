import { useQuery } from "@tanstack/react-query";
import { Link } from "react-router-dom";
import { AlertTriangle, ArrowRight, Car, IdCard, TriangleAlert, Wallet } from "lucide-react";
import {
  drivers as driversApi,
  incomes as incomesApi,
  vehicleAssignments as assignmentsApi,
  vehicles as vehiclesApi,
} from "@/api/endpoints";
import { assetUrl } from "@/api/client";
import { fuelTypes, vehicleStatuses } from "@/lib/enums";
import { date, daysUntil, km, longDate, money, relativeDays } from "@/lib/format";
import { useAuth } from "@/auth/AuthContext";
import { Badge, Button, Card, CardBody, CardHeader, CardTitle, EmptyState, Skeleton } from "@/ui";

/**
 * The driver's home screen.
 *
 * There is no dashboard endpoint for this role — `/api/dashboards` is Admin and
 * Manager only — so the figures here are built from the driver's own lists,
 * which the API already narrows to them.
 */
export function DriverHomePage() {
  const { user } = useAuth();
  const driverId = user?.driverId ?? null;

  const assignments = useQuery({
    queryKey: ["vehicleassignments", "mine"],
    queryFn: () => assignmentsApi.list({ isOpen: true, pageSize: 5 }),
  });

  const openAssignment = assignments.data?.items[0];

  const vehicle = useQuery({
    queryKey: ["vehicles", openAssignment?.vehicleId],
    queryFn: () => vehiclesApi.get(openAssignment!.vehicleId),
    enabled: Boolean(openAssignment?.vehicleId),
  });

  // First day of the current month, so "this month" means the calendar month.
  const monthStart = new Date();
  monthStart.setDate(1);
  monthStart.setHours(0, 0, 0, 0);

  const monthIncome = useQuery({
    queryKey: ["incomes", "mine", "month", monthStart.toISOString()],
    queryFn: () => incomesApi.list({ from: monthStart.toISOString(), pageSize: 100 }),
  });

  const recent = useQuery({
    queryKey: ["incomes", "mine", "recent"],
    queryFn: () => incomesApi.list({ pageSize: 5, sortBy: "incomeDate", descending: true }),
  });

  const profile = useQuery({
    queryKey: ["drivers", driverId],
    queryFn: () => driversApi.get(driverId!),
    enabled: Boolean(driverId),
  });

  const monthTotal = (monthIncome.data?.items ?? []).reduce((total, row) => total + row.amount, 0);
  const licenceDays = daysUntil(profile.data?.licenseExpireDate);

  // An account in the Driver role that is not linked to a driver record matches
  // nothing on the server, so say why the screen is empty instead of showing zeroes.
  if (user && !driverId) {
    return (
      <Card>
        <EmptyState
          icon={<IdCard />}
          title="Hesabınız sürücü kartına bağlanmayıb"
          description="Gəlirlərinizi və maşın təyinatlarınızı görmək üçün administrator hesabınızı sürücü kartına bağlamalıdır. Filo məlumatlarını isə indi də oxuya bilərsiniz."
          action={
            <Button asChild variant="secondary">
              <Link to="/my/fleet">Filoya bax</Link>
            </Button>
          }
        />
      </Card>
    );
  }

  return (
    <>
      <div>
        <h1 className="text-xl font-semibold tracking-tight">
          Salam, {profile.data?.firstName ?? user?.userName}
        </h1>
        <p className="mt-0.5 text-sm text-muted-foreground">{longDate(new Date())}</p>
      </div>

      {licenceDays !== null && licenceDays <= 30 && (
        <div
          className={
            "flex items-start gap-2.5 rounded-lg border px-3.5 py-3 " +
            (licenceDays < 0
              ? "border-danger/30 bg-danger-muted text-danger"
              : "border-warning/30 bg-warning-muted text-warning")
          }
        >
          <TriangleAlert className="mt-px size-4 shrink-0" />
          <div className="text-sm">
            <p className="font-medium">
              {licenceDays < 0
                ? "Sürücülük vəsiqəniz bitib"
                : "Sürücülük vəsiqəniz bitmək üzrədir"}
            </p>
            <p className="mt-0.5 text-xs opacity-90">
              {date(profile.data!.licenseExpireDate)} · {relativeDays(profile.data!.licenseExpireDate)}
            </p>
          </div>
        </div>
      )}

      {/* Current vehicle — the thing a driver opens the app to check. */}
      <Card className="overflow-hidden">
        <CardHeader>
          <CardTitle>Mənim maşınım</CardTitle>
          {openAssignment && (
            <Badge tone="success" dot>
              Sizdədir
            </Badge>
          )}
        </CardHeader>

        {assignments.isLoading ? (
          <CardBody>
            <Skeleton className="h-36 w-full" />
          </CardBody>
        ) : !openAssignment ? (
          <EmptyState
            icon={<Car />}
            title="Hazırda sizə maşın təyin edilməyib"
            description="Maşın təyin ediləndə burada görünəcək."
          />
        ) : (
          <>
            {vehicle.data?.imageURL && (
              <div className="aspect-[16/9] w-full bg-muted sm:aspect-[21/9]">
                <img
                  src={assetUrl(vehicle.data.imageURL)}
                  alt=""
                  className="size-full object-cover"
                />
              </div>
            )}

            <CardBody className="space-y-3">
              <div className="flex items-start justify-between gap-3">
                <div className="min-w-0">
                  <p className="truncate text-base font-semibold">
                    {openAssignment.vehicleName}
                  </p>
                  <p className="font-mono text-sm text-muted-foreground">
                    {openAssignment.vehiclePlateNumber}
                  </p>
                </div>

                {vehicle.data && (
                  <Badge tone={vehicleStatuses.tone(vehicle.data.status)}>
                    {vehicleStatuses.label(vehicle.data.status)}
                  </Badge>
                )}
              </div>

              <dl className="grid grid-cols-2 gap-3 border-t border-border pt-3 sm:grid-cols-4">
                <Fact label="Yürüş" value={vehicle.data ? km(vehicle.data.mileage) : "—"} />
                <Fact
                  label="Yanacaq"
                  value={vehicle.data ? fuelTypes.label(vehicle.data.fuelType) : "—"}
                />
                <Fact label="Verilib" value={date(openAssignment.assignedDate)} />
                <Fact label="İl" value={vehicle.data ? String(vehicle.data.year) : "—"} />
              </dl>

              {openAssignment.note && (
                <p className="rounded-md bg-muted px-3 py-2 text-xs text-muted-foreground">
                  {openAssignment.note}
                </p>
              )}
            </CardBody>
          </>
        )}
      </Card>

      <div className="grid gap-3 sm:grid-cols-2">
        <Card>
          <CardBody>
            <div className="flex items-start justify-between gap-2">
              <p className="text-xs font-medium text-muted-foreground">Bu ayın qazancı</p>
              <Wallet className="size-4 text-success" />
            </div>

            {monthIncome.isLoading ? (
              <Skeleton className="mt-2 h-8 w-28" />
            ) : (
              <>
                <p className="mt-1.5 text-2xl font-semibold tracking-tight text-success tnum">
                  {money(monthTotal)}
                </p>
                <p className="mt-1 text-xs text-muted-foreground">
                  {monthIncome.data?.totalCount ?? 0} qeyd
                  {(monthIncome.data?.totalCount ?? 0) > 100 && " (ilk 100-ü hesablanıb)"}
                </p>
              </>
            )}
          </CardBody>
        </Card>

        <Card>
          <CardBody>
            <div className="flex items-start justify-between gap-2">
              <p className="text-xs font-medium text-muted-foreground">Açıq təyinat</p>
              <Car className="size-4 text-muted-foreground" />
            </div>

            {assignments.isLoading ? (
              <Skeleton className="mt-2 h-8 w-16" />
            ) : (
              <>
                <p className="mt-1.5 text-2xl font-semibold tracking-tight tnum">
                  {assignments.data?.totalCount ?? 0}
                </p>
                <p className="mt-1 text-xs text-muted-foreground">
                  {openAssignment
                    ? `${relativeDays(openAssignment.assignedDate)} verilib`
                    : "Hazırda maşın yoxdur"}
                </p>
              </>
            )}
          </CardBody>
        </Card>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Son qazanclar</CardTitle>
          <Link
            to="/my/earnings"
            className="inline-flex items-center gap-1 text-xs text-primary hover:underline"
          >
            Hamısı
            <ArrowRight className="size-3" />
          </Link>
        </CardHeader>

        {recent.isLoading ? (
          <CardBody className="space-y-2">
            {Array.from({ length: 3 }, (_, i) => (
              <Skeleton key={i} className="h-12 w-full" />
            ))}
          </CardBody>
        ) : recent.data?.items.length === 0 ? (
          <EmptyState
            icon={<AlertTriangle />}
            title="Hələ qazanc qeydi yoxdur"
            description="Qeydlər menecer tərəfindən əlavə olunduqca burada görünəcək."
          />
        ) : (
          <ul className="divide-y divide-border">
            {recent.data?.items.map((income) => (
              <li key={income.id} className="flex items-center justify-between gap-3 px-4 py-3">
                <div className="min-w-0">
                  <p className="truncate text-sm font-medium">{income.vehiclePlateNumber}</p>
                  <p className="truncate text-xs text-muted-foreground">
                    {date(income.incomeDate)}
                    {income.description ? ` · ${income.description}` : ""}
                  </p>
                </div>

                <span className="shrink-0 text-sm font-semibold text-success tnum">
                  {money(income.amount)}
                </span>
              </li>
            ))}
          </ul>
        )}
      </Card>
    </>
  );
}

function Fact({ label, value }: { label: string; value: string }) {
  return (
    <div className="min-w-0">
      <dt className="text-[11px] text-muted-foreground">{label}</dt>
      <dd className="truncate text-sm font-medium tnum">{value}</dd>
    </div>
  );
}
