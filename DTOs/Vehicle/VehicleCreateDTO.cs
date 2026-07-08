using Drivious.Enums;

namespace Drivious.DTOs.Vehicle
{
    public class VehicleCreateDTO
    {
        public IFormFile Image { get; set; }

        public string Brand { get; set; }

        public string Model { get; set; }

        public int Year { get; set; }

        public string PlateNumber { get; set; }

        public string VIN { get; set; }

        public string Color { get; set; }

        public FuelType FuelType { get; set; }

        public int Mileage { get; set; }

        public VehicleStatus Status { get; set; }
    }
}
