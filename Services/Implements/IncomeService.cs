using Drivious.Data;
using Drivious.DTOs.Income;
using Drivious.Models;
using Drivious.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Drivious.Services.Implements
{
    public class IncomeService : IIncomeService
    {
        private readonly AppDbContext _context;

        public IncomeService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<bool> CreateAsync(IncomeCreateDTO dto)
        {
            Income income = new()
            {
                CreatedAt = DateTime.Now,

                VehicleId = dto.VehicleId,
                DriverId = dto.DriverId,
                Amount = dto.Amount,
                IncomeDate = dto.IncomeDate,
                Description = dto.Description,
            };

            var result = await _context.Incomes.AddAsync(income);

            if (result.State != EntityState.Added)
                return false;

            var saveCount = await _context.SaveChangesAsync();

            return saveCount > 0;
        }

        public async Task<List<IncomeGetDTO>> GetAllAsync()
        {
            var incomes = await _context.Incomes.ToListAsync();

            var dtos = incomes.Select(i => new IncomeGetDTO
            {
                Id = i.Id,

                VehicleId = i.VehicleId,
                DriverId = i.DriverId,
                Amount = i.Amount,
                IncomeDate = i.IncomeDate,
                Description = i.Description,

                CreatedAt = i.CreatedAt,
                UpdatedAt = i.UpdatedAt,
                DeletedAt = i.DeletedAt,
                IsDeleted = i.IsDeleted

            }).ToList();

            return dtos;
        }

        public async Task<IncomeGetDTO> GetAsync(Guid id)
        {
            var income = await _context.Incomes.FindAsync(id);

            if (income == null)
            {
                throw new Exception("Income not found!");
            }

            var dto = new IncomeGetDTO()
            {
                Id = income.Id,

                VehicleId = income.VehicleId,
                DriverId = income.DriverId,
                Amount = income.Amount,
                IncomeDate = income.IncomeDate,
                Description = income.Description,

                CreatedAt = income.CreatedAt,
                UpdatedAt = income.UpdatedAt,
                DeletedAt = income.DeletedAt,
                IsDeleted = income.IsDeleted
            };

            return dto;
        }

        public async Task<bool> RemoveAsync(Guid id)
        {
            var income = await _context.Incomes.FindAsync(id);

            if (income == null)
                return false;

            var result = _context.Incomes.Remove(income);

            if (result.State != EntityState.Deleted)
                return false;

            var saveCount = await _context.SaveChangesAsync();

            return saveCount > 0;
        }

        public async Task<bool> ToggleAsync(Guid id)
        {
            var income = await _context.Incomes.FindAsync(id);

            if (income == null)
                return false;

            income.IsDeleted = !income.IsDeleted;

            income.DeletedAt = income.IsDeleted ? DateTime.Now : null;

            var result = _context.Incomes.Update(income);

            if (result.State != EntityState.Modified)
                return false;

            var saveCount = await _context.SaveChangesAsync();

            return saveCount > 0;
        }

        public async Task<bool> UpdateAsync(Guid id, IncomeUpdateDTO dto)
        {
            var income = await _context.Incomes.FindAsync(id);

            if (income == null)
                return false;

            income.VehicleId = dto.VehicleId ?? income.VehicleId;

            income.DriverId = dto.DriverId ?? income.DriverId;

            income.Amount = dto.Amount ?? income.Amount;

            income.IncomeDate = dto.IncomeDate ?? income.IncomeDate;

            income.Description = dto.Description ?? income.Description;

            income.UpdatedAt = DateTime.Now;

            var result = _context.Incomes.Update(income);

            if (result.State != EntityState.Modified)
                return false;

            var saveCount = await _context.SaveChangesAsync();

            return saveCount > 0;
        }
    }
}
