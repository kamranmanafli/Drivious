using Drivious.Enums;

namespace Drivious.DTOs.Maintenance
{
    public class MaintenanceCreateDTO
    {
        public Guid VehicleId { get; set; }

        public MaintenanceType ServiceType { get; set; }

        public string? Description { get; set; }

        public decimal Cost { get; set; }

        public DateTime MaintenanceDate { get; set; }

        public DateTime? NextMaintenanceDate { get; set; }

        public int Mileage { get; set; }

        public string ServiceCenter { get; set; }
    }
}
