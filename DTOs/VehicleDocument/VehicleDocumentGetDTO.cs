using Drivious.Enums;

namespace Drivious.DTOs.VehicleDocumnet
{
    public class VehicleDocumentGetDTO
    {
        public Guid Id { get; set; }

        public Guid VehicleId { get; set; }

        public string Title { get; set; }

        public DocumentType DocumentType { get; set; }

        public string FileName { get; set; }

        public string FileUrl { get; set; }

        public DateTime UploadDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        public bool IsDeleted { get; set; }

    }
}
