using Drivious.DTOs.Common;

namespace Drivious.DTOs.FuelLog
{
    public class FuelLogQueryParameters : QueryParameters
    {
        /// <summary>Matched against the station name and the vehicle's plate number.</summary>
        public Guid? VehicleId { get; set; }

        public DateTime? From { get; set; }

        public DateTime? To { get; set; }
    }
}
