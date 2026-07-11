using Drivious.Data;
using Drivious.DTOs.FuelLog;
using Drivious.Models;
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

        public async Task<bool> CreateAsync(FuelLogCreateDTO dto)
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
                return false;

            var saveCount = await _context.SaveChangesAsync();

            return saveCount > 0;
        }

        public async Task<List<FuelLogGetDTO>> GetAllAsync()
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

            return dtos;
        }

        public async Task<FuelLogGetDTO> GetAsync(Guid id)
        {
            var fuelLog = await _context.FuelLogs.FindAsync(id);

            if (fuelLog == null)
            {
                throw new Exception("FuelLog not found!");
            }

            var dto = new FuelLogGetDTO()
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

            return dto;
        }

        public async Task<bool> RemoveAsync(Guid id)
        {
            var fuelLog = await _context.FuelLogs.FindAsync(id);

            if (fuelLog == null)
                return false;

            var result = _context.FuelLogs.Remove(fuelLog);

            if (result.State != EntityState.Deleted)
                return false;

            var saveCount = await _context.SaveChangesAsync();

            return saveCount > 0;
        }

        public async Task<bool> ToggleAsync(Guid id)
        {
            var fuelLog = await _context.FuelLogs.FindAsync(id);

            if (fuelLog == null)
                return false;

            fuelLog.IsDeleted = !fuelLog.IsDeleted;

            fuelLog.DeletedAt = fuelLog.IsDeleted ? DateTime.Now : null;

            var result = _context.FuelLogs.Update(fuelLog);

            if (result.State != EntityState.Modified)
                return false;

            var saveCount = await _context.SaveChangesAsync();

            return saveCount > 0;
        }

        public async Task<bool> UpdateAsync(Guid id, FuelLogUpdateDTO dto)
        {
            var fuelLog = await _context.FuelLogs.FindAsync(id);

            if (fuelLog == null)
                return false;

            fuelLog.VehicleId = dto.VehicleId ?? fuelLog.VehicleId;

            fuelLog.Liters = dto.Liters ?? fuelLog.Liters;

            fuelLog.Price = dto.Price ?? fuelLog.Price;

            fuelLog.FuelDate = dto.FuelDate ?? fuelLog.FuelDate;

            fuelLog.Mileage = dto.Mileage ?? fuelLog.Mileage;

            fuelLog.StationName = dto.StationName ?? fuelLog.StationName;

            fuelLog.UpdatedAt = DateTime.Now;

            var result = _context.FuelLogs.Update(fuelLog);

            if (result.State != EntityState.Modified)
                return false;

            var saveCount = await _context.SaveChangesAsync();

            return saveCount > 0;
        }
    }
}
