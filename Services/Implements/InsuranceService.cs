using Drivious.Data;
using Drivious.DTOs.Insurance;
using Drivious.Models;
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

        public async Task<bool> CreateAsync(InsuranceCreateDTO dto)
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
                return false;

            var saveCount = await _context.SaveChangesAsync();

            return saveCount > 0;
        }

        public async Task<List<InsuranceGetDTO>> GetAllAsync()
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

            return dtos;
        }

        public async Task<InsuranceGetDTO> GetAsync(Guid id)
        {
            var insurance = await _context.Insurances.FindAsync(id);

            if (insurance == null)
            {
                throw new Exception("Insurance not found!");
            }

            var dto = new InsuranceGetDTO()
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

            return dto;
        }

        public async Task<bool> RemoveAsync(Guid id)
        {
            var insurance = await _context.Insurances.FindAsync(id);

            if (insurance == null)
                return false;

            var result = _context.Insurances.Remove(insurance);

            if (result.State != EntityState.Deleted)
                return false;

            var saveCount = await _context.SaveChangesAsync();

            return saveCount > 0;
        }

        public async Task<bool> ToggleAsync(Guid id)
        {
            var insurance = await _context.Insurances.FindAsync(id);

            if (insurance == null)
                return false;

            insurance.IsDeleted = !insurance.IsDeleted;

            insurance.DeletedAt = insurance.IsDeleted ? DateTime.Now : null;

            var result = _context.Insurances.Update(insurance);

            if (result.State != EntityState.Modified)
                return false;

            var saveCount = await _context.SaveChangesAsync();

            return saveCount > 0;
        }

        public async Task<bool> UpdateAsync(Guid id, InsuranceUpdateDTO dto)
        {
            var insurance = await _context.Insurances.FindAsync(id);

            if (insurance == null)
                return false;

            insurance.VehicleId = dto.VehicleId ?? insurance.VehicleId;

            insurance.CompanyName = dto.CompanyName ?? insurance.CompanyName;

            insurance.PolicyNumber = dto.PolicyNumber ?? insurance.PolicyNumber;

            insurance.StartDate = dto.StartDate ?? insurance.StartDate;

            insurance.EndDate = dto.EndDate ?? insurance.EndDate;

            insurance.Price = dto.Price ?? insurance.Price;

            insurance.UpdatedAt = DateTime.Now;

            var result = _context.Insurances.Update(insurance);

            if (result.State != EntityState.Modified)
                return false;

            var saveCount = await _context.SaveChangesAsync();

            return saveCount > 0;
        }
    }
}
