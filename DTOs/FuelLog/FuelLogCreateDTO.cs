namespace Drivious.DTOs.FuelLog
{
    public class FuelLogCreateDTO
    {
        public Guid VehicleId { get; set; }

        public decimal Liters { get; set; }

        public decimal Price { get; set; }

        public DateTime FuelDate { get; set; }

        public int Mileage { get; set; }

        public string StationName { get; set; }
    }
}
