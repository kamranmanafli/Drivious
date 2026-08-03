import { useState } from "react";
import { useParams } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { Download, Fuel, Pencil, Receipt, TrendingUp, Wrench } from "lucide-react";
import {
  expenses as expensesApi,
  fuelLogs as fuelApi,
  incomes as incomesApi,
  insurances as insuranceApi,
  maintenances as maintenanceApi,
  vehicleAssignments as assignmentsApi,
  vehicleDocuments as documentsApi,
  vehicles as vehiclesApi,
} from "@/api/endpoints";
import { assetUrl, errorMessage } from "@/api/client";
import type { Vehicle } from "@/api/types";
import {
  documentTypes,
  expenseCategories,
  fuelTypes,
  maintenanceTypes,
  vehicleStatuses,
} from "@/lib/enums";
import { date, km, liters, money, number } from "@/lib/format";
import { useAuth } from "@/auth/AuthContext";
import { PageHeader } from "@/components/PageHeader";
import { StatCard } from "@/components/StatCard";
import { VehicleFormDialog } from "./VehiclesPage";
import { ExpiryBadge } from "./InsurancePage";
import { DueBadge } from "./MaintenancePage";
import {
  Badge,
  Button,
  Card,
  CardBody,
  CardHeader,
  CardTitle,
  EmptyState,
  ErrorState,
  Skeleton,
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
} from "@/ui";

/** Pulls the recent rows of one child resource for this vehicle. */
function useVehicleChildren<T>(
  key: string,
  fetcher: (params: Record<string, unknown>) => Promise<{ items: T[]; totalCount: number }>,
  vehicleId: string,
  sortBy: string,
) {
  return useQuery({
    queryKey: [key, "by-vehicle", vehicleId, sortBy],
    queryFn: () => fetcher({ vehicleId, pageSize: 50, sortBy, descending: true }),
    enabled: Boolean(vehicleId),
  });
}

