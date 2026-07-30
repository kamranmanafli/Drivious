using AutoMapper;
using Drivious.Data;
using Drivious.DTOs.Maintenance;
using Drivious.Models;
using Drivious.Responses;
using Drivious.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Drivious.Services.Implements
{
    public class MaintenanceService : IMaintenanceService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public MaintenanceService(
            AppDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ApiResponse> CreateAsync(MaintenanceCreateDTO dto)
        {
            Maintenance maintenance = _mapper.Map<Maintenance>(dto);

            maintenance.CreatedAt = DateTime.UtcNow;

            var result = await _context.Maintenances.AddAsync(maintenance);

            if (result.State != EntityState.Added)
            {
                return new ApiResponse(
                    false,
                    "Maintenance could not be created."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Maintenance could not be saved."
                );
            }

            return new ApiResponse(
                true,
                "Maintenance created successfully."
            );
        }

        public async Task<ApiResponse<List<MaintenanceGetDTO>>> GetAllAsync()
        {
            var maintenances = await _context.Maintenances
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToListAsync();

            var dtos = _mapper.Map<List<MaintenanceGetDTO>>(maintenances);

            return new ApiResponse<List<MaintenanceGetDTO>>(
                true,
                "Maintenances retrieved successfully.",
                dtos
            );
        }

        public async Task<ApiResponse<List<MaintenanceGetDTO>>> GetDeletedAsync()
        {
            var maintenances = await _context.Maintenances
                .AsNoTracking()
                .Where(x => x.IsDeleted)
                .ToListAsync();

            var dtos = _mapper.Map<List<MaintenanceGetDTO>>(maintenances);

            return new ApiResponse<List<MaintenanceGetDTO>>(
                true,
                "Deleted maintenances retrieved successfully.",
                dtos
            );
        }

        public async Task<ApiResponse<MaintenanceGetDTO>> GetAsync(Guid id)
        {
            var maintenance = await _context.Maintenances
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (maintenance == null)
            {
                return new ApiResponse<MaintenanceGetDTO>(
                    false,
                    "Maintenance not found.",
                    null
                );
            }

            var dto = _mapper.Map<MaintenanceGetDTO>(maintenance);

            return new ApiResponse<MaintenanceGetDTO>(
                true,
                "Maintenance retrieved successfully.",
                dto
            );
        }

        public async Task<ApiResponse> RemoveAsync(Guid id)
        {
            var maintenance = await _context.Maintenances.FindAsync(id);

            if (maintenance == null)
            {
                return new ApiResponse(
                    false,
                    "Maintenance not found."
                );
            }

            var result = _context.Maintenances.Remove(maintenance);

            if (result.State != EntityState.Deleted)
            {
                return new ApiResponse(
                    false,
                    "Maintenance could not be deleted."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Maintenance could not be deleted."
                );
            }

            return new ApiResponse(
                true,
                "Maintenance deleted successfully."
            );
        }

        public async Task<ApiResponse> ToggleAsync(Guid id)
        {
            var maintenance = await _context.Maintenances.FindAsync(id);

            if (maintenance == null)
            {
                return new ApiResponse(
                    false,
                    "Maintenance not found."
                );
            }

            maintenance.IsDeleted = !maintenance.IsDeleted;
            maintenance.DeletedAt = maintenance.IsDeleted ? DateTime.UtcNow : null;

            var result = _context.Maintenances.Update(maintenance);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse(
                    false,
                    "Maintenance status could not be changed."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Maintenance status could not be changed."
                );
            }

            return new ApiResponse(
                true,
                "Maintenance status changed successfully."
            );
        }

        public async Task<ApiResponse> UpdateAsync(Guid id, MaintenanceUpdateDTO dto)
        {
            var maintenance = await _context.Maintenances.FindAsync(id);

            if (maintenance == null)
            {
                return new ApiResponse(
                    false,
                    "Maintenance not found."
                );
            }

            _mapper.Map(dto, maintenance);

            maintenance.UpdatedAt = DateTime.UtcNow;

            var result = _context.Maintenances.Update(maintenance);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse(
                    false,
                    "Maintenance could not be updated."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Maintenance could not be updated."
                );
            }

            return new ApiResponse(
                true,
                "Maintenance updated successfully."
            );
        }
    }
}
