using Drivious.DTOs.Driver;
using Drivious.Responses;

namespace Drivious.Services.Interfaces
{
    public interface IDriverService
    {
        Task<ApiResponse<object>> CreateAsync(DriverCreateDTO dto);

        Task<ApiResponse<object>> RemoveAsync(Guid id);

        Task<ApiResponse<List<DriverGetDTO>>> GetAllAsync();

        Task<ApiResponse<DriverGetDTO>> GetAsync(Guid id);

        Task<ApiResponse<object>> UpdateAsync(Guid id, DriverUpdateDTO dto);

        Task<ApiResponse<object>> ToggleAsync(Guid id);
    }
}
