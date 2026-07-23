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

        public async Task<ApiResponse> CreateAsync(DriverCreateDTO dto)
        {
            Driver driver = _mapper.Map<Driver>(dto);

            driver.CreatedAt = DateTime.Now;

            driver.Image = await dto.Image.CreateFileAsync(_env.WebRootPath, "Images", "Driver");

            driver.ImageUrl = $"{_accessor.HttpContext.Request.Scheme}://{_accessor.HttpContext.Request.Host}/Images/Driver/{driver.Image}";

            var result = await _context.Drivers.AddAsync(driver);

            if (result.State != EntityState.Added)
            {
                return new ApiResponse(
                    false,
                    "Driver could not be created."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Driver could not be saved."
                );
            }

            return new ApiResponse(
                true,
                "Driver created successfully."
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

        public async Task<ApiResponse> RemoveAsync(Guid id)
        {
            var driver = await _context.Drivers.FindAsync(id);

            if (driver == null)
            {
                return new ApiResponse(
                    false,
                    "Driver not found."
                );
            }

            if (!string.IsNullOrEmpty(driver.Image))
            {
                driver.Image.DeleteFile(_env.WebRootPath, "Images", "Driver");
            }

            var result = _context.Drivers.Remove(driver);

            if (result.State != EntityState.Deleted)
            {
                return new ApiResponse(
                    false,
                    "Driver could not be deleted."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Driver could not be deleted."
                );
            }

            return new ApiResponse(
                true,
                "Driver deleted successfully."
            );
        }

        public async Task<ApiResponse> ToggleAsync(Guid id)
        {
            var driver = await _context.Drivers.FindAsync(id);

            if (driver == null)
            {
                return new ApiResponse(
                    false,
                    "Driver not found."
                );
            }

            driver.IsDeleted = !driver.IsDeleted;

            driver.DeletedAt = driver.IsDeleted ? DateTime.Now : null;

            var result = _context.Drivers.Update(driver);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse(
                    false,
                    "Driver status could not be changed."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Driver status could not be changed."
                );
            }

            return new ApiResponse(
                true,
                "Driver status changed successfully."
            );
        }

        public async Task<ApiResponse> UpdateAsync(Guid id, DriverUpdateDTO dto)
        {
            var driver = await _context.Drivers.FindAsync(id);

            if (driver == null)
            {
                return new ApiResponse(
                    false,
                    "Driver not found."
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
                return new ApiResponse(
                    false,
                    "Driver could not be updated."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Driver could not be updated."
                );
            }

            return new ApiResponse(
                true,
                "Driver updated successful"
            );
        }
    }
}
