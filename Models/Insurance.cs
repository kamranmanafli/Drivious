using Drivious.Models.BaseModels;

namespace Drivious.Models
{
    public class Insurance : BaseEntity
    {
        public Guid VehicleId { get; set; }           // Sığorta hansı maşına aiddir

        public string CompanyName { get; set; }       // Sığorta şirkətinin adı

        public string PolicyNumber { get; set; }      // Sığorta polisinin nömrəsi

        public DateTime StartDate { get; set; }       // Sığortanın başlama tarixi

        public DateTime EndDate { get; set; }         // Sığortanın bitmə tarixi

        public decimal Price { get; set; }            // Sığortanın qiyməti

        public Vehicle Vehicle { get; set; }          // Əlaqəli maşın
    }
}
