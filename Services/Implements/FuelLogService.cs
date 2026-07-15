using Drivious.Data;
using Drivious.DTOs.FuelLog;
using Drivious.Models;
using Drivious.Responses;
using Drivious.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Drivious.Services.Implements
{
    public class FuelLogService : IFuelLogService
    {
        private readonly AppDbContext _context;

        public FuelLogService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<object>> CreateAsync(FuelLogCreateDTO dto)
        {
            FuelLog fuelLog = new()
            {
                CreatedAt = DateTime.Now,

                VehicleId = dto.VehicleId,
                Liters = dto.Liters,
                Price = dto.Price,
                FuelDate = dto.FuelDate,
                Mileage = dto.Mileage,
                StationName = dto.StationName,
            };

            var result = await _context.FuelLogs.AddAsync(fuelLog);

            if (result.State != EntityState.Added)
            {
                return new ApiResponse<object>(
                    false,
                    "Fuel log could not be created.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Fuel log could not be saved.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Fuel log created successfully.",
                null
            );
        }

        public async Task<ApiResponse<List<FuelLogGetDTO>>> GetAllAsync()
        {
            var fuelLogs = await _context.FuelLogs.ToListAsync();

            var dtos = fuelLogs.Select(f => new FuelLogGetDTO
            {
                Id = f.Id,

                VehicleId = f.VehicleId,
                Liters = f.Liters,
                Price = f.Price,
                FuelDate = f.FuelDate,
                Mileage = f.Mileage,
                StationName = f.StationName,

                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt,
                DeletedAt = f.DeletedAt,
                IsDeleted = f.IsDeleted

            }).ToList();

            return new ApiResponse<List<FuelLogGetDTO>>(
                true,
                "Fuel logs retrieved successfully.",
                dtos
            );
        }

        public async Task<ApiResponse<FuelLogGetDTO>> GetAsync(Guid id)
        {
            var fuelLog = await _context.FuelLogs.FindAsync(id);

            if (fuelLog == null)
            {
                return new ApiResponse<FuelLogGetDTO>(
                    false,
                    "Fuel log not found.",
                    null
                );
            }

            var dto = new FuelLogGetDTO
            {
                Id = fuelLog.Id,

                VehicleId = fuelLog.VehicleId,
                Liters = fuelLog.Liters,
                Price = fuelLog.Price,
                FuelDate = fuelLog.FuelDate,
                Mileage = fuelLog.Mileage,
                StationName = fuelLog.StationName,

                CreatedAt = fuelLog.CreatedAt,
                UpdatedAt = fuelLog.UpdatedAt,
                DeletedAt = fuelLog.DeletedAt,
                IsDeleted = fuelLog.IsDeleted
            };

            return new ApiResponse<FuelLogGetDTO>(
                true,
                "Fuel log retrieved successfully.",
                dto
            );
        }

        public async Task<ApiResponse<object>> RemoveAsync(Guid id)
        {
            var fuelLog = await _context.FuelLogs.FindAsync(id);

            if (fuelLog == null)
            {
                return new ApiResponse<object>(
                    false,
                    "Fuel log not found.",
                    null
                );
            }

            var result = _context.FuelLogs.Remove(fuelLog);

            if (result.State != EntityState.Deleted)
            {
                return new ApiResponse<object>(
                    false,
                    "Fuel log could not be deleted.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Fuel log could not be deleted.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Fuel log deleted successfully.",
                null
            );
        }

        public async Task<ApiResponse<object>> ToggleAsync(Guid id)
        {
            var fuelLog = await _context.FuelLogs.FindAsync(id);

            if (fuelLog == null)
            {
                return new ApiResponse<object>(
                    false,
                    "Fuel log not found.",
                    null
                );
            }

            fuelLog.IsDeleted = !fuelLog.IsDeleted;
            fuelLog.DeletedAt = fuelLog.IsDeleted ? DateTime.Now : null;

            var result = _context.FuelLogs.Update(fuelLog);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse<object>(
                    false,
                    "Fuel log status could not be changed.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Fuel log status could not be changed.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Fuel log status changed successfully.",
                null
            );
        }

        public async Task<ApiResponse<object>> UpdateAsync(Guid id, FuelLogUpdateDTO dto)
        {
            var fuelLog = await _context.FuelLogs.FindAsync(id);

            if (fuelLog == null)
            {
                return new ApiResponse<object>(
                    false,
                    "Fuel log not found.",
                    null
                );
            }

            fuelLog.VehicleId = dto.VehicleId ?? fuelLog.VehicleId;
            fuelLog.Liters = dto.Liters ?? fuelLog.Liters;
            fuelLog.Price = dto.Price ?? fuelLog.Price;
            fuelLog.FuelDate = dto.FuelDate ?? fuelLog.FuelDate;
            fuelLog.Mileage = dto.Mileage ?? fuelLog.Mileage;
            fuelLog.StationName = dto.StationName ?? fuelLog.StationName;

            fuelLog.UpdatedAt = DateTime.Now;

            var result = _context.FuelLogs.Update(fuelLog);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse<object>(
                    false,
                    "Fuel log could not be updated.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Fuel log could not be updated.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Fuel log updated successfully.",
                null
            );
        }
    }
}
