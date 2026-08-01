using AutoMapper;
using AutoMapper.QueryableExtensions;
using Drivious.Data;
using Drivious.DTOs.Common;
using Drivious.DTOs.FuelLog;
using Drivious.Extensions;
using Drivious.Models;
using Drivious.Responses;
using Drivious.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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

        // Only these fields can be ordered by. Building an expression from an arbitrary
        // caller-supplied name would put user input into the query itself.
        private static readonly IReadOnlyDictionary<string, Expression<Func<FuelLog, object?>>> Sortable =
            new Dictionary<string, Expression<Func<FuelLog, object?>>>(StringComparer.OrdinalIgnoreCase)
            {
                ["fuelDate"] = x => x.FuelDate,
                ["liters"] = x => x.Liters,
                ["price"] = x => x.Price,
                ["mileage"] = x => x.Mileage,
                ["stationName"] = x => x.StationName,
                ["plateNumber"] = x => x.Vehicle.PlateNumber,
                ["createdAt"] = x => x.CreatedAt
            };

        private IQueryable<FuelLog> BuildQuery(FuelLogQueryParameters parameters, bool deleted)
        {
            var search = parameters.Search?.Trim();

            return _context.FuelLogs
                .Where(x => x.IsDeleted == deleted)
                .WhereIf(parameters.VehicleId.HasValue, x => x.VehicleId == parameters.VehicleId!.Value)
                .WhereIf(parameters.From.HasValue, x => x.FuelDate >= parameters.From!.Value)
                .WhereIf(parameters.To.HasValue, x => x.FuelDate <= parameters.To!.Value)
                .WhereIf(!string.IsNullOrWhiteSpace(search),
                    x => x.StationName.Contains(search!)
                      || x.Vehicle.PlateNumber.Contains(search!))
                .ApplySort(parameters, Sortable, "fuelDate");
        }

        public async Task<ApiResponse> CreateAsync(FuelLogCreateDTO dto)
        {
            var referenceError = await _context.ValidateReferencesAsync(dto.VehicleId);

            if (referenceError != null)
            {
                return new ApiResponse(false, referenceError);
            }

            FuelLog fuelLog = _mapper.Map<FuelLog>(dto);

            fuelLog.CreatedAt = DateTime.UtcNow;

            var result = await _context.FuelLogs.AddAsync(fuelLog);

            if (result.State != EntityState.Added)
            {
                return new ApiResponse(
                    false,
                    "Fuel log could not be created."
                );
            }

            // A fuel stop records the odometer, so it is the freshest reading available.
            await _context.AdvanceMileageAsync(fuelLog.VehicleId, fuelLog.Mileage);

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Fuel log could not be saved."
                );
            }

            return new ApiResponse(
                true,
                "Fuel log created successfully."
            );
        }

        public async Task<ApiResponse<PagedResult<FuelLogGetDTO>>> GetAllAsync(FuelLogQueryParameters parameters)
        {
            var page = await BuildQuery(parameters, deleted: false)
                .ToPagedResultAsync<FuelLog, FuelLogGetDTO>(parameters, _mapper.ConfigurationProvider);

            return new ApiResponse<PagedResult<FuelLogGetDTO>>(
                true,
                "Fuel logs retrieved successfully.",
                page
            );
        }

        public async Task<ApiResponse<PagedResult<FuelLogGetDTO>>> GetDeletedAsync(FuelLogQueryParameters parameters)
        {
            var page = await BuildQuery(parameters, deleted: true)
                .ToPagedResultAsync<FuelLog, FuelLogGetDTO>(parameters, _mapper.ConfigurationProvider);

            return new ApiResponse<PagedResult<FuelLogGetDTO>>(
                true,
                "Deleted fuel logs retrieved successfully.",
                page
            );
        }

        public async Task<ApiResponse<FuelLogGetDTO>> GetAsync(Guid id)
        {
            var dto = await _context.FuelLogs
                .Where(x => x.Id == id && !x.IsDeleted)
                .ProjectTo<FuelLogGetDTO>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (dto == null)
            {
                return new ApiResponse<FuelLogGetDTO>(
                    false,
                    "Fuel log not found.",
                    null
                );
            }

            return new ApiResponse<FuelLogGetDTO>(
                true,
                "Fuel log retrieved successfully.",
                dto
            );
        }

        public async Task<ApiResponse> RemoveAsync(Guid id)
        {
            var fuelLog = await _context.FuelLogs.FindAsync(id);

            if (fuelLog == null)
            {
                return new ApiResponse(
                    false,
                    "Fuel log not found."
                );
            }

            var result = _context.FuelLogs.Remove(fuelLog);

            if (result.State != EntityState.Deleted)
            {
                return new ApiResponse(
                    false,
                    "Fuel log could not be deleted."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Fuel log could not be deleted."
                );
            }

            return new ApiResponse(
                true,
                "Fuel log deleted successfully."
            );
        }

        public async Task<ApiResponse> ToggleAsync(Guid id)
        {
            var fuelLog = await _context.FuelLogs.FindAsync(id);

            if (fuelLog == null)
            {
                return new ApiResponse(
                    false,
                    "Fuel log not found."
                );
            }

            fuelLog.IsDeleted = !fuelLog.IsDeleted;
            fuelLog.DeletedAt = fuelLog.IsDeleted ? DateTime.UtcNow : null;

            // Toggling is a deliberate choice, so it never counts as a cascade.
            // Leaving a stale flag here would let a vehicle restore resurrect this row.
            fuelLog.DeletedByCascade = false;

            var result = _context.FuelLogs.Update(fuelLog);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse(
                    false,
                    "Fuel log status could not be changed."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Fuel log status could not be changed."
                );
            }

            return new ApiResponse(
                true,
                "Fuel log status changed successfully."
            );
        }

        public async Task<ApiResponse> UpdateAsync(Guid id, FuelLogUpdateDTO dto)
        {
            var fuelLog = await _context.FuelLogs.FindAsync(id);

            if (fuelLog == null)
            {
                return new ApiResponse(
                    false,
                    "Fuel log not found."
                );
            }

            var referenceError = await _context.ValidateReferencesAsync(dto.VehicleId);

            if (referenceError != null)
            {
                return new ApiResponse(false, referenceError);
            }

            _mapper.Map(dto, fuelLog);

            fuelLog.UpdatedAt = DateTime.UtcNow;

            // Correcting a reading upwards should move the vehicle on, the same way
            // recording it in the first place does.
            await _context.AdvanceMileageAsync(fuelLog.VehicleId, fuelLog.Mileage);

            var result = _context.FuelLogs.Update(fuelLog);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse(
                    false,
                    "Fuel log could not be updated."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Fuel log could not be updated."
                );
            }

            return new ApiResponse(
                true,
                "Fuel log updated successfully."
            );
        }
    }
}
