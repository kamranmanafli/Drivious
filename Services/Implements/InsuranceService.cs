using AutoMapper;
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
        private readonly IMapper _mapper;

        public InsuranceService(
            AppDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ApiResponse<object>> CreateAsync(InsuranceCreateDTO dto)
        {
            Insurance insurance = _mapper.Map<Insurance>(dto);

            insurance.CreatedAt = DateTime.Now;

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

            var dtos = _mapper.Map<List<InsuranceGetDTO>>(insurances);

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

            var dto = _mapper.Map<InsuranceGetDTO>(insurance);

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

            _mapper.Map(dto, insurance);

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
