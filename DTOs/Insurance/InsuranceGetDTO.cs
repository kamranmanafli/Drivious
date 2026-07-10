namespace Drivious.DTOs.Insurance
{
    public class InsuranceGetDTO
    {
        public Guid Id { get; set; }

        public Guid VehicleId { get; set; }

        public string CompanyName { get; set; }

        public string PolicyNumber { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public decimal Price { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        public bool IsDeleted { get; set; }
    }
}
