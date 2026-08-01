# Drivious API

Fleet management REST API — vehicles, drivers, assignments, costs and automatic
expiry warnings.

Built with ASP.NET Core 8, Entity Framework Core (SQL Server), ASP.NET Identity
with JWT, AutoMapper, FluentValidation and Swagger.

---

## Requirements

- .NET 8 SDK
- SQL Server (LocalDB, Express or full)

---

## Setup

Secrets are never stored in `appsettings.json`. In development they live in user
secrets; in production they come from environment variables.

```bash
dotnet user-secrets set "ConnectionStrings:default" "Server=.;Database=Drivious;Trusted_Connection=True;TrustServerCertificate=True"
```

```bash
dotnet user-secrets set "Jwt:Key" "<at least 32 characters>"
```

Optionally seed the first administrator — without these three the application
starts, but no account exists yet:

```bash
dotnet user-secrets set "Seed:AdminUserName" "admin"
```

```bash
dotnet user-secrets set "Seed:AdminEmail" "admin@drivious.local"
```

```bash
dotnet user-secrets set "Seed:AdminPassword" "<strong password>"
```

In production the same values are read from `ConnectionStrings__default`,
`Jwt__Key`, `Seed__AdminPassword` and so on.

## Run

```bash
dotnet run
```

Migrations are applied automatically at startup, so a fresh machine only needs a
reachable SQL Server. Swagger UI is served at `/swagger` in development.

---

## Configuration

Non-secret settings live in `appsettings.json`.

| Key | Meaning |
|---|---|
| `Cors:AllowedOrigins` | Front end origins allowed to call the API. An origin missing from this list is blocked by the browser. The application refuses to start when the list is empty. |
| `Jwt:ExpireDays` | Access token lifetime. Kept short because an access token cannot be revoked. |
| `Jwt:RefreshTokenExpireDays` | Refresh token lifetime. |
| `Notifications:LeadDays` | How far ahead a date is warned about, and how far back an already passed one is still reported. |
| `Notifications:ScanIntervalHours` | Gap between two background scans. The first runs at startup. |
| `Notifications:Enabled` | Set to `false` to stop the background scan. |

---

## Authentication

The API uses JWT bearer tokens. `POST /api/auths/login` returns an access token
and a refresh token; the client renews the short-lived access token through
`POST /api/auths/refresh`. Refresh tokens rotate — presenting one retires it and
issues a replacement, so a leaked token cannot be reused.

Changing a password, being assigned a different role, or being linked to another
driver revokes every outstanding refresh token for that account.

### Roles

| Role | Can do |
|---|---|
| `Admin` | Everything, including permanent deletes and role assignment. |
| `Manager` | Create, update and archive fleet data; view archived records. |
| `Driver` | Read fleet data. Incomes and vehicle assignments are narrowed to the driver's own records. |

Self-registration always lands in `Driver`; an administrator promotes from there
with `POST /api/auths/assign-role`.

An account is tied to a driver record with `POST /api/auths/link-driver`. Without
that link a `Driver` account sees no incomes and no assignments at all.

---

## Endpoints

Every resource follows the same shape:

| Method | Route | Role |
|---|---|---|
| `GET` | `/api/{resource}` | read |
| `GET` | `/api/{resource}/{id}` | read |
| `GET` | `/api/{resource}/deleted` | manage |
| `POST` | `/api/{resource}` | manage |
| `PATCH` | `/api/{resource}/{id}` | manage |
| `PATCH` | `/api/{resource}/toggle/{id}` | manage |
| `DELETE` | `/api/{resource}/{id}` | admin |

Resources: `vehicles`, `drivers`, `vehicleassignments`, `expenses`, `incomes`,
`maintenances`, `insurances`, `fuellogs`, `vehicledocuments`, `notifications`.

Additional endpoints:

| Method | Route | Purpose |
|---|---|---|
| `GET` | `/api/dashboards` | Totals, six-month income/expense trend, cost ranking and expiry counts. |
| `POST` | `/api/notifications/scan` | Runs the expiry scan immediately instead of waiting for the background pass. |
| `PATCH` | `/api/vehicleassignments/return/{id}` | Records a vehicle coming back. |
| `POST` | `/api/auths/register`, `login`, `refresh`, `logout`, `change-password`, `assign-role`, `link-driver` | |
| `GET` | `/api/auths/me` | The current account and its roles. |

### Deleting

`DELETE` removes a row permanently and is refused for a vehicle that still has
history. `PATCH toggle/{id}` is the soft delete: it archives the record and, for a
vehicle or a driver, archives its related rows with it. Restoring the parent brings
back only the rows that cascade removed — a record archived deliberately stays
archived.

---

## Query parameters

Every list endpoint accepts paging, search and sorting:

| Parameter | Default | Notes |
|---|---|---|
| `page` | 1 | Values below 1 are corrected, not rejected. |
| `pageSize` | 20 | Capped at 100. |
| `search` | — | Which columns it covers is documented per resource. |
| `sortBy` | per resource | Unknown names fall back to the default order. |
| `descending` | false | |

Plus resource-specific filters, for example:

```
GET /api/vehicles?status=0&fuelType=1&minYear=2015&search=toyota&sortBy=mileage&descending=true
GET /api/expenses?vehicleId=<guid>&category=2&from=2026-01-01&to=2026-06-30&minAmount=100
GET /api/incomes?driverId=<guid>&from=2026-01-01&sortBy=amount&descending=true
GET /api/maintenances?dueBefore=2026-09-01
GET /api/insurances?activeOn=2026-08-01
GET /api/vehicleassignments?isOpen=true
GET /api/notifications?type=3&isRead=false
```

Responses are wrapped:

```json
{
  "success": true,
  "message": "Vehicles retrieved successfully.",
  "data": {
    "items": [],
    "page": 1,
    "pageSize": 20,
    "totalCount": 0,
    "totalPages": 0,
    "hasPrevious": false,
    "hasNext": false
  }
}
```

---

## Expiry notifications

A background service scans the fleet on a timer and raises a notification for
every date that is about to pass or already has:

| Watched | Source |
|---|---|
| Insurance | `Insurance.EndDate` |
| Driving licence | `Driver.LicenseExpireDate` |
| Scheduled service | `Maintenance.NextMaintenanceDate` |
| Document | `VehicleDocument.ExpiryDate` |

A date still ahead produces a warning; one that has passed produces an error. Each
notification carries a reference key naming the reason, the row and the date, so
repeated scans never duplicate a warning — running the scan twice in a row creates
nothing the second time.

`POST /api/notifications/scan` triggers the same pass on demand.

---

## Project layout

```
Controllers/    HTTP endpoints, role attributes
Services/       Business rules
  Interfaces/
  Implements/
  Background/   The notification scan
Data/           DbContext, seeding, shared fleet checks
Models/         EF entities
DTOs/           Request and response shapes, per resource
Validators/     FluentValidation rules
Mappings/       AutoMapper profile
Middlewares/    Exception handling
Extensions/     Query and file helpers
Migrations/     EF migrations
```

---

## Notes on a few decisions

- **List endpoints project in the database.** `ProjectTo` turns a list query into a
  single SQL join, so a page of expenses returns its vehicle's plate number without
  loading vehicle rows or issuing a second query.
- **Sorting is allow-listed.** Each resource declares which fields can be ordered
  by; building an expression from an arbitrary caller-supplied name would put user
  input into the query itself.
- **Uploads keep only the client's file extension.** The rest of the supplied file
  name is discarded, so a name containing path segments cannot escape the target
  folder.
- **Unique VIN and plate numbers are filtered indexes.** A soft-deleted vehicle does
  not block reusing its plate.
