namespace Drivious.DTOs.Insurance
{
    public class InsuranceCreateDTO
    {
        public Guid VehicleId { get; set; }

        public string CompanyName { get; set; } = null!;

        public string PolicyNumber { get; set; } = null!;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public decimal Price { get; set; }
    }
}
