using Drivious.Data;
using Drivious.DTOs.Dashboard;
using Drivious.Enums;
using Drivious.Models;
using Drivious.Responses;
using Drivious.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Drivious.Services.Implements
{
    public class DashboardService : IDashboardService
    {
        /// <summary>How many months the trend covers, including the current one.</summary>
        private const int TrendMonths = 6;

        /// <summary>How many vehicles the cost ranking returns.</summary>
        private const int TopVehicles = 5;

        private readonly AppDbContext _context;
        private readonly NotificationSettings _notificationSettings;

        public DashboardService(
            AppDbContext context,
            IOptions<NotificationSettings> notificationSettings)
        {
            _context = context;
            _notificationSettings = notificationSettings.Value;
        }

        public async Task<ApiResponse<DashboardGetDTO>> GetDashboardAsync()
        {
            var totalIncome = await _context.Incomes
                .Where(x => !x.IsDeleted)
                .SumAsync(x => x.Amount);

            var totalExpense = await _context.Expenses
                .Where(x => !x.IsDeleted)
                .SumAsync(x => x.Amount);

            var dashboard = new DashboardGetDTO
            {
                TotalVehicles = await _context.Vehicles.CountAsync(x => !x.IsDeleted),

                TotalDrivers = await _context.Drivers.CountAsync(x => !x.IsDeleted),

                ActiveVehicles = await _context.Vehicles.CountAsync(x => x.Status == VehicleStatus.Active && !x.IsDeleted),

                ActiveDrivers = await _context.Drivers.CountAsync(x => x.IsActive && !x.IsDeleted),

                TotalNotifications = await _context.Notifications.CountAsync(x => !x.IsDeleted),

                UnreadNotifications = await _context.Notifications.CountAsync(x => !x.IsDeleted && !x.IsRead),

                AssignedVehicles = await _context.VehicleAssignments
                    .Where(x => !x.IsDeleted && x.IsActive && x.ReturnedDate == null)
                    .Select(x => x.VehicleId)
                    .Distinct()
                    .CountAsync(),

                TotalFuelCost = await _context.FuelLogs
                    .Where(x => !x.IsDeleted)
                    .SumAsync(x => x.Price),

                TotalMaintenanceCost = await _context.Maintenances
                    .Where(x => !x.IsDeleted)
                    .SumAsync(x => x.Cost),

                TotalIncome = totalIncome,

                TotalExpense = totalExpense,

                Profit = totalIncome - totalExpense,

                Upcoming = await BuildUpcomingAsync(),

                MonthlyTotals = await BuildMonthlyTotalsAsync(),

                TopSpendingVehicles = await BuildTopSpendingVehiclesAsync()
            };

            return new ApiResponse<DashboardGetDTO>(
                true,
                "Dashboard retrieved successfully.",
                dashboard
            );
        }

        /// <summary>
        /// Counts the same dates the notification generator watches, over the same
        /// window, so the tiles and the notification list never disagree.
        /// </summary>
        private async Task<UpcomingGetDTO> BuildUpcomingAsync()
        {
            var today = DateTime.UtcNow.Date;
            var horizon = today.AddDays(_notificationSettings.LeadDays);

            var expiringInsurances = await _context.Insurances
                .CountAsync(x => !x.IsDeleted && x.EndDate >= today && x.EndDate <= horizon);

            var expiringLicenses = await _context.Drivers
                .CountAsync(x => !x.IsDeleted
                              && x.LicenseExpireDate >= today
                              && x.LicenseExpireDate <= horizon);

            var dueMaintenances = await _context.Maintenances
                .CountAsync(x => !x.IsDeleted
                              && x.NextMaintenanceDate != null
                              && x.NextMaintenanceDate >= today
                              && x.NextMaintenanceDate <= horizon);

            var expiringDocuments = await _context.VehicleDocuments
                .CountAsync(x => !x.IsDeleted
                              && x.ExpiryDate != null
                              && x.ExpiryDate >= today
                              && x.ExpiryDate <= horizon);

            var overdue = await _context.Insurances.CountAsync(x => !x.IsDeleted && x.EndDate < today)
                        + await _context.Drivers.CountAsync(x => !x.IsDeleted && x.LicenseExpireDate < today)
                        + await _context.Maintenances.CountAsync(x => !x.IsDeleted
                              && x.NextMaintenanceDate != null
                              && x.NextMaintenanceDate < today)
                        + await _context.VehicleDocuments.CountAsync(x => !x.IsDeleted
                              && x.ExpiryDate != null
                              && x.ExpiryDate < today);

            return new UpcomingGetDTO
            {
                WithinDays = _notificationSettings.LeadDays,
                ExpiringInsurances = expiringInsurances,
                ExpiringLicenses = expiringLicenses,
                DueMaintenances = dueMaintenances,
                ExpiringDocuments = expiringDocuments,
                OverdueTotal = overdue
            };
        }

        /// <summary>
        /// Income and expense grouped by month. Months with no rows on either side
        /// are still emitted with zeroes, so a chart shows a gap instead of skipping
        /// straight from March to June.
        /// </summary>
        private async Task<List<MonthlyTotalGetDTO>> BuildMonthlyTotalsAsync()
        {
            var today = DateTime.UtcNow.Date;
            var start = new DateTime(today.Year, today.Month, 1).AddMonths(-(TrendMonths - 1));

            var incomes = await _context.Incomes
                .Where(x => !x.IsDeleted && x.IncomeDate >= start)
                .GroupBy(x => new { x.IncomeDate.Year, x.IncomeDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.Amount) })
                .ToListAsync();

            var expenses = await _context.Expenses
                .Where(x => !x.IsDeleted && x.ExpenseDate >= start)
                .GroupBy(x => new { x.ExpenseDate.Year, x.ExpenseDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.Amount) })
                .ToListAsync();

            var months = new List<MonthlyTotalGetDTO>(TrendMonths);

            for (var i = 0; i < TrendMonths; i++)
            {
                var month = start.AddMonths(i);

                var income = incomes
                    .FirstOrDefault(x => x.Year == month.Year && x.Month == month.Month)?.Total ?? 0m;

                var expense = expenses
                    .FirstOrDefault(x => x.Year == month.Year && x.Month == month.Month)?.Total ?? 0m;

                months.Add(new MonthlyTotalGetDTO
                {
                    Year = month.Year,
                    Month = month.Month,
                    Label = $"{month.Year}-{month.Month:00}",
                    Income = income,
                    Expense = expense,
                    Profit = income - expense
                });
            }

            return months;
        }

        /// <summary>
        /// What each vehicle has cost across expenses, fuel and service. The three
        /// sums are taken separately and combined here; one query joining all three
        /// would multiply the rows against each other.
        /// </summary>
        private async Task<List<VehicleCostGetDTO>> BuildTopSpendingVehiclesAsync()
        {
            var expenses = await _context.Expenses
                .Where(x => !x.IsDeleted)
                .GroupBy(x => x.VehicleId)
                .Select(g => new { VehicleId = g.Key, Total = g.Sum(x => x.Amount) })
                .ToListAsync();

            var fuel = await _context.FuelLogs
                .Where(x => !x.IsDeleted)
                .GroupBy(x => x.VehicleId)
                .Select(g => new { VehicleId = g.Key, Total = g.Sum(x => x.Price) })
                .ToListAsync();

            var maintenance = await _context.Maintenances
                .Where(x => !x.IsDeleted)
                .GroupBy(x => x.VehicleId)
                .Select(g => new { VehicleId = g.Key, Total = g.Sum(x => x.Cost) })
                .ToListAsync();

            var totals = expenses.Concat(fuel).Concat(maintenance)
                .GroupBy(x => x.VehicleId)
                .Select(g => new { VehicleId = g.Key, Total = g.Sum(x => x.Total) })
                .Where(x => x.Total > 0)
                .OrderByDescending(x => x.Total)
                .Take(TopVehicles)
                .ToList();

            if (totals.Count == 0)
            {
                return new List<VehicleCostGetDTO>();
            }

            var ids = totals.Select(x => x.VehicleId).ToList();

            var vehicles = await _context.Vehicles
                .Where(x => ids.Contains(x.Id))
                .Select(x => new { x.Id, x.PlateNumber, x.Brand, x.Model })
                .ToListAsync();

            return totals
                .Join(vehicles,
                    total => total.VehicleId,
                    vehicle => vehicle.Id,
                    (total, vehicle) => new VehicleCostGetDTO
                    {
                        VehicleId = vehicle.Id,
                        PlateNumber = vehicle.PlateNumber,
                        VehicleName = $"{vehicle.Brand} {vehicle.Model}",
                        TotalCost = total.Total
                    })
                .OrderByDescending(x => x.TotalCost)
                .ToList();
        }
    }
}
