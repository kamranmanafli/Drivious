using AutoMapper;
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
        private readonly IMapper _mapper;

        public DriverService(
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

        public async Task<ApiResponse<object>> CreateAsync(DriverCreateDTO dto)
        {
            Driver driver = _mapper.Map<Driver>(dto);

            driver.CreatedAt = DateTime.Now;

            driver.Image = await dto.Image.CreateFileAsync(_env.WebRootPath, "Images", "Driver");

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

            var dtos = _mapper.Map<List<DriverGetDTO>>(drivers);

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

            var dto = _mapper.Map<DriverGetDTO>(driver);

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

            _mapper.Map(dto, driver);

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
