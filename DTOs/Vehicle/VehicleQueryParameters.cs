using Drivious.DTOs.Common;
using Drivious.Enums;

namespace Drivious.DTOs.Vehicle
{
    public class VehicleQueryParameters : QueryParameters
    {
        /// <summary>Matched against brand, model, plate number and VIN.</summary>
        public VehicleStatus? Status { get; set; }

        public FuelType? FuelType { get; set; }

        public string? Brand { get; set; }

        public int? MinYear { get; set; }

        public int? MaxYear { get; set; }
    }
}
