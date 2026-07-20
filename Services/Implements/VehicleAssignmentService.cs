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

        public async Task<ApiResponse<object>> CreateAsync(VehicleAssignmentCreateDTO dto)
        {
            VehicleAssignment vehicleAssignment = _mapper.Map<VehicleAssignment>(dto);

            vehicleAssignment.CreatedAt = DateTime.Now;

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

            var dtos = _mapper.Map<List<VehicleAssignmentGetDTO>>(vehicleAssignments);

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

            var dto = _mapper.Map<VehicleAssignmentGetDTO>(vehicleAssignment);

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

            _mapper.Map(dto, vehicleAssignment);

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