export function VehicleDetailPage() {
  const { id = "" } = useParams();
  const { canManage } = useAuth();
  const [editing, setEditing] = useState(false);

  const vehicle = useQuery({
    queryKey: ["vehicles", id],
    queryFn: () => vehiclesApi.get(id),
    enabled: Boolean(id),
  });

  const expenses = useVehicleChildren("expenses", expensesApi.list, id, "expenseDate");
  const incomes = useVehicleChildren("incomes", incomesApi.list, id, "incomeDate");
  const fuel = useVehicleChildren("fuellogs", fuelApi.list, id, "fuelDate");
  const maintenance = useVehicleChildren("maintenances", maintenanceApi.list, id, "maintenanceDate");
  const insurance = useVehicleChildren("insurances", insuranceApi.list, id, "endDate");
  const documents = useVehicleChildren("vehicledocuments", documentsApi.list, id, "uploadDate");
  const assignments = useVehicleChildren(
    "vehicleassignments",
    assignmentsApi.list,
    id,
    "assignedDate",
  );

  if (vehicle.error) {
    return (
      <Card>
        <ErrorState message={errorMessage(vehicle.error)} onRetry={() => void vehicle.refetch()} />
      </Card>
    );
  }

  if (vehicle.isLoading || !vehicle.data) {
    return (
      <div className="space-y-5">
        <Skeleton className="h-9 w-64" />
        <Skeleton className="h-40 w-full" />
        <Skeleton className="h-64 w-full" />
      </div>
    );
  }

  const car: Vehicle = vehicle.data;

  const sum = <T,>(rows: T[] | undefined, get: (row: T) => number) =>
    (rows ?? []).reduce((total, row) => total + get(row), 0);

  const totalExpense = sum(expenses.data?.items, (r) => r.amount);
  const totalIncome = sum(incomes.data?.items, (r) => r.amount);
  const totalFuel = sum(fuel.data?.items, (r) => r.price);
  const totalService = sum(maintenance.data?.items, (r) => r.cost);

  const openAssignment = assignments.data?.items.find((a) => !a.returnedDate && a.isActive);

  return (
    <>
      <PageHeader
        back={{ to: "/vehicles", label: "Maşınlar" }}
        title={`${car.brand} ${car.model}`}
        description={`${car.plateNumber} · ${car.year} · ${car.color}`}
        actions={
          canManage && (
            <Button variant="secondary" onClick={() => setEditing(true)}>
              <Pencil />
              Redaktə et
            </Button>
          )
        }
      />

      <div className="grid gap-4 lg:grid-cols-[300px_1fr]">
        <Card className="overflow-hidden">
          <div className="aspect-[4/3] w-full bg-muted">
            {car.imageURL ? (
              <img
                src={assetUrl(car.imageURL)}
                alt={`${car.brand} ${car.model}`}
                className="size-full object-cover"
              />
            ) : (
              <div className="flex size-full items-center justify-center text-sm text-muted-foreground">
                Şəkil yoxdur
              </div>
            )}
          </div>

          <CardBody className="space-y-2.5">
            <Row label="Status">
              <Badge tone={vehicleStatuses.tone(car.status)} dot>
                {vehicleStatuses.label(car.status)}
              </Badge>
            </Row>
            <Row label="Dövlət nişanı">
              <span className="font-mono text-xs">{car.plateNumber}</span>
            </Row>
            <Row label="VIN">
              <span className="font-mono text-xs">{car.vin}</span>
            </Row>
            <Row label="Yanacaq">{fuelTypes.label(car.fuelType)}</Row>
            <Row label="Yürüş">
              <span className="tnum">{km(car.mileage)}</span>
            </Row>
            <Row label="Rəng">{car.color}</Row>
            <Row label="Buraxılış ili">
              <span className="tnum">{car.year}</span>
            </Row>
            <Row label="Sistemə əlavə">
              <span className="tnum">{date(car.createdAt)}</span>
            </Row>

            {openAssignment && (
              <div className="mt-3 rounded-md bg-primary-muted px-3 py-2">
                <p className="text-xs text-muted-foreground">Hazırda sürücüdə</p>
                <p className="mt-0.5 text-sm font-medium text-primary">
                  {openAssignment.driverFullName}
                </p>
                <p className="text-xs text-muted-foreground">
                  {date(openAssignment.assignedDate)}-dən
                </p>
              </div>
            )}
          </CardBody>
        </Card>

        <div className="space-y-4">
          <div className="grid gap-3 sm:grid-cols-2 xl:grid-cols-4">
            <StatCard
              label="Gəlir"
              value={money(totalIncome)}
              icon={<TrendingUp />}
              tone="success"
              loading={incomes.isLoading}
            />
            <StatCard
              label="Xərc"
              value={money(totalExpense)}
              icon={<Receipt />}
              loading={expenses.isLoading}
            />
            <StatCard
              label="Yanacaq"
              value={money(totalFuel)}
              hint={
                fuel.data
                  ? `${liters(sum(fuel.data.items, (r) => r.liters))} · ${fuel.data.items.length} dəfə`
                  : undefined
              }
              icon={<Fuel />}
              loading={fuel.isLoading}
            />
            <StatCard
              label="Servis"
              value={money(totalService)}
              hint={maintenance.data ? `${maintenance.data.items.length} qeyd` : undefined}
              icon={<Wrench />}
              loading={maintenance.isLoading}
            />
          </div>

          <p className="px-1 text-xs text-muted-foreground">
            Rəqəmlər bu maşın üzrə son 50 qeyd əsasında hesablanır. Tam məbləğ üçün müvafiq
            bölmədəki filtrlərdən istifadə edin.
          </p>
        </div>
      </div>

      <Tabs defaultValue="expenses">
        <TabsList>
          <TabsTrigger value="expenses">
            Xərclər {count(expenses.data?.totalCount)}
          </TabsTrigger>
          <TabsTrigger value="incomes">Gəlirlər {count(incomes.data?.totalCount)}</TabsTrigger>
          <TabsTrigger value="fuel">Yanacaq {count(fuel.data?.totalCount)}</TabsTrigger>
          <TabsTrigger value="maintenance">
            Servis {count(maintenance.data?.totalCount)}
          </TabsTrigger>
          <TabsTrigger value="insurance">Sığorta {count(insurance.data?.totalCount)}</TabsTrigger>
          <TabsTrigger value="documents">Sənədlər {count(documents.data?.totalCount)}</TabsTrigger>
          <TabsTrigger value="assignments">
            Təyinatlar {count(assignments.data?.totalCount)}
          </TabsTrigger>
        </TabsList>

        <TabsContent value="expenses">
          <MiniTable
            loading={expenses.isLoading}
            rows={expenses.data?.items}
            empty="Bu maşın üzrə xərc qeydi yoxdur."
            head={["Tarix", "Kateqoriya", "Təsvir", "Məbləğ"]}
            row={(r) => [
              date(r.expenseDate),
              <Badge tone={expenseCategories.tone(r.category)}>
                {expenseCategories.label(r.category)}
              </Badge>,
              r.description,
              money(r.amount),
            ]}
            rowKey={(r) => r.id}
          />
        </TabsContent>

        <TabsContent value="incomes">
          <MiniTable
            loading={incomes.isLoading}
            rows={incomes.data?.items}
            empty="Bu maşın üzrə gəlir qeydi yoxdur."
            head={["Tarix", "Sürücü", "Qeyd", "Məbləğ"]}
            row={(r) => [
              date(r.incomeDate),
              r.driverFullName ?? "—",
              r.description || "—",
              <span className="text-success">{money(r.amount)}</span>,
            ]}
            rowKey={(r) => r.id}
          />
        </TabsContent>

        <TabsContent value="fuel">
          <MiniTable
            loading={fuel.isLoading}
            rows={fuel.data?.items}
            empty="Bu maşın üzrə yanacaq qeydi yoxdur."
            head={["Tarix", "Məntəqə", "Litr", "Yürüş", "Məbləğ"]}
            row={(r) => [
              date(r.fuelDate),
              r.stationName,
              liters(r.liters),
              km(r.mileage),
              money(r.price),
            ]}
            rowKey={(r) => r.id}
          />
        </TabsContent>

        <TabsContent value="maintenance">
          <MiniTable
            loading={maintenance.isLoading}
            rows={maintenance.data?.items}
            empty="Bu maşın üzrə servis qeydi yoxdur."
            head={["Tarix", "Növ", "Servis mərkəzi", "Növbəti", "Xərc"]}
            row={(r) => [
              date(r.maintenanceDate),
              <Badge tone={maintenanceTypes.tone(r.serviceType)}>
                {maintenanceTypes.label(r.serviceType)}
              </Badge>,
              r.serviceCenter,
              <DueBadge value={r.nextMaintenanceDate} />,
              money(r.cost),
            ]}
            rowKey={(r) => r.id}
          />
        </TabsContent>

        <TabsContent value="insurance">
          <MiniTable
            loading={insurance.isLoading}
            rows={insurance.data?.items}
            empty="Bu maşın üzrə sığorta qeydi yoxdur."
            head={["Şirkət", "Polis nömrəsi", "Başlama", "Bitmə", "Qiymət"]}
            row={(r) => [
              r.companyName,
              <span className="font-mono text-xs">{r.policyNumber}</span>,
              date(r.startDate),
              <ExpiryBadge value={r.endDate} />,
              money(r.price),
            ]}
            rowKey={(r) => r.id}
          />
        </TabsContent>

        <TabsContent value="documents">
          <MiniTable
            loading={documents.isLoading}
            rows={documents.data?.items}
            empty="Bu maşın üzrə sənəd yoxdur."
            head={["Başlıq", "Növ", "Yüklənib", "Bitmə tarixi", ""]}
            row={(r) => [
              r.title,
              <Badge tone={documentTypes.tone(r.documentType)}>
                {documentTypes.label(r.documentType)}
              </Badge>,
              date(r.uploadDate),
              r.expiryDate ? <ExpiryBadge value={r.expiryDate} /> : "—",
              <a
                href={assetUrl(r.fileUrl)}
                target="_blank"
                rel="noreferrer"
                className="inline-flex items-center gap-1 text-primary hover:underline"
              >
                <Download className="size-3.5" />
                Aç
              </a>,
            ]}
            rowKey={(r) => r.id}
          />
        </TabsContent>

        <TabsContent value="assignments">
          <MiniTable
            loading={assignments.isLoading}
            rows={assignments.data?.items}
            empty="Bu maşın heç bir sürücüyə təyin edilməyib."
            head={["Sürücü", "Verilib", "Qaytarılıb", "Qeyd", "Vəziyyət"]}
            row={(r) => [
              r.driverFullName ?? "—",
              date(r.assignedDate),
              r.returnedDate ? date(r.returnedDate) : "—",
              r.note || "—",
              r.returnedDate ? (
                <Badge tone="neutral">Qaytarılıb</Badge>
              ) : (
                <Badge tone="success" dot>
                  Sürücüdə
                </Badge>
              ),
            ]}
            rowKey={(r) => r.id}
          />
        </TabsContent>
      </Tabs>

      {editing && <VehicleFormDialog vehicle={car} onClose={() => setEditing(false)} />}
    </>
  );
}

