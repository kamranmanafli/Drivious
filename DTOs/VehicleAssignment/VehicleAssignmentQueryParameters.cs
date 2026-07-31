using Drivious.DTOs.Common;

namespace Drivious.DTOs.VehicleAssignment
{
    public class VehicleAssignmentQueryParameters : QueryParameters
    {
        /// <summary>Matched against the note, plate number and driver name.</summary>
        public Guid? VehicleId { get; set; }

        public Guid? DriverId { get; set; }

        public bool? IsActive { get; set; }

        /// <summary>Only handovers still open, i.e. with no return recorded.</summary>
        public bool? IsOpen { get; set; }
    }
}
