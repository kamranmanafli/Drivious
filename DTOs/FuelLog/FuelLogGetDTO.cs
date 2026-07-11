namespace Drivious.DTOs.FuelLog
{
    public class FuelLogGetDTO
    {
        public Guid Id { get; set; }

        public Guid VehicleId { get; set; }

        public decimal Liters { get; set; }

        public decimal Price { get; set; }

        public DateTime FuelDate { get; set; }

        public int Mileage { get; set; }

        public string StationName { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        public bool IsDeleted { get; set; }
    }
}
