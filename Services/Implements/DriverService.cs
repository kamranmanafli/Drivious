using Drivious.Data;
using Drivious.DTOs.Driver;
using Drivious.Extensions;
using Drivious.Models;
using Drivious.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Drivious.Services.Implements
{
    public class DriverService : IDriverService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _accessor;

        public DriverService(
            AppDbContext context,
            IWebHostEnvironment env,
            IHttpContextAccessor accessor)
        {
            _context = context;
            _env = env;
            _accessor = accessor;
        }

        public async Task<bool> CreateAsync(DriverCreateDTO dto)
        {
            Driver driver = new()
            {
                CreatedAt = DateTime.Now,

                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNumber = dto.PhoneNumber,
                Email = dto.Email,
                IdentityNumber = dto.IdentityNumber,
                DriverLicenseNumber = dto.DriverLicenseNumber,
                LicenseExpireDate = dto.LicenseExpireDate,
                BirthDate = dto.BirthDate,
                HireDate = dto.HireDate,
                Address = dto.Address,
                IsActive = dto.IsActive,

                Image = await dto.Image.CreateFileAsync(_env.WebRootPath, "Images", "Driver"),
            };

            driver.ImageUrl = $"{_accessor.HttpContext.Request.Scheme}://{_accessor.HttpContext.Request.Host}/Images/Driver/{driver.Image}";

            var result = await _context.Drivers.AddAsync(driver);

            if (result.State != EntityState.Added)
                return false;

            var saveCount = await _context.SaveChangesAsync();

            return saveCount > 0;
        }

        public async Task<List<DriverGetDTO>> GetAllAsync()
        {
            var drivers = await _context.Drivers.ToListAsync();

            var dtos = drivers.Select(d => new DriverGetDTO
            {
                Id = d.Id,

                FirstName = d.FirstName,
                LastName = d.LastName,
                PhoneNumber = d.PhoneNumber,
                Email = d.Email,
                IdentityNumber = d.IdentityNumber,
                DriverLicenseNumber = d.DriverLicenseNumber,
                LicenseExpireDate = d.LicenseExpireDate,
                BirthDate = d.BirthDate,
                HireDate = d.HireDate,
                Address = d.Address,
                Image = d.Image,
                ImageUrl = d.ImageUrl,
                IsActive = d.IsActive,

                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt,
                DeletedAt = d.DeletedAt,
                IsDeleted = d.IsDeleted
            }).ToList();

            return dtos;
        }

        public async Task<DriverGetDTO> GetAsync(Guid id)
        {
            var driver = await _context.Drivers.FindAsync(id);

            if (driver == null)
            {
                throw new Exception("Driver not found!");
            }

            var dto = new DriverGetDTO()
            {
                Id = driver.Id,

                FirstName = driver.FirstName,
                LastName = driver.LastName,
                PhoneNumber = driver.PhoneNumber,
                Email = driver.Email,
                IdentityNumber = driver.IdentityNumber,
                DriverLicenseNumber = driver.DriverLicenseNumber,
                LicenseExpireDate = driver.LicenseExpireDate,
                BirthDate = driver.BirthDate,
                HireDate = driver.HireDate,
                Address = driver.Address,

                Image = driver.Image,
                ImageUrl = driver.ImageUrl,

                IsActive = driver.IsActive,

                CreatedAt = driver.CreatedAt,
                UpdatedAt = driver.UpdatedAt,
                DeletedAt = driver.DeletedAt,
                IsDeleted = driver.IsDeleted
            };

            return dto;
        }

        public async Task<bool> RemoveAsync(Guid id)
        {
            var driver = await _context.Drivers.FindAsync(id);

            if (driver == null)
                return false;

            if (!string.IsNullOrEmpty(driver.Image))
            {
                driver.Image.DeleteFile(_env.WebRootPath, "Images", "Driver");
            }

            var result = _context.Drivers.Remove(driver);

            if (result.State != EntityState.Deleted)
                return false;

            var saveCount = await _context.SaveChangesAsync();

            return saveCount > 0;
        }

        public async Task<bool> ToggleAsync(Guid id)
        {
            var driver = await _context.Drivers.FindAsync(id);

            if (driver == null)
                return false;

            driver.IsDeleted = !driver.IsDeleted;

            driver.DeletedAt = driver.IsDeleted ? DateTime.Now : null;

            var result = _context.Drivers.Update(driver);

            if (result.State != EntityState.Modified)
                return false;

            var saveCount = await _context.SaveChangesAsync();

            return saveCount > 0;
        }

        public async Task<bool> UpdateAsync(Guid id, DriverUpdateDTO dto)
        {
            var driver = await _context.Drivers.FindAsync(id);

            if (driver == null)
                return false;

            if (dto.Image != null)
            {
                // Köhnə şəkli silirik
                if (!string.IsNullOrEmpty(driver.Image))
                {
                    driver.Image.DeleteFile(_env.WebRootPath, "Images", "Driver");
                }

                // Yeni şəkli əlavə edirik
                driver.Image = await dto.Image.CreateFileAsync(_env.WebRootPath, "Images", "Driver");

                driver.ImageUrl = $"{_accessor.HttpContext.Request.Scheme}://{_accessor.HttpContext.Request.Host}/Images/Driver/{driver.Image}";
            }

            driver.FirstName = dto.FirstName ?? driver.FirstName;

            driver.LastName = dto.LastName ?? driver.LastName;

            driver.PhoneNumber = dto.PhoneNumber ?? driver.PhoneNumber;

            driver.Email = dto.Email ?? driver.Email;

            driver.IdentityNumber = dto.IdentityNumber ?? driver.IdentityNumber;

            driver.DriverLicenseNumber = dto.DriverLicenseNumber ?? driver.DriverLicenseNumber;

            driver.LicenseExpireDate = dto.LicenseExpireDate ?? driver.LicenseExpireDate;

            driver.BirthDate = dto.BirthDate ?? driver.BirthDate;

            driver.HireDate = dto.HireDate ?? driver.HireDate;

            driver.Address = dto.Address ?? driver.Address;

            driver.IsActive = dto.IsActive ?? driver.IsActive;

            driver.UpdatedAt = DateTime.Now;

            var result = _context.Drivers.Update(driver);

            if (result.State != EntityState.Modified)
                return false;

            var saveCount = await _context.SaveChangesAsync();

            return saveCount > 0;
        }
    }
}
