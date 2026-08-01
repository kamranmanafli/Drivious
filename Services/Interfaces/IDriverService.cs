using Drivious.DTOs.Common;
using Drivious.DTOs.Driver;
using Drivious.Responses;

namespace Drivious.Services.Interfaces
{
    public interface IDriverService
    {
        Task<ApiResponse> CreateAsync(DriverCreateDTO dto);

        Task<ApiResponse> RemoveAsync(Guid id);

        Task<ApiResponse<PagedResult<DriverGetDTO>>> GetAllAsync(DriverQueryParameters parameters);

        Task<ApiResponse<PagedResult<DriverGetDTO>>> GetDeletedAsync(DriverQueryParameters parameters);

        Task<ApiResponse<DriverGetDTO>> GetAsync(Guid id);

        Task<ApiResponse> UpdateAsync(Guid id, DriverUpdateDTO dto);

        Task<ApiResponse> ToggleAsync(Guid id);
    }
}
