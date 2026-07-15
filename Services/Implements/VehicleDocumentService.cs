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

        public VehicleDocumentService(
            AppDbContext context,
            IWebHostEnvironment env,
            IHttpContextAccessor accessor)
        {
            _context = context;
            _env = env;
            _accessor = accessor;
        }

        public async Task<ApiResponse<object>> CreateAsync(VehicleDocumentCreateDTO dto)
        {
            VehicleDocument vehicleDocument = new()
            {
                CreatedAt = DateTime.Now,

                VehicleId = dto.VehicleId,
                Title = dto.Title,
                DocumentType = dto.DocumentType,

                FileName = await dto.File.CreateFileAsync(_env.WebRootPath, "Files", "VehicleDocuments"),

                UploadDate = DateTime.Now,
            };

            vehicleDocument.FileUrl = $"{_accessor.HttpContext.Request.Scheme}://{_accessor.HttpContext.Request.Host}/Files/VehicleDocuments/{vehicleDocument.FileName}";

            var result = await _context.VehicleDocuments.AddAsync(vehicleDocument);

            if (result.State != EntityState.Added)
            {
                return new ApiResponse<object>(
                    false,
                    "Vehicle document could not be created.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Vehicle document could not be saved.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Vehicle document created successfully.",
                null
            );
        }

        public async Task<ApiResponse<List<VehicleDocumentGetDTO>>> GetAllAsync()
        {
            var documents = await _context.VehicleDocuments.ToListAsync();

            var dtos = documents.Select(d => new VehicleDocumentGetDTO
            {
                Id = d.Id,

                VehicleId = d.VehicleId,
                Title = d.Title,
                DocumentType = d.DocumentType,
                FileName = d.FileName,
                FileUrl = d.FileUrl,
                UploadDate = d.UploadDate,

                CreatedAt = d.CreatedAt,
                UpdatedAt = d.UpdatedAt,
                DeletedAt = d.DeletedAt,
                IsDeleted = d.IsDeleted

            }).ToList();

            return new ApiResponse<List<VehicleDocumentGetDTO>>(
                true,
                "Vehicle documents retrieved successfully.",
                dtos
            );
        }

        public async Task<ApiResponse<VehicleDocumentGetDTO>> GetAsync(Guid id)
        {
            var document = await _context.VehicleDocuments.FindAsync(id);

            if (document == null)
            {
                return new ApiResponse<VehicleDocumentGetDTO>(
                    false,
                    "Vehicle document not found.",
                    null
                );
            }

            var dto = new VehicleDocumentGetDTO()
            {
                Id = document.Id,

                VehicleId = document.VehicleId,
                Title = document.Title,
                DocumentType = document.DocumentType,
                FileName = document.FileName,
                FileUrl = document.FileUrl,
                UploadDate = document.UploadDate,

                CreatedAt = document.CreatedAt,
                UpdatedAt = document.UpdatedAt,
                DeletedAt = document.DeletedAt,
                IsDeleted = document.IsDeleted
            };

            return new ApiResponse<VehicleDocumentGetDTO>(
                true,
                "Vehicle document retrieved successfully.",
                dto
            );
        }

        public async Task<ApiResponse<object>> RemoveAsync(Guid id)
        {
            var document = await _context.VehicleDocuments.FindAsync(id);

            if (document == null)
            {
                return new ApiResponse<object>(
                    false,
                    "Vehicle document not found.",
                    null
                );
            }

            document.FileName.DeleteFile(_env.WebRootPath, "Files", "VehicleDocuments");

            var result = _context.VehicleDocuments.Remove(document);

            if (result.State != EntityState.Deleted)
            {
                return new ApiResponse<object>(
                    false,
                    "Vehicle document could not be deleted.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Vehicle document could not be deleted.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Vehicle document deleted successfully.",
                null
            );
        }

        public async Task<ApiResponse<object>> ToggleAsync(Guid id)
        {
            var document = await _context.VehicleDocuments.FindAsync(id);

            if (document == null)
            {
                return new ApiResponse<object>(
                    false,
                    "Vehicle document not found.",
                    null
                );
            }

            document.IsDeleted = !document.IsDeleted;
            document.DeletedAt = document.IsDeleted ? DateTime.Now : null;

            var result = _context.VehicleDocuments.Update(document);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse<object>(
                    false,
                    "Vehicle document status could not be changed.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Vehicle document status could not be changed.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Vehicle document status changed successfully.",
                null
            );
        }

        public async Task<ApiResponse<object>> UpdateAsync(Guid id, VehicleDocumentUpdateDTO dto)
        {
            var document = await _context.VehicleDocuments.FindAsync(id);

            if (document == null)
            {
                return new ApiResponse<object>(
                    false,
                    "Vehicle document not found.",
                    null
                );
            }

            if (dto.File != null)
            {
                if (!string.IsNullOrEmpty(document.FileName))
                {
                    document.FileName.DeleteFile(_env.WebRootPath, "Files", "VehicleDocuments");
                }

                document.FileName = await dto.File.CreateFileAsync(_env.WebRootPath, "Files", "VehicleDocuments");

                document.FileUrl = $"{_accessor.HttpContext.Request.Scheme}://{_accessor.HttpContext.Request.Host}/Files/VehicleDocuments/{document.FileName}";

                document.UploadDate = DateTime.Now;
            }

            document.VehicleId = dto.VehicleId ?? document.VehicleId;
            document.Title = dto.Title ?? document.Title;
            document.DocumentType = dto.DocumentType ?? document.DocumentType;

            document.UpdatedAt = DateTime.Now;

            var result = _context.VehicleDocuments.Update(document);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse<object>(
                    false,
                    "Vehicle document could not be updated.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Vehicle document could not be updated.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Vehicle document updated successfully.",
                null
            );
        }
    }
}
