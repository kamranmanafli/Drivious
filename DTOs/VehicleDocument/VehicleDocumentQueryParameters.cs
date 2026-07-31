using Drivious.DTOs.Common;
using Drivious.Enums;

namespace Drivious.DTOs.VehicleDocument
{
    public class VehicleDocumentQueryParameters : QueryParameters
    {
        /// <summary>Matched against the title and the vehicle's plate number.</summary>
        public Guid? VehicleId { get; set; }

        public DocumentType? DocumentType { get; set; }
    }
}
