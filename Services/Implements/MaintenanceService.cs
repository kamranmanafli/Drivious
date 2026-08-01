using AutoMapper;
using AutoMapper.QueryableExtensions;
using Drivious.Data;
using Drivious.DTOs.Common;
using Drivious.DTOs.Maintenance;
using Drivious.Extensions;
using Drivious.Models;
using Drivious.Responses;
using Drivious.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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

        // Only these fields can be ordered by. Building an expression from an arbitrary
        // caller-supplied name would put user input into the query itself.
        private static readonly IReadOnlyDictionary<string, Expression<Func<Maintenance, object?>>> Sortable =
            new Dictionary<string, Expression<Func<Maintenance, object?>>>(StringComparer.OrdinalIgnoreCase)
            {
                ["serviceType"] = x => x.ServiceType,
                ["cost"] = x => x.Cost,
                ["maintenanceDate"] = x => x.MaintenanceDate,
                ["nextMaintenanceDate"] = x => x.NextMaintenanceDate,
                ["mileage"] = x => x.Mileage,
                ["serviceCenter"] = x => x.ServiceCenter,
                ["plateNumber"] = x => x.Vehicle.PlateNumber,
                ["createdAt"] = x => x.CreatedAt
            };

        private IQueryable<Maintenance> BuildQuery(MaintenanceQueryParameters parameters, bool deleted)
        {
            var search = parameters.Search?.Trim();

            return _context.Maintenances
                .Where(x => x.IsDeleted == deleted)
                .WhereIf(parameters.VehicleId.HasValue, x => x.VehicleId == parameters.VehicleId!.Value)
                .WhereIf(parameters.ServiceType.HasValue, x => x.ServiceType == parameters.ServiceType!.Value)
                .WhereIf(parameters.From.HasValue, x => x.MaintenanceDate >= parameters.From!.Value)
                .WhereIf(parameters.To.HasValue, x => x.MaintenanceDate <= parameters.To!.Value)
                .WhereIf(parameters.DueBefore.HasValue,
                    x => x.NextMaintenanceDate != null && x.NextMaintenanceDate <= parameters.DueBefore!.Value)
                .WhereIf(!string.IsNullOrWhiteSpace(search),
                    x => x.ServiceCenter.Contains(search!)
                      || (x.Description != null && x.Description.Contains(search!))
                      || x.Vehicle.PlateNumber.Contains(search!))
                .ApplySort(parameters, Sortable, "maintenanceDate");
        }

        public async Task<ApiResponse> CreateAsync(MaintenanceCreateDTO dto)
        {
            var referenceError = await _context.ValidateReferencesAsync(dto.VehicleId);

            if (referenceError != null)
            {
                return new ApiResponse(false, referenceError);
            }

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

            // A service visit reads the odometer, so it can move the vehicle forward.
            await _context.AdvanceMileageAsync(maintenance.VehicleId, maintenance.Mileage);

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

        public async Task<ApiResponse<PagedResult<MaintenanceGetDTO>>> GetAllAsync(MaintenanceQueryParameters parameters)
        {
            var page = await BuildQuery(parameters, deleted: false)
                .ToPagedResultAsync<Maintenance, MaintenanceGetDTO>(parameters, _mapper.ConfigurationProvider);

            return new ApiResponse<PagedResult<MaintenanceGetDTO>>(
                true,
                "Maintenances retrieved successfully.",
                page
            );
        }

        public async Task<ApiResponse<PagedResult<MaintenanceGetDTO>>> GetDeletedAsync(MaintenanceQueryParameters parameters)
        {
            var page = await BuildQuery(parameters, deleted: true)
                .ToPagedResultAsync<Maintenance, MaintenanceGetDTO>(parameters, _mapper.ConfigurationProvider);

            return new ApiResponse<PagedResult<MaintenanceGetDTO>>(
                true,
                "Deleted maintenances retrieved successfully.",
                page
            );
        }

        public async Task<ApiResponse<MaintenanceGetDTO>> GetAsync(Guid id)
        {
            var dto = await _context.Maintenances
                .Where(x => x.Id == id && !x.IsDeleted)
                .ProjectTo<MaintenanceGetDTO>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (dto == null)
            {
                return new ApiResponse<MaintenanceGetDTO>(
                    false,
                    "Maintenance not found.",
                    null
                );
            }

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

            // Toggling is a deliberate choice, so it never counts as a cascade.
            // Leaving a stale flag here would let a vehicle restore resurrect this row.
            maintenance.DeletedByCascade = false;

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

            var referenceError = await _context.ValidateReferencesAsync(dto.VehicleId);

            if (referenceError != null)
            {
                return new ApiResponse(false, referenceError);
            }

            _mapper.Map(dto, maintenance);

            maintenance.UpdatedAt = DateTime.UtcNow;

            // Correcting a reading upwards should move the vehicle on, the same way
            // recording it in the first place does.
            await _context.AdvanceMileageAsync(maintenance.VehicleId, maintenance.Mileage);

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
