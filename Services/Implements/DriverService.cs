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

        private string? BuildImageUrl(string? fileName)
        {
            var request = _accessor.HttpContext?.Request;

            if (request == null || string.IsNullOrEmpty(fileName)) return null;

            return $"{request.Scheme}://{request.Host}/Images/Driver/{fileName}";
        }

        public async Task<ApiResponse> CreateAsync(DriverCreateDTO dto)
        {
            Driver driver = _mapper.Map<Driver>(dto);

            driver.CreatedAt = DateTime.UtcNow;

            driver.Image = await dto.Image.CreateFileAsync(_env.WebRootPath, "Images", "Driver");

            driver.ImageUrl = BuildImageUrl(driver.Image);

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
            var drivers = await _context.Drivers
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToListAsync();

            var dtos = _mapper.Map<List<DriverGetDTO>>(drivers);

            return new ApiResponse<List<DriverGetDTO>>(
                true,
                "Drivers retrieved successfully.",
                dtos
            );
        }

        public async Task<ApiResponse<List<DriverGetDTO>>> GetDeletedAsync()
        {
            var drivers = await _context.Drivers
                .AsNoTracking()
                .Where(x => x.IsDeleted)
                .ToListAsync();

            var dtos = _mapper.Map<List<DriverGetDTO>>(drivers);

            return new ApiResponse<List<DriverGetDTO>>(
                true,
                "Deleted drivers retrieved successfully.",
                dtos
            );
        }

        public async Task<ApiResponse<DriverGetDTO>> GetAsync(Guid id)
        {
            var driver = await _context.Drivers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

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

            // A hard delete cascades in the database and would silently destroy the
            // driver's incomes and assignment history.
            var children = await _context.CountDriverChildrenAsync(id);

            if (children > 0)
            {
                return new ApiResponse(
                    false,
                    $"This driver has {children} related record(s) and cannot be permanently deleted. " +
                    "Use the toggle endpoint to archive them instead.");
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

            driver.DeletedAt = driver.IsDeleted ? DateTime.UtcNow : null;

            // Archiving a driver has to take their incomes and assignments with it,
            // otherwise those rows keep appearing against a driver who is gone.
            await _context.CascadeDriverSoftDeleteAsync(driver.Id, driver.IsDeleted);

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

            _mapper.Map(dto, driver);

            if (dto.Image != null)
            {
                if (!string.IsNullOrEmpty(driver.Image))
                {
                    driver.Image.DeleteFile(_env.WebRootPath, "Images", "Driver");
                }

                driver.Image = await dto.Image.CreateFileAsync(_env.WebRootPath, "Images", "Driver");

                driver.ImageUrl = BuildImageUrl(driver.Image);
            }

            driver.UpdatedAt = DateTime.UtcNow;

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
                "Driver updated successfully."
            );
        }
    }
}
