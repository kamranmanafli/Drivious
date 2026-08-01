using AutoMapper;
using AutoMapper.QueryableExtensions;
using Drivious.Data;
using Drivious.DTOs.VehicleDocument;
using Drivious.Extensions;
using Drivious.Models;
using Drivious.Responses;
using Drivious.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Drivious.Services.Implements
{
    public class VehicleDocumentService : IVehicleDocumentService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _accessor;
        private readonly IMapper _mapper;

        public VehicleDocumentService(
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

        private string? BuildFileUrl(string? fileName)
        {
            var request = _accessor.HttpContext?.Request;

            if (request == null || string.IsNullOrEmpty(fileName)) return null;

            return $"{request.Scheme}://{request.Host}/Files/VehicleDocuments/{fileName}";
        }

        public async Task<ApiResponse> CreateAsync(VehicleDocumentCreateDTO dto)
        {
            var referenceError = await _context.ValidateReferencesAsync(dto.VehicleId);

            if (referenceError != null)
            {
                return new ApiResponse(false, referenceError);
            }

            VehicleDocument vehicleDocument = _mapper.Map<VehicleDocument>(dto);

            vehicleDocument.CreatedAt = DateTime.UtcNow;

            vehicleDocument.FileName = await dto.File.CreateFileAsync(
                _env.WebRootPath,
                "Files",
                "VehicleDocuments");

            vehicleDocument.FileUrl = BuildFileUrl(vehicleDocument.FileName)!;

            vehicleDocument.UploadDate = DateTime.UtcNow;

            var result = await _context.VehicleDocuments.AddAsync(vehicleDocument);

            if (result.State != EntityState.Added)
            {
                return new ApiResponse(
                    false,
                    "Vehicle document could not be created."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Vehicle document could not be saved."
                );
            }

            return new ApiResponse(
                true,
                "Vehicle document created successfully."
            );
        }

        public async Task<ApiResponse<List<VehicleDocumentGetDTO>>> GetAllAsync()
        {
            // Projected in the database so the vehicle columns the DTO carries are
            // resolved by a join; mapping loaded entities would leave them null.
            var dtos = await _context.VehicleDocuments
                .Where(x => !x.IsDeleted)
                .ProjectTo<VehicleDocumentGetDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return new ApiResponse<List<VehicleDocumentGetDTO>>(
                true,
                "Vehicle documents retrieved successfully.",
                dtos
            );
        }

        public async Task<ApiResponse<List<VehicleDocumentGetDTO>>> GetDeletedAsync()
        {
            var dtos = await _context.VehicleDocuments
                .Where(x => x.IsDeleted)
                .ProjectTo<VehicleDocumentGetDTO>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return new ApiResponse<List<VehicleDocumentGetDTO>>(
                true,
                "Deleted vehicle documents retrieved successfully.",
                dtos
            );
        }

        public async Task<ApiResponse<VehicleDocumentGetDTO>> GetAsync(Guid id)
        {
            var dto = await _context.VehicleDocuments
                .Where(x => x.Id == id && !x.IsDeleted)
                .ProjectTo<VehicleDocumentGetDTO>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            if (dto == null)
            {
                return new ApiResponse<VehicleDocumentGetDTO>(
                    false,
                    "Vehicle document not found.",
                    null
                );
            }

            return new ApiResponse<VehicleDocumentGetDTO>(
                true,
                "Vehicle document retrieved successfully.",
                dto
            );
        }

        public async Task<ApiResponse> RemoveAsync(Guid id)
        {
            var document = await _context.VehicleDocuments.FindAsync(id);

            if (document == null)
            {
                return new ApiResponse(
                    false,
                    "Vehicle document not found."
                );
            }

            document.FileName.DeleteFile(_env.WebRootPath, "Files", "VehicleDocuments");

            var result = _context.VehicleDocuments.Remove(document);

            if (result.State != EntityState.Deleted)
            {
                return new ApiResponse(
                    false,
                    "Vehicle document could not be deleted."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Vehicle document could not be deleted."
                );
            }

            return new ApiResponse(
                true,
                "Vehicle document deleted successfully."
            );
        }

        public async Task<ApiResponse> ToggleAsync(Guid id)
        {
            var document = await _context.VehicleDocuments.FindAsync(id);

            if (document == null)
            {
                return new ApiResponse(
                    false,
                    "Vehicle document not found."
                );
            }

            document.IsDeleted = !document.IsDeleted;
            document.DeletedAt = document.IsDeleted ? DateTime.UtcNow : null;

            // Toggling is a deliberate choice, so it never counts as a cascade.
            // Leaving a stale flag here would let a vehicle restore resurrect this row.
            document.DeletedByCascade = false;

            var result = _context.VehicleDocuments.Update(document);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse(
                    false,
                    "Vehicle document status could not be changed."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Vehicle document status could not be changed."
                );
            }

            return new ApiResponse(
                true,
                "Vehicle document status changed successfully."
            );
        }

        public async Task<ApiResponse> UpdateAsync(Guid id, VehicleDocumentUpdateDTO dto)
        {
            var document = await _context.VehicleDocuments.FindAsync(id);

            if (document == null)
            {
                return new ApiResponse(
                    false,
                    "Vehicle document not found."
                );
            }

            var referenceError = await _context.ValidateReferencesAsync(dto.VehicleId);

            if (referenceError != null)
            {
                return new ApiResponse(false, referenceError);
            }

            _mapper.Map(dto, document);

            if (dto.File != null)
            {
                if (!string.IsNullOrEmpty(document.FileName))
                {
                    document.FileName.DeleteFile(_env.WebRootPath, "Files", "VehicleDocuments");
                }

                document.FileName = await dto.File.CreateFileAsync(
                    _env.WebRootPath,
                    "Files",
                    "VehicleDocuments");

                document.FileUrl = BuildFileUrl(document.FileName)!;

                document.UploadDate = DateTime.UtcNow;
            }

            document.UpdatedAt = DateTime.UtcNow;

            var result = _context.VehicleDocuments.Update(document);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse(
                    false,
                    "Vehicle document could not be updated."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Vehicle document could not be updated."
                );
            }

            return new ApiResponse(
                true,
                "Vehicle document updated successfully."
            );
        }
    }
}
