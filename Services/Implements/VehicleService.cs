using AutoMapper;
using Drivious.Data;
using Drivious.DTOs.Vehicle;
using Drivious.Extensions;
using Drivious.Models;
using Drivious.Responses;
using Drivious.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Drivious.Services.Implements
{
    public class VehicleService : IVehicleService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _accessor;
        private readonly IMapper _mapper;

        public VehicleService(
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

        public async Task<ApiResponse<object>> CreateAsync(VehicleCreateDTO dto)
        {
            Vehicle vehicle = _mapper.Map<Vehicle>(dto);

            vehicle.CreatedAt = DateTime.Now;

            vehicle.Image = await dto.Image.CreateFileAsync(_env.WebRootPath, "Images", "Vehicle");

            vehicle.ImageURL = $"{_accessor.HttpContext.Request.Scheme}://{_accessor.HttpContext.Request.Host}/Images/Vehicle/{vehicle.Image}";

            var result = await _context.Vehicles.AddAsync(vehicle);

            if (result.State != EntityState.Added)
            {
                return new ApiResponse<object>(
                    false,
                    "Vehicle could not be created.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Vehicle could not be saved.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Vehicle created successfully.",
                null
            );
        }

        public async Task<ApiResponse<List<VehicleGetDTO>>> GetAllAsync()
        {
            var vehicles = await _context.Vehicles.ToListAsync();

            var dtos = _mapper.Map<List<VehicleGetDTO>>(vehicles);

            return new ApiResponse<List<VehicleGetDTO>>(
                true,
                "Vehicles retrieved successfully.",
                dtos
            );
        }

        public async Task<ApiResponse<VehicleGetDTO>> GetAsync(Guid id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);

            if (vehicle == null)
            {
                return new ApiResponse<VehicleGetDTO>(
                    false,
                    "Vehicle not found.",
                    null
                );
            }

            var dto = _mapper.Map<VehicleGetDTO>(vehicle);

            return new ApiResponse<VehicleGetDTO>(
                true,
                "Vehicle retrieved successfully.",
                dto
            );
        }

        public async Task<ApiResponse<object>> RemoveAsync(Guid id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);

            if (vehicle == null)
            {
                return new ApiResponse<object>(
                    false,
                    "Vehicle not found.",
                    null
                );
            }

            vehicle.Image.DeleteFile(_env.WebRootPath, "Images", "Vehicle");

            var result = _context.Vehicles.Remove(vehicle);

            if (result.State != EntityState.Deleted)
            {
                return new ApiResponse<object>(
                    false,
                    "Vehicle could not be deleted.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Vehicle could not be deleted.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Vehicle deleted successfully.",
                null
            );
        }

        public async Task<ApiResponse<object>> ToggleAsync(Guid id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);

            if (vehicle == null)
            {
                return new ApiResponse<object>(
                    false,
                    "Vehicle not found.",
                    null
                );
            }

            vehicle.IsDeleted = !vehicle.IsDeleted;
            vehicle.DeletedAt = vehicle.IsDeleted ? DateTime.Now : null;

            var result = _context.Vehicles.Update(vehicle);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse<object>(
                    false,
                    "Vehicle status could not be changed.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Vehicle status could not be changed.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Vehicle status changed successfully.",
                null
            );
        }

        public async Task<ApiResponse<object>> UpdateAsync(Guid id, VehicleUpdateDTO dto)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);

            if (vehicle == null)
            {
                return new ApiResponse<object>(
                    false,
                    "Vehicle not found.",
                    null
                );
            }

            if (dto.Image != null)
            {
                if (!string.IsNullOrEmpty(vehicle.Image))
                {
                    vehicle.Image.DeleteFile(_env.WebRootPath, "Images", "Vehicle");
                }

                vehicle.Image = await dto.Image.CreateFileAsync(_env.WebRootPath, "Images", "Vehicle");

                vehicle.ImageURL = $"{_accessor.HttpContext.Request.Scheme}://{_accessor.HttpContext.Request.Host}/Images/Vehicle/{vehicle.Image}";
            }

            _mapper.Map(dto, vehicle);

            vehicle.UpdatedAt = DateTime.Now;

            var result = _context.Vehicles.Update(vehicle);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse<object>(
                    false,
                    "Vehicle could not be updated.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Vehicle could not be updated.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Vehicle updated successfully.",
                null
            );
        }
    }
}
