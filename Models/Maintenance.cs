using Drivious.Enums;
using Drivious.Models.BaseModels;

namespace Drivious.Models
{
    public class Maintenance : BaseEntity
    {
        public Guid VehicleId { get; set; }                  // Hansı maşına aiddir

        public MaintenanceType ServiceType { get; set; }     // Servis növü

        public string? Description { get; set; }             // Servis haqqında qeyd

        public decimal Cost { get; set; }                    // Servis xərci

        public DateTime MaintenanceDate { get; set; }        // Servis tarixi

        public DateTime? NextMaintenanceDate { get; set; }   // Növbəti servis tarixi

        public int Mileage { get; set; }                     // Servis zamanı yürüş (km)

        public string ServiceCenter { get; set; }            // Servisin adı

        public Vehicle Vehicle { get; set; }                 // Əlaqəli maşın
    }
}
