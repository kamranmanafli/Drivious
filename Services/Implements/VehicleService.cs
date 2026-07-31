using AutoMapper;
using AutoMapper.QueryableExtensions;
using Drivious.Data;
using Drivious.DTOs.Common;
using Drivious.DTOs.Vehicle;
using Drivious.Extensions;
using Drivious.Models;
using Drivious.Responses;
using Drivious.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Drivious.Services.Implements
{
    public class VehicleService : IVehicleService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _accessor;
        private readonly IMapper _mapper;

        public VehicleService(
            AppDbContext context,
            IWebHostEnvironment env,
            IHttpContextAccessor accessor,
            IMapper mapper)
        {
            _context = context;
            _env = env;
            _accessor = accessor;
            _mapper = mapper;
        }

        private string? BuildImageUrl(string? fileName)
        {
            var request = _accessor.HttpContext?.Request;

            if (request == null || string.IsNullOrEmpty(fileName)) return null;

            return $"{request.Scheme}://{request.Host}/Images/Vehicle/{fileName}";
        }

        // Only these fields can be ordered by. Building an expression from an arbitrary
        // caller-supplied name would put user input into the query itself.
        private static readonly IReadOnlyDictionary<string, Expression<Func<Vehicle, object?>>> Sortable =
            new Dictionary<string, Expression<Func<Vehicle, object?>>>(StringComparer.OrdinalIgnoreCase)
            {
                ["brand"] = x => x.Brand,
                ["model"] = x => x.Model,
                ["year"] = x => x.Year,
                ["plateNumber"] = x => x.PlateNumber,
                ["mileage"] = x => x.Mileage,
                ["status"] = x => x.Status,
                ["createdAt"] = x => x.CreatedAt
            };

        private IQueryable<Vehicle> BuildQuery(VehicleQueryParameters parameters, bool deleted)
        {
            var search = parameters.Search?.Trim();

            return _context.Vehicles
                .AsNoTracking()
                .Where(x => x.IsDeleted == deleted)
                .WhereIf(parameters.Status.HasValue, x => x.Status == parameters.Status!.Value)
                .WhereIf(parameters.FuelType.HasValue, x => x.FuelType == parameters.FuelType!.Value)
                .WhereIf(!string.IsNullOrWhiteSpace(parameters.Brand), x => x.Brand == parameters.Brand)
                .WhereIf(parameters.MinYear.HasValue, x => x.Year >= parameters.MinYear!.Value)
                .WhereIf(parameters.MaxYear.HasValue, x => x.Year <= parameters.MaxYear!.Value)
                .WhereIf(!string.IsNullOrWhiteSpace(search),
                    x => x.Brand.Contains(search!)
                      || x.Model.Contains(search!)
                      || x.PlateNumber.Contains(search!)
                      || x.VIN.Contains(search!))
                .ApplySort(parameters, Sortable, "createdAt");
        }

        // The unique indexes on Vehicles are the real guarantee; this check exists so a
        // duplicate returns a readable message instead of a 500 from the database.
        private async Task<string?> FindConflictAsync(string? vin, string? plateNumber, Guid? excludeId = null)
        {
            var query = _context.Vehicles.Where(x => !x.IsDeleted);

            if (excludeId.HasValue)
            {
                query = query.Where(x => x.Id != excludeId.Value);
            }

            if (!string.IsNullOrWhiteSpace(vin) && await query.AnyAsync(x => x.VIN == vin))
            {
                return "A vehicle with this VIN already exists.";
            }

            if (!string.IsNullOrWhiteSpace(plateNumber) && await query.AnyAsync(x => x.PlateNumber == plateNumber))
            {
                return "A vehicle with this plate number already exists.";
            }

            return null;
        }

        public async Task<ApiResponse> CreateAsync(VehicleCreateDTO dto)
        {
            // Checked before the upload is written so a rejected create leaves no orphan file.
            var conflict = await FindConflictAsync(dto.VIN, dto.PlateNumber);

            if (conflict != null)
            {
                return new ApiResponse(false, conflict);
            }

            Vehicle vehicle = _mapper.Map<Vehicle>(dto);

            vehicle.CreatedAt = DateTime.UtcNow;

            vehicle.Image = await dto.Image.CreateFileAsync(_env.WebRootPath, "Images", "Vehicle");

            vehicle.ImageURL = BuildImageUrl(vehicle.Image);

            var result = await _context.Vehicles.AddAsync(vehicle);

            if (result.State != EntityState.Added)
            {
                return new ApiResponse(
                    false,
                    "Vehicle could not be created."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Vehicle could not be saved."
                );
            }

            return new ApiResponse(
                true,
                "Vehicle created successfully."
            );
        }

        public async Task<ApiResponse<PagedResult<VehicleGetDTO>>> GetAllAsync(VehicleQueryParameters parameters)
        {
            var page = await BuildQuery(parameters, deleted: false)
                .ToPagedResultAsync<Vehicle, VehicleGetDTO>(parameters, _mapper.ConfigurationProvider);

            return new ApiResponse<PagedResult<VehicleGetDTO>>(
                true,
                "Vehicles retrieved successfully.",
                page
            );
        }

        public async Task<ApiResponse<PagedResult<VehicleGetDTO>>> GetDeletedAsync(VehicleQueryParameters parameters)
        {
            var page = await BuildQuery(parameters, deleted: true)
                .ToPagedResultAsync<Vehicle, VehicleGetDTO>(parameters, _mapper.ConfigurationProvider);

            return new ApiResponse<PagedResult<VehicleGetDTO>>(
                true,
                "Deleted vehicles retrieved successfully.",
                page
            );
        }

        public async Task<ApiResponse<VehicleGetDTO>> GetAsync(Guid id)
        {
            var vehicle = await _context.Vehicles
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (vehicle == null)
            {
                return new ApiResponse<VehicleGetDTO>(
                    false,
                    "Vehicle not found.",
                    null
                );
            }

            var dto = _mapper.Map<VehicleGetDTO>(vehicle);

            return new ApiResponse<VehicleGetDTO>(
                true,
                "Vehicle retrieved successfully.",
                dto
            );
        }

        public async Task<ApiResponse> RemoveAsync(Guid id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);

            if (vehicle == null)
            {
                return new ApiResponse(
                    false,
                    "Vehicle not found."
                );
            }

            // A hard delete cascades in the database and would silently destroy the
            // vehicle's whole history, so require the soft delete instead.
            var children = await _context.CountVehicleChildrenAsync(id);

            if (children > 0)
            {
                return new ApiResponse(
                    false,
                    $"This vehicle has {children} related record(s) and cannot be permanently deleted. " +
                    "Use the toggle endpoint to archive it instead.");
            }

            vehicle.Image.DeleteFile(_env.WebRootPath, "Images", "Vehicle");

            var result = _context.Vehicles.Remove(vehicle);

            if (result.State != EntityState.Deleted)
            {
                return new ApiResponse(
                    false,
                    "Vehicle could not be deleted."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Vehicle could not be deleted."
                );
            }

            return new ApiResponse(
                true,
                "Vehicle deleted successfully."
            );
        }

        public async Task<ApiResponse> ToggleAsync(Guid id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);

            if (vehicle == null)
            {
                return new ApiResponse(
                    false,
                    "Vehicle not found."
                );
            }

            vehicle.IsDeleted = !vehicle.IsDeleted;
            vehicle.DeletedAt = vehicle.IsDeleted ? DateTime.UtcNow : null;

            // Otherwise a "deleted" vehicle keeps showing its expenses, fuel logs and
            // documents in every list endpoint.
            await _context.CascadeVehicleSoftDeleteAsync(vehicle.Id, vehicle.IsDeleted);

            var result = _context.Vehicles.Update(vehicle);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse(
                    false,
                    "Vehicle status could not be changed."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Vehicle status could not be changed."
                );
            }

            return new ApiResponse(
                true,
                "Vehicle status changed successfully."
            );
        }

        public async Task<ApiResponse> UpdateAsync(Guid id, VehicleUpdateDTO dto)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);

            if (vehicle == null)
            {
                return new ApiResponse(
                    false,
                    "Vehicle not found."
                );
            }

            var conflict = await FindConflictAsync(
                dto.VIN ?? vehicle.VIN,
                dto.PlateNumber ?? vehicle.PlateNumber,
                id);

            if (conflict != null)
            {
                return new ApiResponse(false, conflict);
            }

            _mapper.Map(dto, vehicle);

            if (dto.Image != null)
            {
                if (!string.IsNullOrEmpty(vehicle.Image))
                {
                    vehicle.Image.DeleteFile(_env.WebRootPath, "Images", "Vehicle");
                }

                vehicle.Image = await dto.Image.CreateFileAsync(_env.WebRootPath, "Images", "Vehicle");

                vehicle.ImageURL = BuildImageUrl(vehicle.Image);
            }

            vehicle.UpdatedAt = DateTime.UtcNow;

            var result = _context.Vehicles.Update(vehicle);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse(
                    false,
                    "Vehicle could not be updated."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Vehicle could not be updated."
                );
            }

            return new ApiResponse(
                true,
                "Vehicle updated successfully."
            );
        }
    }
}
