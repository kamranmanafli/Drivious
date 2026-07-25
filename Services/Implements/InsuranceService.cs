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

        public async Task<ApiResponse> CreateAsync(InsuranceCreateDTO dto)
        {
            Insurance insurance = _mapper.Map<Insurance>(dto);

            insurance.CreatedAt = DateTime.Now;

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
            insurance.DeletedAt = insurance.IsDeleted ? DateTime.Now : null;

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

            _mapper.Map(dto, insurance);

            insurance.UpdatedAt = DateTime.Now;

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
