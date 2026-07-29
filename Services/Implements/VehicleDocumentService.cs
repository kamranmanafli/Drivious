using AutoMapper;
using Drivious.Data;
using Drivious.DTOs.VehicleDocumnet;
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

        public async Task<ApiResponse> CreateAsync(VehicleDocumentCreateDTO dto)
        {
            VehicleDocument vehicleDocument = _mapper.Map<VehicleDocument>(dto);

            vehicleDocument.CreatedAt = DateTime.Now;

            vehicleDocument.FileName = await dto.File.CreateFileAsync(
                _env.WebRootPath,
                "Files",
                "VehicleDocuments");

            vehicleDocument.FileUrl =
                $"{_accessor.HttpContext.Request.Scheme}://{_accessor.HttpContext.Request.Host}/Files/VehicleDocuments/{vehicleDocument.FileName}";

            vehicleDocument.UploadDate = DateTime.Now;

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
            var documents = await _context.VehicleDocuments
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToListAsync();

            var dtos = _mapper.Map<List<VehicleDocumentGetDTO>>(documents);

            return new ApiResponse<List<VehicleDocumentGetDTO>>(
                true,
                "Vehicle documents retrieved successfully.",
                dtos
            );
        }

        public async Task<ApiResponse<List<VehicleDocumentGetDTO>>> GetDeletedAsync()
        {
            var documents = await _context.VehicleDocuments
                .AsNoTracking()
                .Where(x => x.IsDeleted)
                .ToListAsync();

            var dtos = _mapper.Map<List<VehicleDocumentGetDTO>>(documents);

            return new ApiResponse<List<VehicleDocumentGetDTO>>(
                true,
                "Deleted vehicle documents retrieved successfully.",
                dtos
            );
        }

        public async Task<ApiResponse<VehicleDocumentGetDTO>> GetAsync(Guid id)
        {
            var document = await _context.VehicleDocuments
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (document == null)
            {
                return new ApiResponse<VehicleDocumentGetDTO>(
                    false,
                    "Vehicle document not found.",
                    null
                );
            }

            var dto = _mapper.Map<VehicleDocumentGetDTO>(document);

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
            document.DeletedAt = document.IsDeleted ? DateTime.Now : null;

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

                document.FileUrl =
                    $"{_accessor.HttpContext.Request.Scheme}://{_accessor.HttpContext.Request.Host}/Files/VehicleDocuments/{document.FileName}";

                document.UploadDate = DateTime.Now;
            }

            _mapper.Map(dto, document);

            document.UpdatedAt = DateTime.Now;

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
