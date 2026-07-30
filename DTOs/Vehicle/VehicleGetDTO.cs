using Drivious.Enums;

namespace Drivious.DTOs.Vehicle
{
    public class VehicleGetDTO
    {
        public Guid Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        public bool IsDeleted { get; set; }

        public string? Image { get; set; }

        public string? ImageURL { get; set; }

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
