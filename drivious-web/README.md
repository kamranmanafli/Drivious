# Drivious Web

Front end for the Drivious fleet API — vehicles, drivers, assignments, costs and
expiry warnings.

Two interfaces in one project. The role decides which one loads at sign-in:

| Shell | Roles | Shape |
|---|---|---|
| Console | Admin, Manager | Desktop-first: sidebar, data tables, filters |
| Driver app | Driver | Phone-first: bottom navigation, cards |

An admin-only section (`/admin/users`) sits inside the console and is hidden from
managers.

Built with Vite, React 18, TypeScript, Tailwind CSS 4, TanStack Query, Radix
primitives and Recharts. Interface language is Azerbaijani.

---

## Run

```bash
npm install
```

```bash
npm run dev
```

Opens on <http://localhost:5173>, which is already in the API's
`Cors:AllowedOrigins`. Changing the port means adding the new origin there too,
or the browser blocks every request.

| Script | Does |
|---|---|
| `npm run dev` | Development server |
| `npm run build` | Type-check and build to `dist/` |
| `npm run preview` | Serve the built output |
| `npm run typecheck` | Type-check only |

---

## Configuration

`.env`:

| Key | Meaning |
|---|---|
| `VITE_API_URL` | Origin of the ASP.NET API, e.g. `http://localhost:5221`. |
| `VITE_DEMO` | `true` runs on in-memory sample data with no backend. `false` talks to the API. |

### Demo mode

`VITE_DEMO=true` swaps in an axios adapter that answers from memory instead of
the network. It reimplements what a person can observe — the response envelope,
paging, the per-resource filters and sort allow-lists, the role rules, the driver
scoping, soft delete with cascade, and the dashboard aggregation — so the whole
interface can be opened and reviewed without a SQL Server instance.

Everything above the adapter runs unchanged: the same client, interceptors,
refresh rotation, error handling and screens.

Three accounts, shown on the sign-in screen:

| User | Password | Role |
|---|---|---|
| `admin` | `Admin123!` | Admin |
| `menecer` | `Menecer123!` | Manager |
| `surucu` | `Surucu123!` | Driver, linked to a driver record |

Demo mode is **not** a second source of truth for business rules. Where it and
the API disagree, the API is right.

Set `VITE_DEMO=false` to point at the real backend. Nothing else changes.

---

## Layout

```
src/
  api/          Client, endpoints, request/response types, message translation
    demo/       The in-memory adapter and its sample fleet
  auth/         Session context and route guards
  lib/          Enum labels, formatting, class-name helper
  ui/           Design-system primitives
  components/   DataTable, Toolbar, forms, pickers
  app/
    console/    Admin + Manager screens
    admin/      Admin-only screens
    driver/     Driver screens
  routes.tsx    Picks the shell by role
```

---

## Notes on a few decisions

- **One column definition drives both layouts.** Each `Column` declares where it
  lands in the mobile card (`title`, `subtitle`, `trailing`, `meta`), so a column
  added to the table cannot be forgotten on the phone.

- **List state lives in the URL.** Paging, search, sort and filters are query
  parameters, so a filtered view survives a refresh and can be pasted to a
  colleague — which is what someone actually does with "every insurance expiring
  this month".

- **Formatting does not go through the `az-AZ` locale.** Not every browser ships
  Azerbaijani data, and the ones that do not fall back to the root locale, which
  renders a date as `2026-08-15` and a distance as `170,933 km`. Dates, numbers
  and month names are produced in `lib/format.ts` instead, so the app reads the
  same everywhere.

- **API messages are translated, not replaced.** `api/messages.ts` maps the
  server's English to Azerbaijani; the regular CRUD confirmations are covered by
  rules rather than entries. Anything unmatched is passed through untouched — a
  message the user cannot read still beats swallowing it.

- **Two casings are copied from the server rather than normalised.**
  `Vehicle.ImageURL` serialises as `imageURL` while `Driver.ImageUrl` serialises
  as `imageUrl`, because System.Text.Json only lowercases the leading uppercase
  run. `Vehicle.VIN` becomes `vin`. Renaming them here would just mean the field
  arrives undefined.

- **Uploaded asset URLs are rewritten onto `VITE_API_URL`.** The API builds them
  from its own request host, so an image saved while it answered on localhost
  keeps that host.

- **Role checks in the interface only hide guaranteed failures.** The API
  enforces the same split; nothing here is a security boundary.

---

## What the API does not offer

Two things shaped the interface and are worth knowing:

- **Notifications are global.** They are not addressed to an account, so every
  user sees the same list and "read" applies to everyone. The console says so on
  the page; the driver app shows the feed read-only.

- **The dashboard is Manager and Admin only.** The driver's home screen builds
  its own summary from the lists the API already narrows to them.

`GET /api/auths/users` was added to the API alongside this client — without it
there is no way to discover which accounts exist, and the admin screen could only
have been a form taking a user name the operator already knew.
