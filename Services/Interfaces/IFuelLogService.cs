using Drivious.DTOs.FuelLog;
using Drivious.Responses;

namespace Drivious.Services.Interfaces
{
    public interface IFuelLogService
    {
        Task<ApiResponse<object>> CreateAsync(FuelLogCreateDTO dto);

        Task<ApiResponse<object>> RemoveAsync(Guid id);

        Task<ApiResponse<List<FuelLogGetDTO>>> GetAllAsync();

        Task<ApiResponse<FuelLogGetDTO>> GetAsync(Guid id);

        Task<ApiResponse<object>> UpdateAsync(Guid id, FuelLogUpdateDTO dto);

        Task<ApiResponse<object>> ToggleAsync(Guid id);
    }
}
