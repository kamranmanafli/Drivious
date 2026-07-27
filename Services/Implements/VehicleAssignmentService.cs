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

        public async Task<ApiResponse> CreateAsync(VehicleAssignmentCreateDTO dto)
        {
            VehicleAssignment vehicleAssignment = _mapper.Map<VehicleAssignment>(dto);

            vehicleAssignment.CreatedAt = DateTime.Now;

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

            vehicleAssignment.IsDeleted = !vehicleAssignment.IsDeleted;
            vehicleAssignment.DeletedAt = vehicleAssignment.IsDeleted ? DateTime.Now : null;

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

            _mapper.Map(dto, vehicleAssignment);

            vehicleAssignment.UpdatedAt = DateTime.Now;

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
