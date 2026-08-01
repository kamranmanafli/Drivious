using AutoMapper;
using AutoMapper.QueryableExtensions;
using Drivious.Data;
using Drivious.DTOs.Common;
using Drivious.DTOs.VehicleAssignment;
using Drivious.Extensions;
using Drivious.Models;
using Drivious.Responses;
using Drivious.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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

        // Only these fields can be ordered by. Building an expression from an arbitrary
        // caller-supplied name would put user input into the query itself.
        private static readonly IReadOnlyDictionary<string, Expression<Func<VehicleAssignment, object?>>> Sortable =
            new Dictionary<string, Expression<Func<VehicleAssignment, object?>>>(StringComparer.OrdinalIgnoreCase)
            {
                ["assignedDate"] = x => x.AssignedDate,
                ["returnedDate"] = x => x.ReturnedDate,
                ["isActive"] = x => x.IsActive,
                ["plateNumber"] = x => x.Vehicle.PlateNumber,
                ["driver"] = x => x.Driver.FirstName,
                ["createdAt"] = x => x.CreatedAt
            };

        private IQueryable<VehicleAssignment> BuildQuery(
            VehicleAssignmentQueryParameters parameters, bool deleted)
        {
            var search = parameters.Search?.Trim();

            return _context.VehicleAssignments
                .Where(x => x.IsDeleted == deleted)
                .WhereIf(parameters.VehicleId.HasValue, x => x.VehicleId == parameters.VehicleId!.Value)
                .WhereIf(parameters.DriverId.HasValue, x => x.DriverId == parameters.DriverId!.Value)
                .WhereIf(parameters.IsActive.HasValue, x => x.IsActive == parameters.IsActive!.Value)
                .WhereIf(parameters.IsOpen == true, x => x.ReturnedDate == null)
                .WhereIf(parameters.IsOpen == false, x => x.ReturnedDate != null)
                .WhereIf(!string.IsNullOrWhiteSpace(search),
                    x => (x.Note != null && x.Note.Contains(search!))
                      || x.Vehicle.PlateNumber.Contains(search!)
                      || x.Driver.FirstName.Contains(search!)
                      || x.Driver.LastName.Contains(search!))
                .ApplySort(parameters, Sortable, "assignedDate");
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

        public async Task<ApiResponse<PagedResult<VehicleAssignmentGetDTO>>> GetAllAsync(
            VehicleAssignmentQueryParameters parameters)
        {
            var page = await BuildQuery(parameters, deleted: false)
                .ToPagedResultAsync<VehicleAssignment, VehicleAssignmentGetDTO>(
                    parameters, _mapper.ConfigurationProvider);

            return new ApiResponse<PagedResult<VehicleAssignmentGetDTO>>(
                true,
                "Vehicle assignments retrieved successfully.",
                page
            );
        }

        public async Task<ApiResponse<PagedResult<VehicleAssignmentGetDTO>>> GetDeletedAsync(
            VehicleAssignmentQueryParameters parameters)
        {
            var page = await BuildQuery(parameters, deleted: true)
                .ToPagedResultAsync<VehicleAssignment, VehicleAssignmentGetDTO>(
                    parameters, _mapper.ConfigurationProvider);

            return new ApiResponse<PagedResult<VehicleAssignmentGetDTO>>(
                true,
                "Deleted vehicle assignments retrieved successfully.",
                page
            );
        }

        public async Task<ApiResponse<VehicleAssignmentGetDTO>> GetAsync(Guid id)
        {
            var dto = await _context.VehicleAssignments
                .Where(x => x.Id == id && !x.IsDeleted)
                .ProjectTo<VehicleAssignmentGetDTO>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (dto == null)
            {
                return new ApiResponse<VehicleAssignmentGetDTO>(
                    false,
                    "Vehicle assignment not found.",
                    null
                );
            }

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
