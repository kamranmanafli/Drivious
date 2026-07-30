using Drivious.Enums;
using Drivious.Models.BaseModels;

namespace Drivious.Models
{
    public class VehicleDocument : BaseEntity
    {
        public Guid VehicleId { get; set; }          // Hansı maşına aiddir

        public string Title { get; set; } = null!;            // Sənədin adı

        public DocumentType DocumentType { get; set; } // Sənədin növü

        public string FileName { get; set; } = null!;         // Faylın adı

        public string FileUrl { get; set; } = null!;          // Faylın yolu

        public DateTime UploadDate { get; set; }     // Yüklənmə tarixi

        public Vehicle Vehicle { get; set; } = null!;         // Əlaqəli maşın
    }
}
