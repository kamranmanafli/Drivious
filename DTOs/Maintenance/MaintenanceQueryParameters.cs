using Drivious.DTOs.Common;
using Drivious.Enums;

namespace Drivious.DTOs.Maintenance
{
    public class MaintenanceQueryParameters : QueryParameters
    {
        /// <summary>Matched against the service centre, description and plate number.</summary>
        public Guid? VehicleId { get; set; }

        public MaintenanceType? ServiceType { get; set; }

        public DateTime? From { get; set; }

        public DateTime? To { get; set; }

        /// <summary>Only records whose next service falls before this date.</summary>
        public DateTime? DueBefore { get; set; }
    }
}
