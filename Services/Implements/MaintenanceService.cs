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

        public MaintenanceService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<object>> CreateAsync(MaintenanceCreateDTO dto)
        {
            Maintenance maintenance = new()
            {
                CreatedAt = DateTime.Now,

                VehicleId = dto.VehicleId,
                ServiceType = dto.ServiceType,
                Description = dto.Description,
                Cost = dto.Cost,
                MaintenanceDate = dto.MaintenanceDate,
                NextMaintenanceDate = dto.NextMaintenanceDate,
                Mileage = dto.Mileage,
                ServiceCenter = dto.ServiceCenter,
            };

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

            var dtos = maintenances.Select(m => new MaintenanceGetDTO
            {
                Id = m.Id,

                VehicleId = m.VehicleId,
                ServiceType = m.ServiceType,
                Description = m.Description,
                Cost = m.Cost,
                MaintenanceDate = m.MaintenanceDate,
                NextMaintenanceDate = m.NextMaintenanceDate,
                Mileage = m.Mileage,
                ServiceCenter = m.ServiceCenter,

                CreatedAt = m.CreatedAt,
                UpdatedAt = m.UpdatedAt,
                DeletedAt = m.DeletedAt,
                IsDeleted = m.IsDeleted

            }).ToList();

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

            var dto = new MaintenanceGetDTO
            {
                Id = maintenance.Id,

                VehicleId = maintenance.VehicleId,
                ServiceType = maintenance.ServiceType,
                Description = maintenance.Description,
                Cost = maintenance.Cost,
                MaintenanceDate = maintenance.MaintenanceDate,
                NextMaintenanceDate = maintenance.NextMaintenanceDate,
                Mileage = maintenance.Mileage,
                ServiceCenter = maintenance.ServiceCenter,

                CreatedAt = maintenance.CreatedAt,
                UpdatedAt = maintenance.UpdatedAt,
                DeletedAt = maintenance.DeletedAt,
                IsDeleted = maintenance.IsDeleted
            };

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

            maintenance.VehicleId = dto.VehicleId ?? maintenance.VehicleId;
            maintenance.ServiceType = dto.ServiceType ?? maintenance.ServiceType;
            maintenance.Description = dto.Description ?? maintenance.Description;
            maintenance.Cost = dto.Cost ?? maintenance.Cost;
            maintenance.MaintenanceDate = dto.MaintenanceDate ?? maintenance.MaintenanceDate;
            maintenance.NextMaintenanceDate = dto.NextMaintenanceDate ?? maintenance.NextMaintenanceDate;
            maintenance.Mileage = dto.Mileage ?? maintenance.Mileage;
            maintenance.ServiceCenter = dto.ServiceCenter ?? maintenance.ServiceCenter;

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
