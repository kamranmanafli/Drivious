import { Navigate, Route, Routes } from "react-router-dom";
import { useAuth } from "./auth/AuthContext";
import { RequireAuth, RequireRole } from "./auth/guards";
import { Role } from "./api/types";
import { Spinner } from "./ui";

import { LoginPage } from "./app/LoginPage";
import { RegisterPage } from "./app/RegisterPage";

import { ConsoleLayout } from "./app/console/ConsoleLayout";
import { DashboardPage } from "./app/console/DashboardPage";
import { VehiclesPage } from "./app/console/VehiclesPage";
import { VehicleDetailPage } from "./app/console/VehicleDetailPage";
import { DriversPage } from "./app/console/DriversPage";
import { AssignmentsPage } from "./app/console/AssignmentsPage";
import { IncomesPage } from "./app/console/IncomesPage";
import { ExpensesPage } from "./app/console/ExpensesPage";
import { FuelPage } from "./app/console/FuelPage";
import { MaintenancePage } from "./app/console/MaintenancePage";
import { InsurancePage } from "./app/console/InsurancePage";
import { DocumentsPage } from "./app/console/DocumentsPage";
import { NotificationsPage } from "./app/console/NotificationsPage";
import { ArchivePage } from "./app/console/ArchivePage";

import { UsersPage } from "./app/admin/UsersPage";

import { DriverLayout } from "./app/driver/DriverLayout";
import { DriverHomePage } from "./app/driver/DriverHomePage";
import { DriverEarningsPage } from "./app/driver/DriverEarningsPage";
import { DriverFleetPage } from "./app/driver/DriverFleetPage";
import { DriverNotificationsPage } from "./app/driver/DriverNotificationsPage";
import { DriverProfilePage } from "./app/driver/DriverProfilePage";

const MANAGE = [Role.Admin, Role.Manager];

/**
 * Two shells behind one set of routes.
 *
 * The role decides which one loads: a Driver gets the phone-shaped app, and
 * everyone else gets the console. They are kept apart rather than shown as one
 * screen with pieces hidden, because a driver's job — check my car, check my
 * earnings — is not a narrower version of a manager's.
 */
export function AppRoutes() {
  const { ready, isDriverOnly } = useAuth();

  // Deciding before the stored token has been checked would flash the console
  // at a driver, or bounce a signed-in user to /login.
  if (!ready) {
    return (
      <div className="flex min-h-dvh items-center justify-center">
        <Spinner className="size-6" />
      </div>
    );
  }

  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />

      <Route element={<RequireAuth />}>
        {isDriverOnly ? (
          <Route element={<DriverLayout />}>
            <Route index element={<DriverHomePage />} />
            <Route path="my/earnings" element={<DriverEarningsPage />} />
            <Route path="my/fleet" element={<DriverFleetPage />} />
            <Route path="my/notifications" element={<DriverNotificationsPage />} />
            <Route path="my/profile" element={<DriverProfilePage />} />

            <Route path="*" element={<Navigate to="/" replace />} />
          </Route>
        ) : (
          <Route element={<ConsoleLayout />}>
            {/* The dashboard endpoint is Manager and Admin only. */}
            <Route element={<RequireRole allow={MANAGE} />}>
              <Route index element={<DashboardPage />} />
            </Route>

            <Route path="vehicles" element={<VehiclesPage />} />
            <Route path="vehicles/:id" element={<VehicleDetailPage />} />
            <Route path="drivers" element={<DriversPage />} />
            <Route path="assignments" element={<AssignmentsPage />} />

            <Route path="incomes" element={<IncomesPage />} />
            <Route path="expenses" element={<ExpensesPage />} />
            <Route path="fuel" element={<FuelPage />} />

            <Route path="maintenance" element={<MaintenancePage />} />
            <Route path="insurance" element={<InsurancePage />} />
            <Route path="documents" element={<DocumentsPage />} />

            <Route path="notifications" element={<NotificationsPage />} />

            {/* Listing archived rows is a Manager capability; the permanent
                delete inside is gated separately on Admin. */}
            <Route element={<RequireRole allow={MANAGE} />}>
              <Route path="archive" element={<ArchivePage />} />
            </Route>

            <Route element={<RequireRole allow={[Role.Admin]} />}>
              <Route path="admin/users" element={<UsersPage />} />
            </Route>

            <Route path="*" element={<Navigate to="/vehicles" replace />} />
          </Route>
        )}
      </Route>
    </Routes>
  );
}
