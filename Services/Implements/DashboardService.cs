using Drivious.Data;
using Drivious.DTOs.Dashboard;
using Drivious.Enums;
using Drivious.Responses;
using Drivious.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Drivious.Services.Implements
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<DashboardGetDTO>> GetDashboardAsync()
        {
            var totalIncome = await _context.Incomes.SumAsync(x => x.Amount);
            var totalExpense = await _context.Expenses.SumAsync(x => x.Amount);

            var dashboard = new DashboardGetDTO
            {
                TotalVehicles = await _context.Vehicles.CountAsync(x => !x.IsDeleted),

                TotalDrivers = await _context.Drivers.CountAsync(x => !x.IsDeleted),

                ActiveVehicles = await _context.Vehicles.CountAsync(x => x.Status == VehicleStatus.Active && !x.IsDeleted),

                ActiveDrivers = await _context.Drivers.CountAsync(x => x.IsActive && !x.IsDeleted),

                TotalNotifications = await _context.Notifications.CountAsync(x => !x.IsDeleted),

                TotalIncome = totalIncome,

                TotalExpense = totalExpense,

                Profit = totalIncome - totalExpense
            };

            return new ApiResponse<DashboardGetDTO>(
                true,
                "Dashboard retrieved successfully.",
                dashboard
            );
        }
    }
}

