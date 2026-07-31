using Drivious.DTOs.Common;

namespace Drivious.DTOs.Income
{
    public class IncomeQueryParameters : QueryParameters
    {
        /// <summary>Matched against the description and the vehicle's plate number.</summary>
        public Guid? VehicleId { get; set; }

        public Guid? DriverId { get; set; }

        public DateTime? From { get; set; }

        public DateTime? To { get; set; }

        public decimal? MinAmount { get; set; }

        public decimal? MaxAmount { get; set; }
    }
}
