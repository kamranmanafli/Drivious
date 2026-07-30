using Drivious.Enums;

namespace Drivious.DTOs.VehicleDocument
{
    public class VehicleDocumentGetDTO
    {
        public Guid Id { get; set; }

        public Guid VehicleId { get; set; }

        public string Title { get; set; } = null!;

        public DocumentType DocumentType { get; set; }

        public string FileName { get; set; } = null!;

        public string FileUrl { get; set; } = null!;

        public DateTime UploadDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        public bool IsDeleted { get; set; }

    }
}
