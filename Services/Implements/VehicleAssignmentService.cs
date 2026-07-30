using AutoMapper;
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
        private readonly IMapper _mapper;

        public VehicleAssignmentService(
            AppDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        /// <summary>
        /// A vehicle can only be out with one driver at a time, and a driver can only
        /// hold one vehicle at a time. Returns the message to send back, or null when
        /// the handover is allowed.
        /// </summary>
        private async Task<string?> FindActiveConflictAsync(
            Guid? vehicleId,
            Guid? driverId,
            Guid? excludeId = null)
        {
            var open = _context.VehicleAssignments
                .Where(x => !x.IsDeleted && x.IsActive && x.ReturnedDate == null);

            if (excludeId.HasValue)
            {
                open = open.Where(x => x.Id != excludeId.Value);
            }

            if (vehicleId.HasValue && await open.AnyAsync(x => x.VehicleId == vehicleId.Value))
            {
                return "This vehicle is already assigned to a driver. Return it first.";
            }

            if (driverId.HasValue && await open.AnyAsync(x => x.DriverId == driverId.Value))
            {
                return "This driver already holds a vehicle. Return it first.";
            }

            return null;
        }

        public async Task<ApiResponse> CreateAsync(VehicleAssignmentCreateDTO dto)
        {
            var referenceError = await _context.ValidateReferencesAsync(dto.VehicleId, dto.DriverId);

            if (referenceError != null)
            {
                return new ApiResponse(false, referenceError);
            }

            var conflict = await FindActiveConflictAsync(dto.VehicleId, dto.DriverId);

            if (conflict != null)
            {
                return new ApiResponse(false, conflict);
            }

            VehicleAssignment vehicleAssignment = _mapper.Map<VehicleAssignment>(dto);

            vehicleAssignment.CreatedAt = DateTime.UtcNow;

            // An assignment that already carries a return date is history, not a
            // current handover.
            vehicleAssignment.IsActive = vehicleAssignment.ReturnedDate == null;

            var result = await _context.VehicleAssignments.AddAsync(vehicleAssignment);

            if (result.State != EntityState.Added)
            {
                return new ApiResponse(
                    false,
                    "Vehicle assignment could not be created."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Vehicle assignment could not be saved."
                );
            }

            return new ApiResponse(
                true,
                "Vehicle assignment created successfully."
            );
        }

        public async Task<ApiResponse<List<VehicleAssignmentGetDTO>>> GetAllAsync()
        {
            var vehicleAssignments = await _context.VehicleAssignments
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToListAsync();

            var dtos = _mapper.Map<List<VehicleAssignmentGetDTO>>(vehicleAssignments);

            return new ApiResponse<List<VehicleAssignmentGetDTO>>(
                true,
                "Vehicle assignments retrieved successfully.",
                dtos
            );
        }

        public async Task<ApiResponse<List<VehicleAssignmentGetDTO>>> GetDeletedAsync()
        {
            var vehicleAssignments = await _context.VehicleAssignments
                .AsNoTracking()
                .Where(x => x.IsDeleted)
                .ToListAsync();

            var dtos = _mapper.Map<List<VehicleAssignmentGetDTO>>(vehicleAssignments);

            return new ApiResponse<List<VehicleAssignmentGetDTO>>(
                true,
                "Deleted vehicle assignments retrieved successfully.",
                dtos
            );
        }

        public async Task<ApiResponse<VehicleAssignmentGetDTO>> GetAsync(Guid id)
        {
            var vehicleAssignment = await _context.VehicleAssignments
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (vehicleAssignment == null)
            {
                return new ApiResponse<VehicleAssignmentGetDTO>(
                    false,
                    "Vehicle assignment not found.",
                    null
                );
            }

            var dto = _mapper.Map<VehicleAssignmentGetDTO>(vehicleAssignment);

            return new ApiResponse<VehicleAssignmentGetDTO>(
                true,
                "Vehicle assignment retrieved successfully.",
                dto
            );
        }

        public async Task<ApiResponse> RemoveAsync(Guid id)
        {
            var vehicleAssignment = await _context.VehicleAssignments.FindAsync(id);

            if (vehicleAssignment == null)
            {
                return new ApiResponse(
                    false,
                    "Vehicle assignment not found."
                );
            }

            var result = _context.VehicleAssignments.Remove(vehicleAssignment);

            if (result.State != EntityState.Deleted)
            {
                return new ApiResponse(
                    false,
                    "Vehicle assignment could not be deleted."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Vehicle assignment could not be deleted."
                );
            }

            return new ApiResponse(
                true,
                "Vehicle assignment deleted successfully."
            );
        }

        public async Task<ApiResponse> ToggleAsync(Guid id)
        {
            var vehicleAssignment = await _context.VehicleAssignments.FindAsync(id);

            if (vehicleAssignment == null)
            {
                return new ApiResponse(
                    false,
                    "Vehicle assignment not found."
                );
            }

            // Restoring an open assignment puts the vehicle back in someone's hands, so
            // it has to clear the same conflicts a fresh handover would.
            if (vehicleAssignment.IsDeleted
                && vehicleAssignment.IsActive
                && vehicleAssignment.ReturnedDate == null)
            {
                var conflict = await FindActiveConflictAsync(
                    vehicleAssignment.VehicleId,
                    vehicleAssignment.DriverId,
                    vehicleAssignment.Id);

                if (conflict != null)
                {
                    return new ApiResponse(false, conflict);
                }
            }

            vehicleAssignment.IsDeleted = !vehicleAssignment.IsDeleted;
            vehicleAssignment.DeletedAt = vehicleAssignment.IsDeleted ? DateTime.UtcNow : null;

            // Toggling is a deliberate choice, so it never counts as a cascade.
            // Leaving a stale flag here would let a parent restore resurrect this row.
            vehicleAssignment.DeletedByCascade = false;

            var result = _context.VehicleAssignments.Update(vehicleAssignment);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse(
                    false,
                    "Vehicle assignment status could not be changed."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Vehicle assignment status could not be changed."
                );
            }

            return new ApiResponse(
                true,
                "Vehicle assignment status changed successfully."
            );
        }

        public async Task<ApiResponse> ReturnAsync(Guid id, DateTime? returnedDate)
        {
            var assignment = await _context.VehicleAssignments
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (assignment == null)
            {
                return new ApiResponse(false, "Vehicle assignment not found.");
            }

            if (assignment.ReturnedDate != null)
            {
                return new ApiResponse(false, "This vehicle has already been returned.");
            }

            var returnedAt = returnedDate ?? DateTime.UtcNow;

            if (returnedAt < assignment.AssignedDate)
            {
                return new ApiResponse(false, "Return date cannot be earlier than the assigned date.");
            }

            assignment.ReturnedDate = returnedAt;
            assignment.IsActive = false;
            assignment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new ApiResponse(true, "Vehicle returned successfully.");
        }

        public async Task<ApiResponse> UpdateAsync(Guid id, VehicleAssignmentUpdateDTO dto)
        {
            var vehicleAssignment = await _context.VehicleAssignments.FindAsync(id);

            if (vehicleAssignment == null)
            {
                return new ApiResponse(
                    false,
                    "Vehicle assignment not found."
                );
            }

            var referenceError = await _context.ValidateReferencesAsync(dto.VehicleId, dto.DriverId);

            if (referenceError != null)
            {
                return new ApiResponse(false, referenceError);
            }

            var conflict = await FindActiveConflictAsync(dto.VehicleId, dto.DriverId, id);

            if (conflict != null)
            {
                return new ApiResponse(false, conflict);
            }

            _mapper.Map(dto, vehicleAssignment);

            // Recording a return closes the assignment, whichever field the caller set.
            if (vehicleAssignment.ReturnedDate != null)
            {
                vehicleAssignment.IsActive = false;
            }

            vehicleAssignment.UpdatedAt = DateTime.UtcNow;

            var result = _context.VehicleAssignments.Update(vehicleAssignment);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse(
                    false,
                    "Vehicle assignment could not be updated."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Vehicle assignment could not be updated."
                );
            }

            return new ApiResponse(
                true,
                "Vehicle assignment updated successfully."
            );
        }
    }
}
