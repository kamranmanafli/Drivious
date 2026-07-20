using AutoMapper;
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
        private readonly IMapper _mapper;

        public FuelLogService(
            AppDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ApiResponse<object>> CreateAsync(FuelLogCreateDTO dto)
        {
            FuelLog fuelLog = _mapper.Map<FuelLog>(dto);

            fuelLog.CreatedAt = DateTime.Now;

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

            var dtos = _mapper.Map<List<FuelLogGetDTO>>(fuelLogs);

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

            var dto = _mapper.Map<FuelLogGetDTO>(fuelLog);

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

            _mapper.Map(dto, fuelLog);

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
