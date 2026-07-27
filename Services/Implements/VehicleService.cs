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

        public async Task<ApiResponse> CreateAsync(VehicleCreateDTO dto)
        {
            Vehicle vehicle = _mapper.Map<Vehicle>(dto);

            vehicle.CreatedAt = DateTime.Now;

            vehicle.Image = await dto.Image.CreateFileAsync(_env.WebRootPath, "Images", "Vehicle");

            vehicle.ImageURL = $"{_accessor.HttpContext.Request.Scheme}://{_accessor.HttpContext.Request.Host}/Images/Vehicle/{vehicle.Image}";

            var result = await _context.Vehicles.AddAsync(vehicle);

            if (result.State != EntityState.Added)
            {
                return new ApiResponse(
                    false,
                    "Vehicle could not be created."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Vehicle could not be saved."
                );
            }

            return new ApiResponse(
                true,
                "Vehicle created successfully."
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

        public async Task<ApiResponse> RemoveAsync(Guid id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);

            if (vehicle == null)
            {
                return new ApiResponse(
                    false,
                    "Vehicle not found."
                );
            }

            vehicle.Image.DeleteFile(_env.WebRootPath, "Images", "Vehicle");

            var result = _context.Vehicles.Remove(vehicle);

            if (result.State != EntityState.Deleted)
            {
                return new ApiResponse(
                    false,
                    "Vehicle could not be deleted."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Vehicle could not be deleted."
                );
            }

            return new ApiResponse(
                true,
                "Vehicle deleted successfully."
            );
        }

        public async Task<ApiResponse> ToggleAsync(Guid id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);

            if (vehicle == null)
            {
                return new ApiResponse(
                    false,
                    "Vehicle not found."
                );
            }

            vehicle.IsDeleted = !vehicle.IsDeleted;
            vehicle.DeletedAt = vehicle.IsDeleted ? DateTime.Now : null;

            var result = _context.Vehicles.Update(vehicle);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse(
                    false,
                    "Vehicle status could not be changed."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Vehicle status could not be changed."
                );
            }

            return new ApiResponse(
                true,
                "Vehicle status changed successfully."
            );
        }

        public async Task<ApiResponse> UpdateAsync(Guid id, VehicleUpdateDTO dto)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);

            if (vehicle == null)
            {
                return new ApiResponse(
                    false,
                    "Vehicle not found."
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
                return new ApiResponse(
                    false,
                    "Vehicle could not be updated."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Vehicle could not be updated."
                );
            }

            return new ApiResponse(
                true,
                "Vehicle updated successfully."
            );
        }
    }
}
