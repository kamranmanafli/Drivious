using Drivious.Enums;

namespace Drivious.DTOs.Vehicle
{
    public class VehicleCreateDTO
    {
        public IFormFile Image { get; set; } = null!;

        public string Brand { get; set; } = null!;

        public string Model { get; set; } = null!;

        public int Year { get; set; }

        public string PlateNumber { get; set; } = null!;

        public string VIN { get; set; } = null!;

        public string Color { get; set; } = null!;

        public FuelType FuelType { get; set; }

        public int Mileage { get; set; }

        public VehicleStatus Status { get; set; }
    }
}
