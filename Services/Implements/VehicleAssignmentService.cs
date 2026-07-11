using Drivious.Data;
using Drivious.DTOs.VehicleAssignment;
using Drivious.Models;
using Drivious.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Drivious.Services.Implements
{
    public class VehicleAssignmentService : IVehicleAssignmentService
    {
        private readonly AppDbContext _context;

        public VehicleAssignmentService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateAsync(VehicleAssignmentCreateDTO dto)
        {
            VehicleAssignment vehicleAssignment = new()
            {
                CreatedAt = DateTime.Now,

                VehicleId = dto.VehicleId,
                DriverId = dto.DriverId,
                AssignedDate = dto.AssignedDate,
                ReturnedDate = dto.ReturnedDate,
                IsActive = dto.IsActive,
                Note = dto.Note,
            };

            var result = await _context.VehicleAssignments.AddAsync(vehicleAssignment);

            if (result.State != EntityState.Added)
                return false;

            var saveCount = await _context.SaveChangesAsync();

            return saveCount > 0;
        }

        public async Task<List<VehicleAssignmentGetDTO>> GetAllAsync()
        {
            var vehicleAssignments = await _context.VehicleAssignments.ToListAsync();

            var dtos = vehicleAssignments.Select(v => new VehicleAssignmentGetDTO
            {
                Id = v.Id,

                VehicleId = v.VehicleId,
                DriverId = v.DriverId,
                AssignedDate = v.AssignedDate,
                ReturnedDate = v.ReturnedDate,
                IsActive = v.IsActive,
                Note = v.Note,

                CreatedAt = v.CreatedAt,
                UpdatedAt = v.UpdatedAt,
                DeletedAt = v.DeletedAt,
                IsDeleted = v.IsDeleted

            }).ToList();

            return dtos;
        }

        public async Task<VehicleAssignmentGetDTO> GetAsync(Guid id)
        {
            var vehicleAssignment = await _context.VehicleAssignments.FindAsync(id);

            if (vehicleAssignment == null)
            {
                throw new Exception("VehicleAssignment not found!");
            }

            var dto = new VehicleAssignmentGetDTO()
            {
                Id = vehicleAssignment.Id,

                VehicleId = vehicleAssignment.VehicleId,
                DriverId = vehicleAssignment.DriverId,
                AssignedDate = vehicleAssignment.AssignedDate,
                ReturnedDate = vehicleAssignment.ReturnedDate,
                IsActive = vehicleAssignment.IsActive,
                Note = vehicleAssignment.Note,

                CreatedAt = vehicleAssignment.CreatedAt,
                UpdatedAt = vehicleAssignment.UpdatedAt,
                DeletedAt = vehicleAssignment.DeletedAt,
                IsDeleted = vehicleAssignment.IsDeleted
            };

            return dto;
        }

        public async Task<bool> RemoveAsync(Guid id)
        {
            var vehicleAssignment = await _context.VehicleAssignments.FindAsync(id);

            if (vehicleAssignment == null)
                return false;

            var result = _context.VehicleAssignments.Remove(vehicleAssignment);

            if (result.State != EntityState.Deleted)
                return false;

            var saveCount = await _context.SaveChangesAsync();

            return saveCount > 0;
        }

        public async Task<bool> ToggleAsync(Guid id)
        {
            var vehicleAssignment = await _context.VehicleAssignments.FindAsync(id);

            if (vehicleAssignment == null)
                return false;

            vehicleAssignment.IsDeleted = !vehicleAssignment.IsDeleted;

            vehicleAssignment.DeletedAt = vehicleAssignment.IsDeleted ? DateTime.Now : null;

            var result = _context.VehicleAssignments.Update(vehicleAssignment);

            if (result.State != EntityState.Modified)
                return false;

            var saveCount = await _context.SaveChangesAsync();

            return saveCount > 0;
        }

        public async Task<bool> UpdateAsync(Guid id, VehicleAssignmentUpdateDTO dto)
        {
            var vehicleAssignment = await _context.VehicleAssignments.FindAsync(id);

            if (vehicleAssignment == null)
                return false;

            vehicleAssignment.VehicleId = dto.VehicleId ?? vehicleAssignment.VehicleId;

            vehicleAssignment.DriverId = dto.DriverId ?? vehicleAssignment.DriverId;

            vehicleAssignment.AssignedDate = dto.AssignedDate ?? vehicleAssignment.AssignedDate;

            vehicleAssignment.ReturnedDate = dto.ReturnedDate ?? vehicleAssignment.ReturnedDate;

            vehicleAssignment.IsActive = dto.IsActive ?? vehicleAssignment.IsActive;

            vehicleAssignment.Note = dto.Note ?? vehicleAssignment.Note;

            vehicleAssignment.UpdatedAt = DateTime.Now;

            var result = _context.VehicleAssignments.Update(vehicleAssignment);

            if (result.State != EntityState.Modified)
                return false;

            var saveCount = await _context.SaveChangesAsync();

            return saveCount > 0;
        }
    }
}
