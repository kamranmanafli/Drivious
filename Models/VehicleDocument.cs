using Drivious.Enums;
using Drivious.Models.BaseModels;
using System.Xml.Linq;

namespace Drivious.Models
{
    public class VehicleDocument : BaseEntity
    {
        public Guid VehicleId { get; set; }          // Hansı maşına aiddir

        public string Title { get; set; }            // Sənədin adı

        public DocumentType DocumentType { get; set; } // Sənədin növü

        public string FileName { get; set; }         // Faylın adı

        public string FileUrl { get; set; }          // Faylın yolu

        public DateTime UploadDate { get; set; }     // Yüklənmə tarixi

        public Vehicle Vehicle { get; set; }         // Əlaqəli maşın
    }
}
