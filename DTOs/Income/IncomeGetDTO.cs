namespace Drivious.DTOs.Income
{
    public class IncomeGetDTO
    {
        public Guid Id { get; set; }

        public Guid VehicleId { get; set; }

        /// <summary>Projected from the vehicle so a list can be rendered in one call.</summary>
        public string? VehiclePlateNumber { get; set; }

        public string? VehicleName { get; set; }

        public Guid DriverId { get; set; }

        public string? DriverFullName { get; set; }

        public decimal Amount { get; set; }

        public DateTime IncomeDate { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        public bool IsDeleted { get; set; }
    }
}
