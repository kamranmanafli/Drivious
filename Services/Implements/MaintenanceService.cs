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

        public async Task<ApiResponse<object>> CreateAsync(MaintenanceCreateDTO dto)
        {
            Maintenance maintenance = _mapper.Map<Maintenance>(dto);

            maintenance.CreatedAt = DateTime.Now;

            var result = await _context.Maintenances.AddAsync(maintenance);

            if (result.State != EntityState.Added)
            {
                return new ApiResponse<object>(
                    false,
                    "Maintenance could not be created.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Maintenance could not be saved.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Maintenance created successfully.",
                null
            );
        }

        public async Task<ApiResponse<List<MaintenanceGetDTO>>> GetAllAsync()
        {
            var maintenances = await _context.Maintenances.ToListAsync();

            var dtos = _mapper.Map<List<MaintenanceGetDTO>>(maintenances);

            return new ApiResponse<List<MaintenanceGetDTO>>(
                true,
                "Maintenances retrieved successfully.",
                dtos
            );
        }

        public async Task<ApiResponse<MaintenanceGetDTO>> GetAsync(Guid id)
        {
            var maintenance = await _context.Maintenances.FindAsync(id);

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

        public async Task<ApiResponse<object>> RemoveAsync(Guid id)
        {
            var maintenance = await _context.Maintenances.FindAsync(id);

            if (maintenance == null)
            {
                return new ApiResponse<object>(
                    false,
                    "Maintenance not found.",
                    null
                );
            }

            var result = _context.Maintenances.Remove(maintenance);

            if (result.State != EntityState.Deleted)
            {
                return new ApiResponse<object>(
                    false,
                    "Maintenance could not be deleted.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Maintenance could not be deleted.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Maintenance deleted successfully.",
                null
            );
        }

        public async Task<ApiResponse<object>> ToggleAsync(Guid id)
        {
            var maintenance = await _context.Maintenances.FindAsync(id);

            if (maintenance == null)
            {
                return new ApiResponse<object>(
                    false,
                    "Maintenance not found.",
                    null
                );
            }

            maintenance.IsDeleted = !maintenance.IsDeleted;
            maintenance.DeletedAt = maintenance.IsDeleted ? DateTime.Now : null;

            var result = _context.Maintenances.Update(maintenance);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse<object>(
                    false,
                    "Maintenance status could not be changed.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Maintenance status could not be changed.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Maintenance status changed successfully.",
                null
            );
        }

        public async Task<ApiResponse<object>> UpdateAsync(Guid id, MaintenanceUpdateDTO dto)
        {
            var maintenance = await _context.Maintenances.FindAsync(id);

            if (maintenance == null)
            {
                return new ApiResponse<object>(
                    false,
                    "Maintenance not found.",
                    null
                );
            }

            _mapper.Map(dto, maintenance);

            maintenance.UpdatedAt = DateTime.Now;

            var result = _context.Maintenances.Update(maintenance);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse<object>(
                    false,
                    "Maintenance could not be updated.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Maintenance could not be updated.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Maintenance updated successfully.",
                null
            );
        }
    }
}
