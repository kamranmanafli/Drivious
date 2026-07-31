namespace Drivious.DTOs.Insurance
{
    public class InsuranceGetDTO
    {
        public Guid Id { get; set; }

        public Guid VehicleId { get; set; }

        /// <summary>Projected from the vehicle so a list can be rendered in one call.</summary>
        public string? VehiclePlateNumber { get; set; }

        public string? VehicleName { get; set; }

        public string CompanyName { get; set; } = null!;

        public string PolicyNumber { get; set; } = null!;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public decimal Price { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        public bool IsDeleted { get; set; }
    }
}
