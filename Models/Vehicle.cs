using Drivious.Enums;
using Drivious.Models.BaseModels;
using System.Reflection.Metadata;

namespace Drivious.Models
{
    public class Vehicle : BaseEntity
    {
        public string Brand { get; set; }                          // Maşının markası

        public string Model { get; set; }                          // Maşının modeli

        public int Year { get; set; }                              // Buraxılış ili

        public string PlateNumber { get; set; }                    // Dövlət nömrə nişanı

        public string VIN { get; set; }                            // VIN nömrəsi

        public string Color { get; set; }                          // Maşının rəngi

        public FuelType FuelType { get; set; }                     // Yanacaq növü

        public int Mileage { get; set; }                           // Cari yürüş (km)

        public VehicleStatus Status { get; set; }                  // Maşının statusu

        public string? Image { get; set; }                         // Şəkil adı

        public string? ImageURL { get; set; }                      // Şəkilin yolu

        public List<VehicleAssignment> VehicleAssignments { get; set; } = new(); // Təyinat tarixçəsi

        public List<Expense> Expenses { get; set; } = new();       // Xərc tarixçəsi

        public List<Income> Incomes { get; set; } = new();         // Gəlir tarixçəsi

        public List<Maintenance> Maintenances { get; set; } = new(); // Servis tarixçəsi

        public List<Insurance> Insurances { get; set; } = new();   // Sığorta tarixçəsi

        public List<FuelLog> FuelLogs { get; set; } = new();       // Yanacaq tarixçəsi

        public List<VehicleDocument> vehicleDocuments { get; set; } = new();     // Sənədlər
    }
}
