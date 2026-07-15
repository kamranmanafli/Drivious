using Drivious.Data;
using Drivious.DTOs.VehicleAssignment;
using Drivious.Models;
using Drivious.Responses;
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

        public async Task<ApiResponse<object>> CreateAsync(VehicleAssignmentCreateDTO dto)
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
            {
                return new ApiResponse<object>(
                    false,
                    "Vehicle assignment could not be created.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Vehicle assignment could not be saved.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Vehicle assignment created successfully.",
                null
            );
        }

        public async Task<ApiResponse<List<VehicleAssignmentGetDTO>>> GetAllAsync()
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

            return new ApiResponse<List<VehicleAssignmentGetDTO>>(
                true,
                "Vehicle assignments retrieved successfully.",
                dtos
            );
        }

        public async Task<ApiResponse<VehicleAssignmentGetDTO>> GetAsync(Guid id)
        {
            var vehicleAssignment = await _context.VehicleAssignments.FindAsync(id);

            if (vehicleAssignment == null)
            {
                return new ApiResponse<VehicleAssignmentGetDTO>(
                    false,
                    "Vehicle assignment not found.",
                    null
                );
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

            return new ApiResponse<VehicleAssignmentGetDTO>(
                true,
                "Vehicle assignment retrieved successfully.",
                dto
            );
        }

        public async Task<ApiResponse<object>> RemoveAsync(Guid id)
        {
            var vehicleAssignment = await _context.VehicleAssignments.FindAsync(id);

            if (vehicleAssignment == null)
            {
                return new ApiResponse<object>(
                    false,
                    "Vehicle assignment not found.",
                    null
                );
            }

            var result = _context.VehicleAssignments.Remove(vehicleAssignment);

            if (result.State != EntityState.Deleted)
            {
                return new ApiResponse<object>(
                    false,
                    "Vehicle assignment could not be deleted.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Vehicle assignment could not be deleted.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Vehicle assignment deleted successfully.",
                null
            );
        }

        public async Task<ApiResponse<object>> ToggleAsync(Guid id)
        {
            var vehicleAssignment = await _context.VehicleAssignments.FindAsync(id);

            if (vehicleAssignment == null)
            {
                return new ApiResponse<object>(
                    false,
                    "Vehicle assignment not found.",
                    null
                );
            }

            vehicleAssignment.IsDeleted = !vehicleAssignment.IsDeleted;
            vehicleAssignment.DeletedAt = vehicleAssignment.IsDeleted ? DateTime.Now : null;

            var result = _context.VehicleAssignments.Update(vehicleAssignment);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse<object>(
                    false,
                    "Vehicle assignment status could not be changed.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Vehicle assignment status could not be changed.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Vehicle assignment status changed successfully.",
                null
            );
        }

        public async Task<ApiResponse<object>> UpdateAsync(Guid id, VehicleAssignmentUpdateDTO dto)
        {
            var vehicleAssignment = await _context.VehicleAssignments.FindAsync(id);

            if (vehicleAssignment == null)
            {
                return new ApiResponse<object>(
                    false,
                    "Vehicle assignment not found.",
                    null
                );
            }

            vehicleAssignment.VehicleId = dto.VehicleId ?? vehicleAssignment.VehicleId;
            vehicleAssignment.DriverId = dto.DriverId ?? vehicleAssignment.DriverId;
            vehicleAssignment.AssignedDate = dto.AssignedDate ?? vehicleAssignment.AssignedDate;
            vehicleAssignment.ReturnedDate = dto.ReturnedDate ?? vehicleAssignment.ReturnedDate;
            vehicleAssignment.IsActive = dto.IsActive ?? vehicleAssignment.IsActive;
            vehicleAssignment.Note = dto.Note ?? vehicleAssignment.Note;

            vehicleAssignment.UpdatedAt = DateTime.Now;

            var result = _context.VehicleAssignments.Update(vehicleAssignment);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse<object>(
                    false,
                    "Vehicle assignment could not be updated.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Vehicle assignment could not be updated.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Vehicle assignment updated successfully.",
                null
            );
        }
    }
}
