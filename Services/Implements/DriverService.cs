using Drivious.Data;
using Drivious.DTOs.Driver;
using Drivious.Extensions;
using Drivious.Models;
using Drivious.Responses;
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

        public async Task<ApiResponse<object>> CreateAsync(DriverCreateDTO dto)
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
            {
                return new ApiResponse<object>(
                    false,
                    "Driver could not be created.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Driver could not be saved.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Driver created successfully.",
                null
            );
        }

        public async Task<ApiResponse<List<DriverGetDTO>>> GetAllAsync()
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

            return new ApiResponse<List<DriverGetDTO>>(
                true,
                "Drivers retrieved successfully.",
                dtos
            );
        }

        public async Task<ApiResponse<DriverGetDTO>> GetAsync(Guid id)
        {
            var driver = await _context.Drivers.FindAsync(id);

            if (driver == null)
            {
                return new ApiResponse<DriverGetDTO>(
                    false,
                    "Driver not found.",
                    null
                );
            }

            var dto = new DriverGetDTO
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

            return new ApiResponse<DriverGetDTO>(
                true,
                "Driver retrieved successfully.",
                dto
            );
        }

        public async Task<ApiResponse<object>> RemoveAsync(Guid id)
        {
            var driver = await _context.Drivers.FindAsync(id);

            if (driver == null)
            {
                return new ApiResponse<object>(
                    false,
                    "Driver not found.",
                    null
                );
            }

            if (!string.IsNullOrEmpty(driver.Image))
            {
                driver.Image.DeleteFile(_env.WebRootPath, "Images", "Driver");
            }

            var result = _context.Drivers.Remove(driver);

            if (result.State != EntityState.Deleted)
            {
                return new ApiResponse<object>(
                    false,
                    "Driver could not be deleted.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Driver could not be deleted.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Driver deleted successfully.",
                null
            );
        }

        public async Task<ApiResponse<object>> ToggleAsync(Guid id)
        {
            var driver = await _context.Drivers.FindAsync(id);

            if (driver == null)
            {
                return new ApiResponse<object>(
                    false,
                    "Driver not found.",
                    null
                );
            }

            driver.IsDeleted = !driver.IsDeleted;

            driver.DeletedAt = driver.IsDeleted ? DateTime.Now : null;

            var result = _context.Drivers.Update(driver);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse<object>(
                    false,
                    "Driver status could not be changed.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Driver status could not be changed.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Driver status changed successfully.",
                null
            );
        }

        public async Task<ApiResponse<object>> UpdateAsync(Guid id, DriverUpdateDTO dto)
        {
            var driver = await _context.Drivers.FindAsync(id);

            if (driver == null)
            {
                return new ApiResponse<object>(
                    false,
                    "Driver not found.",
                    null
                );
            }

            if (dto.Image != null)
            {
                if (!string.IsNullOrEmpty(driver.Image))
                {
                    driver.Image.DeleteFile(_env.WebRootPath, "Images", "Driver");
                }

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
            {
                return new ApiResponse<object>(
                    false,
                    "Driver could not be updated.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Driver could not be updated.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Driver updated successfully.",
                null
            );
        }
    }
}
