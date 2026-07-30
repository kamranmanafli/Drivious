using Drivious.Enums;

namespace Drivious.DTOs.VehicleDocument
{
    public class VehicleDocumentCreateDTO
    {
        public Guid VehicleId { get; set; }

        public string Title { get; set; } = null!;

        public DocumentType DocumentType { get; set; }

        public IFormFile File { get; set; } = null!;
    }
}
