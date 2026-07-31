using Drivious.DTOs.Common;

namespace Drivious.DTOs.Driver
{
    public class DriverQueryParameters : QueryParameters
    {
        /// <summary>Matched against first name, last name, phone number and email.</summary>
        public bool? IsActive { get; set; }

        /// <summary>Only drivers whose licence expires before this date.</summary>
        public DateTime? LicenseExpiresBefore { get; set; }
    }
}
