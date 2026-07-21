using Drivious.DTOs.Dashboard;
using Drivious.Responses;

namespace Drivious.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<ApiResponse<DashboardGetDTO>> GetDashboardAsync();
    }
}
