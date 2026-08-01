using AutoMapper;
using AutoMapper.QueryableExtensions;
using Drivious.Data;
using Drivious.DTOs.Common;
using Drivious.DTOs.VehicleDocument;
using Drivious.Extensions;
using Drivious.Models;
using Drivious.Responses;
using Drivious.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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

        // Only these fields can be ordered by. Building an expression from an arbitrary
        // caller-supplied name would put user input into the query itself.
        private static readonly IReadOnlyDictionary<string, Expression<Func<VehicleDocument, object?>>> Sortable =
            new Dictionary<string, Expression<Func<VehicleDocument, object?>>>(StringComparer.OrdinalIgnoreCase)
            {
                ["title"] = x => x.Title,
                ["documentType"] = x => x.DocumentType,
                ["uploadDate"] = x => x.UploadDate,
                ["expiryDate"] = x => x.ExpiryDate,
                ["plateNumber"] = x => x.Vehicle.PlateNumber,
                ["createdAt"] = x => x.CreatedAt
            };

        private IQueryable<VehicleDocument> BuildQuery(
            VehicleDocumentQueryParameters parameters, bool deleted)
        {
            var search = parameters.Search?.Trim();

            return _context.VehicleDocuments
                .Where(x => x.IsDeleted == deleted)
                .WhereIf(parameters.VehicleId.HasValue, x => x.VehicleId == parameters.VehicleId!.Value)
                .WhereIf(parameters.DocumentType.HasValue,
                    x => x.DocumentType == parameters.DocumentType!.Value)
                .WhereIf(!string.IsNullOrWhiteSpace(search),
                    x => x.Title.Contains(search!)
                      || x.Vehicle.PlateNumber.Contains(search!))
                .ApplySort(parameters, Sortable, "uploadDate");
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

        public async Task<ApiResponse<PagedResult<VehicleDocumentGetDTO>>> GetAllAsync(
            VehicleDocumentQueryParameters parameters)
        {
            var page = await BuildQuery(parameters, deleted: false)
                .ToPagedResultAsync<VehicleDocument, VehicleDocumentGetDTO>(
                    parameters, _mapper.ConfigurationProvider);

            return new ApiResponse<PagedResult<VehicleDocumentGetDTO>>(
                true,
                "Vehicle documents retrieved successfully.",
                page
            );
        }

        public async Task<ApiResponse<PagedResult<VehicleDocumentGetDTO>>> GetDeletedAsync(
            VehicleDocumentQueryParameters parameters)
        {
            var page = await BuildQuery(parameters, deleted: true)
                .ToPagedResultAsync<VehicleDocument, VehicleDocumentGetDTO>(
                    parameters, _mapper.ConfigurationProvider);

            return new ApiResponse<PagedResult<VehicleDocumentGetDTO>>(
                true,
                "Deleted vehicle documents retrieved successfully.",
                page
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