function count(total?: number) {
  return total === undefined ? null : (
    <span className="ml-1 text-xs text-muted-foreground tnum">{number(total)}</span>
  );
}

function Row({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <div className="flex items-center justify-between gap-3 text-sm">
      <span className="text-xs text-muted-foreground">{label}</span>
      <span className="text-right">{children}</span>
    </div>
  );
}

/**
 * A read-only table for the detail tabs. Deliberately simpler than DataTable —
 * these panels do not page, sort or edit; they point at the full section.
 */
function MiniTable<T>({
  loading,
  rows,
  head,
  row,
  rowKey,
  empty,
}: {
  loading: boolean;
  rows?: T[];
  head: string[];
  row: (item: T) => React.ReactNode[];
  rowKey: (item: T) => string;
  empty: string;
}) {
  if (loading) {
    return (
      <div className="space-y-2">
        {Array.from({ length: 5 }, (_, i) => (
          <Skeleton key={i} className="h-11 w-full" />
        ))}
      </div>
    );
  }

  if (!rows || rows.length === 0) {
    return (
      <Card>
        <EmptyState title={empty} />
      </Card>
    );
  }

  return (
    <Card className="overflow-hidden">
      <CardHeader className="hidden">
        <CardTitle>Qeydlər</CardTitle>
      </CardHeader>

      <div className="overflow-x-auto">
        <table className="w-full border-collapse text-sm">
          <thead>
            <tr className="border-b border-border bg-surface-raised">
              {head.map((label, index) => (
                <th
                  key={label || index}
                  className={
                    "px-3 py-2.5 text-xs font-medium text-muted-foreground " +
                    (index === head.length - 1 ? "text-right" : "text-left")
                  }
                >
                  {label}
                </th>
              ))}
            </tr>
          </thead>

          <tbody>
            {rows.map((item) => {
              const cells = row(item);

              return (
                <tr key={rowKey(item)} className="border-b border-border last:border-0">
                  {cells.map((cell, index) => (
                    <td
                      key={index}
                      className={
                        "px-3 py-2.5 " +
                        (index === cells.length - 1 ? "text-right whitespace-nowrap" : "")
                      }
                    >
                      {cell}
                    </td>
                  ))}
                </tr>
              );
            })}
          </tbody>
        </table>
      </div>
    </Card>
  );
}
