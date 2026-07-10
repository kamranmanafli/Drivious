namespace Drivious.DTOs.Income
{
    public class IncomeUpdateDTO
    {
        public Guid? VehicleId { get; set; }

        public Guid? DriverId { get; set; }

        public decimal? Amount { get; set; }

        public DateTime? IncomeDate { get; set; }

        public string? Description { get; set; }
    }
}
