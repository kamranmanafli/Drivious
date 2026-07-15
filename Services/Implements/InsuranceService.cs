using Drivious.Data;
using Drivious.DTOs.Insurance;
using Drivious.Models;
using Drivious.Responses;
using Drivious.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Drivious.Services.Implements
{
    public class InsuranceService : IInsuranceService
    {
        private readonly AppDbContext _context;

        public InsuranceService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<object>> CreateAsync(InsuranceCreateDTO dto)
        {
            Insurance insurance = new()
            {
                CreatedAt = DateTime.Now,

                VehicleId = dto.VehicleId,
                CompanyName = dto.CompanyName,
                PolicyNumber = dto.PolicyNumber,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Price = dto.Price,
            };

            var result = await _context.Insurances.AddAsync(insurance);

            if (result.State != EntityState.Added)
            {
                return new ApiResponse<object>(
                    false,
                    "Insurance could not be created.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Insurance could not be saved.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Insurance created successfully.",
                null
            );
        }

        public async Task<ApiResponse<List<InsuranceGetDTO>>> GetAllAsync()
        {
            var insurances = await _context.Insurances.ToListAsync();

            var dtos = insurances.Select(i => new InsuranceGetDTO
            {
                Id = i.Id,

                VehicleId = i.VehicleId,
                CompanyName = i.CompanyName,
                PolicyNumber = i.PolicyNumber,
                StartDate = i.StartDate,
                EndDate = i.EndDate,
                Price = i.Price,

                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt,
                DeletedAt = i.DeletedAt,
                IsDeleted = i.IsDeleted

            }).ToList();

            return new ApiResponse<List<InsuranceGetDTO>>(
                true,
                "Insurances retrieved successfully.",
                dtos
            );
        }

        public async Task<ApiResponse<InsuranceGetDTO>> GetAsync(Guid id)
        {
            var insurance = await _context.Insurances.FindAsync(id);

            if (insurance == null)
            {
                return new ApiResponse<InsuranceGetDTO>(
                    false,
                    "Insurance not found.",
                    null
                );
            }

            var dto = new InsuranceGetDTO
            {
                Id = insurance.Id,

                VehicleId = insurance.VehicleId,
                CompanyName = insurance.CompanyName,
                PolicyNumber = insurance.PolicyNumber,
                StartDate = insurance.StartDate,
                EndDate = insurance.EndDate,
                Price = insurance.Price,

                CreatedAt = insurance.CreatedAt,
                UpdatedAt = insurance.UpdatedAt,
                DeletedAt = insurance.DeletedAt,
                IsDeleted = insurance.IsDeleted
            };

            return new ApiResponse<InsuranceGetDTO>(
                true,
                "Insurance retrieved successfully.",
                dto
            );
        }

        public async Task<ApiResponse<object>> RemoveAsync(Guid id)
        {
            var insurance = await _context.Insurances.FindAsync(id);

            if (insurance == null)
            {
                return new ApiResponse<object>(
                    false,
                    "Insurance not found.",
                    null
                );
            }

            var result = _context.Insurances.Remove(insurance);

            if (result.State != EntityState.Deleted)
            {
                return new ApiResponse<object>(
                    false,
                    "Insurance could not be deleted.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Insurance could not be deleted.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Insurance deleted successfully.",
                null
            );
        }

        public async Task<ApiResponse<object>> ToggleAsync(Guid id)
        {
            var insurance = await _context.Insurances.FindAsync(id);

            if (insurance == null)
            {
                return new ApiResponse<object>(
                    false,
                    "Insurance not found.",
                    null
                );
            }

            insurance.IsDeleted = !insurance.IsDeleted;
            insurance.DeletedAt = insurance.IsDeleted ? DateTime.Now : null;

            var result = _context.Insurances.Update(insurance);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse<object>(
                    false,
                    "Insurance status could not be changed.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Insurance status could not be changed.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Insurance status changed successfully.",
                null
            );
        }

        public async Task<ApiResponse<object>> UpdateAsync(Guid id, InsuranceUpdateDTO dto)
        {
            var insurance = await _context.Insurances.FindAsync(id);

            if (insurance == null)
            {
                return new ApiResponse<object>(
                    false,
                    "Insurance not found.",
                    null
                );
            }

            insurance.VehicleId = dto.VehicleId ?? insurance.VehicleId;
            insurance.CompanyName = dto.CompanyName ?? insurance.CompanyName;
            insurance.PolicyNumber = dto.PolicyNumber ?? insurance.PolicyNumber;
            insurance.StartDate = dto.StartDate ?? insurance.StartDate;
            insurance.EndDate = dto.EndDate ?? insurance.EndDate;
            insurance.Price = dto.Price ?? insurance.Price;

            insurance.UpdatedAt = DateTime.Now;

            var result = _context.Insurances.Update(insurance);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse<object>(
                    false,
                    "Insurance could not be updated.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Insurance could not be updated.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Insurance updated successfully.",
                null
            );
        }
    }
}
