using Drivious.Data;
using Drivious.DTOs.Vehicle;
using Drivious.Extensions;
using Drivious.Models;
using Drivious.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Drivious.Services.Implements
{
    public class VehicleService : IVehicleService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _accessor;

        public VehicleService(
            AppDbContext context,
            IWebHostEnvironment env,
            IHttpContextAccessor accessor)
        {
            _context = context;
            _env = env;
            _accessor = accessor;
        }

        public async Task<bool> CreateAsync(VehicleCreateDTO dto)
        {
            Vehicle vehicle = new()
            {
                CreatedAt = DateTime.Now,

                Brand = dto.Brand,
                Model = dto.Model,
                Year = dto.Year,
                PlateNumber = dto.PlateNumber,
                VIN = dto.VIN,
                Color = dto.Color,
                FuelType = dto.FuelType,
                Mileage = dto.Mileage,
                Status = dto.Status,

                Image = await dto.Image.CreateFileAsync(_env.WebRootPath, "Images", "Vehicle"),
            };

            vehicle.ImageURL = $"{_accessor.HttpContext.Request.Scheme}://{_accessor.HttpContext.Request.Host}/Images/Vehicle/{vehicle.Image}";

            var result = await _context.AddAsync(vehicle);

            if (result.State != EntityState.Added)
                return false;

            var saveCount = await _context.SaveChangesAsync();

            return saveCount > 0;
        }

        public async Task<List<VehicleGetDTO>> GetAllAsync()
        {
            var vehicles = await _context.Vehicles.ToListAsync();

            var dtos = vehicles.Select(v => new VehicleGetDTO
            {
                Id = v.Id,

                Brand = v.Brand,
                Model = v.Model,
                Year = v.Year,
                PlateNumber = v.PlateNumber,
                VIN = v.VIN,
                Color = v.Color,
                FuelType = v.FuelType,
                Mileage = v.Mileage,
                Status = v.Status,

                Image = v.Image,
                ImageURL = v.ImageURL,

                CreatedAt = v.CreatedAt,
                UpdatedAt = v.UpdatedAt,
                DeletedAt = v.DeletedAt,
                IsDeleted = v.IsDeleted
            }).ToList();

            return dtos;
        }

        public async Task<VehicleGetDTO> GetAsync(Guid id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);

            if (vehicle == null)
            {
                throw new Exception("Vehicle not found!");
            }

            var dto = new VehicleGetDTO()
            {
                Id = vehicle.Id,

                Brand = vehicle.Brand,
                Model = vehicle.Model,
                Year = vehicle.Year,
                PlateNumber = vehicle.PlateNumber,
                VIN = vehicle.VIN,
                Color = vehicle.Color,
                FuelType = vehicle.FuelType,
                Mileage = vehicle.Mileage,
                Status = vehicle.Status,

                Image = vehicle.Image,
                ImageURL = vehicle.ImageURL,

                CreatedAt = vehicle.CreatedAt,
                UpdatedAt = vehicle.UpdatedAt,
                DeletedAt = vehicle.DeletedAt,
                IsDeleted = vehicle.IsDeleted
            };

            return dto;
        }

        public async Task<bool> RemoveAsync(Guid id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);

            if (vehicle == null)
                return false;

            vehicle.Image.DeleteFile(_env.WebRootPath, "Images", "Vehicle");

            var result = _context.Vehicles.Remove(vehicle);

            if (result.State != EntityState.Deleted)
                return false;

            var saveCount = await _context.SaveChangesAsync();

            return saveCount > 0;
        }

        public async Task<bool> ToggleAsync(Guid id)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);

            if (vehicle == null)
                return false;

            vehicle.IsDeleted = !vehicle.IsDeleted;

            vehicle.DeletedAt = vehicle.IsDeleted ? DateTime.Now : null;

            var result = _context.Update(vehicle);

            if (result.State != EntityState.Modified)
                return false;

            var saveCount = await _context.SaveChangesAsync();

            return saveCount > 0;
        }

        public async Task<bool> UpdateAsync(Guid id, VehicleUpdateDTO dto)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);

            if (vehicle == null)
                return false;

            if (dto.Image != null)
            {
                // Köhnə şəkli silirik
                if (!string.IsNullOrEmpty(vehicle.Image))
                {
                    vehicle.Image.DeleteFile(_env.WebRootPath, "Images", "Vehicle");
                }

                // Yeni şəkli əlavə edirik
                vehicle.Image = await dto.Image.CreateFileAsync(_env.WebRootPath, "Images", "Vehicle");

                vehicle.ImageURL = $"{_accessor.HttpContext.Request.Scheme}://{_accessor.HttpContext.Request.Host}/Images/Vehicle/{vehicle.Image}";
            }

            vehicle.Brand = dto.Brand ?? vehicle.Brand;

            vehicle.Model = dto.Model ?? vehicle.Model;

            vehicle.Year = dto.Year ?? vehicle.Year;

            vehicle.PlateNumber = dto.PlateNumber ?? vehicle.PlateNumber;

            vehicle.VIN = dto.VIN ?? vehicle.VIN;

            vehicle.Color = dto.Color ?? vehicle.Color;

            vehicle.FuelType = dto.FuelType ?? vehicle.FuelType;

            vehicle.Mileage = dto.Mileage ?? vehicle.Mileage;

            vehicle.Status = dto.Status ?? vehicle.Status;

            vehicle.UpdatedAt = DateTime.Now;

            var result = _context.Update(vehicle);

            if (result.State != EntityState.Modified)
                return false;

            var saveCount = await _context.SaveChangesAsync();

            return saveCount > 0;
        }
    }
}
