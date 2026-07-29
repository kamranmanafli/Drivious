using Drivious.DTOs.FuelLog;
using Drivious.Responses;

namespace Drivious.Services.Interfaces
{
    public interface IFuelLogService
    {
        Task<ApiResponse> CreateAsync(FuelLogCreateDTO dto);

        Task<ApiResponse> RemoveAsync(Guid id);

        Task<ApiResponse<List<FuelLogGetDTO>>> GetAllAsync();

        Task<ApiResponse<List<FuelLogGetDTO>>> GetDeletedAsync();

        Task<ApiResponse<FuelLogGetDTO>> GetAsync(Guid id);

        Task<ApiResponse> UpdateAsync(Guid id, FuelLogUpdateDTO dto);

        Task<ApiResponse> ToggleAsync(Guid id);
    }
}
