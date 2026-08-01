using AutoMapper;
using AutoMapper.QueryableExtensions;
using Drivious.Data;
using Drivious.DTOs.Common;
using Drivious.DTOs.Insurance;
using Drivious.Extensions;
using Drivious.Models;
using Drivious.Responses;
using Drivious.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Drivious.Services.Implements
{
    public class InsuranceService : IInsuranceService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public InsuranceService(
            AppDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Only these fields can be ordered by. Building an expression from an arbitrary
        // caller-supplied name would put user input into the query itself.
        private static readonly IReadOnlyDictionary<string, Expression<Func<Insurance, object?>>> Sortable =
            new Dictionary<string, Expression<Func<Insurance, object?>>>(StringComparer.OrdinalIgnoreCase)
            {
                ["companyName"] = x => x.CompanyName,
                ["policyNumber"] = x => x.PolicyNumber,
                ["startDate"] = x => x.StartDate,
                ["endDate"] = x => x.EndDate,
                ["price"] = x => x.Price,
                ["plateNumber"] = x => x.Vehicle.PlateNumber,
                ["createdAt"] = x => x.CreatedAt
            };

        private IQueryable<Insurance> BuildQuery(InsuranceQueryParameters parameters, bool deleted)
        {
            var search = parameters.Search?.Trim();

            return _context.Insurances
                .Where(x => x.IsDeleted == deleted)
                .WhereIf(parameters.VehicleId.HasValue, x => x.VehicleId == parameters.VehicleId!.Value)
                .WhereIf(parameters.ExpiresBefore.HasValue, x => x.EndDate <= parameters.ExpiresBefore!.Value)
                .WhereIf(parameters.ActiveOn.HasValue,
                    x => x.StartDate <= parameters.ActiveOn!.Value && x.EndDate >= parameters.ActiveOn!.Value)
                .WhereIf(!string.IsNullOrWhiteSpace(search),
                    x => x.CompanyName.Contains(search!)
                      || x.PolicyNumber.Contains(search!)
                      || x.Vehicle.PlateNumber.Contains(search!))
                .ApplySort(parameters, Sortable, "endDate");
        }

        public async Task<ApiResponse> CreateAsync(InsuranceCreateDTO dto)
        {
            var referenceError = await _context.ValidateReferencesAsync(dto.VehicleId);

            if (referenceError != null)
            {
                return new ApiResponse(false, referenceError);
            }

            Insurance insurance = _mapper.Map<Insurance>(dto);

            insurance.CreatedAt = DateTime.UtcNow;

            var result = await _context.Insurances.AddAsync(insurance);

            if (result.State != EntityState.Added)
            {
                return new ApiResponse(
                    false,
                    "Insurance could not be created."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Insurance could not be saved."
                );
            }

            return new ApiResponse(
                true,
                "Insurance created successfully."
            );
        }

        public async Task<ApiResponse<PagedResult<InsuranceGetDTO>>> GetAllAsync(InsuranceQueryParameters parameters)
        {
            var page = await BuildQuery(parameters, deleted: false)
                .ToPagedResultAsync<Insurance, InsuranceGetDTO>(parameters, _mapper.ConfigurationProvider);

            return new ApiResponse<PagedResult<InsuranceGetDTO>>(
                true,
                "Insurances retrieved successfully.",
                page
            );
        }

        public async Task<ApiResponse<PagedResult<InsuranceGetDTO>>> GetDeletedAsync(InsuranceQueryParameters parameters)
        {
            var page = await BuildQuery(parameters, deleted: true)
                .ToPagedResultAsync<Insurance, InsuranceGetDTO>(parameters, _mapper.ConfigurationProvider);

            return new ApiResponse<PagedResult<InsuranceGetDTO>>(
                true,
                "Deleted insurances retrieved successfully.",
                page
            );
        }

        public async Task<ApiResponse<InsuranceGetDTO>> GetAsync(Guid id)
        {
            var dto = await _context.Insurances
                .Where(x => x.Id == id && !x.IsDeleted)
                .ProjectTo<InsuranceGetDTO>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (dto == null)
            {
                return new ApiResponse<InsuranceGetDTO>(
                    false,
                    "Insurance not found.",
                    null
                );
            }

            return new ApiResponse<InsuranceGetDTO>(
                true,
                "Insurance retrieved successfully.",
                dto
            );
        }

        public async Task<ApiResponse> RemoveAsync(Guid id)
        {
            var insurance = await _context.Insurances.FindAsync(id);

            if (insurance == null)
            {
                return new ApiResponse(
                    false,
                    "Insurance not found."
                );
            }

            var result = _context.Insurances.Remove(insurance);

            if (result.State != EntityState.Deleted)
            {
                return new ApiResponse(
                    false,
                    "Insurance could not be deleted."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Insurance could not be deleted."
                );
            }

            return new ApiResponse(
                true,
                "Insurance deleted successfully."
            );
        }

        public async Task<ApiResponse> ToggleAsync(Guid id)
        {
            var insurance = await _context.Insurances.FindAsync(id);

            if (insurance == null)
            {
                return new ApiResponse(
                    false,
                    "Insurance not found."
                );
            }

            insurance.IsDeleted = !insurance.IsDeleted;
            insurance.DeletedAt = insurance.IsDeleted ? DateTime.UtcNow : null;

            // Toggling is a deliberate choice, so it never counts as a cascade.
            // Leaving a stale flag here would let a vehicle restore resurrect this row.
            insurance.DeletedByCascade = false;

            var result = _context.Insurances.Update(insurance);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse(
                    false,
                    "Insurance status could not be changed."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Insurance status could not be changed."
                );
            }

            return new ApiResponse(
                true,
                "Insurance status changed successfully."
            );
        }

        public async Task<ApiResponse> UpdateAsync(Guid id, InsuranceUpdateDTO dto)
        {
            var insurance = await _context.Insurances.FindAsync(id);

            if (insurance == null)
            {
                return new ApiResponse(
                    false,
                    "Insurance not found."
                );
            }

            var referenceError = await _context.ValidateReferencesAsync(dto.VehicleId);

            if (referenceError != null)
            {
                return new ApiResponse(false, referenceError);
            }

            _mapper.Map(dto, insurance);

            insurance.UpdatedAt = DateTime.UtcNow;

            var result = _context.Insurances.Update(insurance);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse(
                    false,
                    "Insurance could not be updated."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Insurance could not be updated."
                );
            }

            return new ApiResponse(
                true,
                "Insurance updated successfully."
            );
        }
    }
}
