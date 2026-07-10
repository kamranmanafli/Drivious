using Drivious.Enums;

namespace Drivious.DTOs.Maintenance
{
    public class MaintenanceGetDTO
    {
        public Guid Id { get; set; }

        public Guid VehicleId { get; set; }

        public MaintenanceType ServiceType { get; set; }

        public string? Description { get; set; }

        public decimal Cost { get; set; }

        public DateTime MaintenanceDate { get; set; }

        public DateTime? NextMaintenanceDate { get; set; }

        public int Mileage { get; set; }

        public string ServiceCenter { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        public bool IsDeleted { get; set; }
    }
}
