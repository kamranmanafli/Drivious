import { Car, Search } from "lucide-react";
import { vehicles as vehiclesApi } from "@/api/endpoints";
import { assetUrl, errorMessage } from "@/api/client";
import type { Vehicle } from "@/api/types";
import { cn } from "@/lib/cn";
import { fuelTypes, vehicleStatuses } from "@/lib/enums";
import { km } from "@/lib/format";
import { useResourceList } from "@/components/useResourceList";
import { Pagination } from "@/components/Pagination";
import { useEffect, useState } from "react";
import {
  Badge,
  Card,
  EmptyState,
  ErrorState,
  Input,
  NativeSelect,
  Skeleton,
} from "@/ui";
import { EnumOptions } from "@/components/pickers";

/**
 * Read-only fleet browser for the Driver role — the API lets that role read
 * vehicles, so this is a list of cards rather than the console's data table.
 */
export function DriverFleetPage() {
  const list = useResourceList<Vehicle>({
    key: "vehicles",
    fetcher: vehiclesApi.list,
    defaultSort: "plateNumber",
    defaultDescending: false,
    pageSize: 12,
    filters: ["status"],
  });

  const [draft, setDraft] = useState(list.value("search"));

  useEffect(() => {
    const timer = setTimeout(() => {
      if (draft !== list.value("search")) list.setParam("search", draft);
    }, 300);
    return () => clearTimeout(timer);
  }, [draft, list]);

  return (
    <>
      <div>
        <h1 className="text-xl font-semibold tracking-tight">Filo</h1>
        <p className="mt-0.5 text-sm text-muted-foreground">
          Şirkətin bütün maşınları. Yalnız oxumaq üçün.
        </p>
      </div>

      <div className="flex gap-2">
        <div className="relative min-w-0 flex-1">
          <Search className="pointer-events-none absolute left-2.5 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            type="search"
            value={draft}
            onChange={(event) => setDraft(event.target.value)}
            placeholder="Marka, model və ya nişan…"
            className="pl-8"
          />
        </div>

        <NativeSelect
          value={list.value("status")}
          onChange={(event) => list.setParam("status", event.target.value)}
          className="w-32 shrink-0"
        >
          <EnumOptions entries={vehicleStatuses.list} placeholder="Hamısı" />
        </NativeSelect>
      </div>

      {list.error ? (
        <Card>
          <ErrorState message={errorMessage(list.error)} onRetry={() => void list.refetch()} />
        </Card>
      ) : list.isLoading ? (
        <div className="grid gap-3 sm:grid-cols-2">
          {Array.from({ length: 6 }, (_, i) => (
            <Skeleton key={i} className="h-44 w-full" />
          ))}
        </div>
      ) : list.items.length === 0 ? (
        <Card>
          <EmptyState
            icon={<Car />}
            title="Maşın tapılmadı"
            description="Axtarışı və ya filtri dəyişib yenidən cəhd edin."
          />
        </Card>
      ) : (
        <div className={cn("space-y-3", list.isFetching && "opacity-60")}>
          <div className="grid gap-3 sm:grid-cols-2">
            {list.items.map((vehicle) => (
              <Card key={vehicle.id} className="overflow-hidden">
                <div className="aspect-[16/10] w-full bg-muted">
                  {vehicle.imageURL ? (
                    <img
                      src={assetUrl(vehicle.imageURL)}
                      alt=""
                      loading="lazy"
                      className="size-full object-cover"
                    />
                  ) : (
                    <div className="flex size-full items-center justify-center">
                      <Car className="size-8 text-muted-foreground/50" />
                    </div>
                  )}
                </div>

                <div className="space-y-2 p-3.5">
                  <div className="flex items-start justify-between gap-2">
                    <div className="min-w-0">
                      <p className="truncate text-sm font-semibold">
                        {vehicle.brand} {vehicle.model}
                      </p>
                      <p className="font-mono text-xs text-muted-foreground">
                        {vehicle.plateNumber}
                      </p>
                    </div>

                    <Badge tone={vehicleStatuses.tone(vehicle.status)} dot>
                      {vehicleStatuses.label(vehicle.status)}
                    </Badge>
                  </div>

                  <dl className="grid grid-cols-3 gap-2 border-t border-border pt-2.5">
                    <div>
                      <dt className="text-[11px] text-muted-foreground">İl</dt>
                      <dd className="text-xs font-medium tnum">{vehicle.year}</dd>
                    </div>
                    <div>
                      <dt className="text-[11px] text-muted-foreground">Yanacaq</dt>
                      <dd className="truncate text-xs font-medium">
                        {fuelTypes.label(vehicle.fuelType)}
                      </dd>
                    </div>
                    <div>
                      <dt className="text-[11px] text-muted-foreground">Yürüş</dt>
                      <dd className="text-xs font-medium tnum">{km(vehicle.mileage)}</dd>
                    </div>
                  </dl>
                </div>
              </Card>
            ))}
          </div>

          {list.result && <Pagination result={list.result} onPage={list.setPage} />}
        </div>
      )}
    </>
  );
}
