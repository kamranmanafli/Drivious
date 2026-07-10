namespace Drivious.DTOs.Insurance
{
    public class InsuranceUpdateDTO
    {
        public Guid? VehicleId { get; set; }

        public string? CompanyName { get; set; }

        public string? PolicyNumber { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public decimal? Price { get; set; }
    }
}
