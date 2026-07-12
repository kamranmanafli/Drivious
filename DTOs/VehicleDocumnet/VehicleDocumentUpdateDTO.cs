using Drivious.Enums;

namespace Drivious.DTOs.VehicleDocumnet
{
    public class VehicleDocumentUpdateDTO
    {
        public Guid? VehicleId { get; set; }

        public string? Title { get; set; }

        public DocumentType? DocumentType { get; set; }

        public IFormFile? File { get; set; }

        public DateTime? UploadDate { get; set; }
    }
}
