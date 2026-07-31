using Drivious.DTOs.Common;

namespace Drivious.DTOs.Insurance
{
    public class InsuranceQueryParameters : QueryParameters
    {
        /// <summary>Matched against the company name, policy number and plate number.</summary>
        public Guid? VehicleId { get; set; }

        /// <summary>Only policies that have already lapsed or expire before this date.</summary>
        public DateTime? ExpiresBefore { get; set; }

        /// <summary>Only policies in force on the given date.</summary>
        public DateTime? ActiveOn { get; set; }
    }
}
