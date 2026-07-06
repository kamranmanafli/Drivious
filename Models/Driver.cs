using Drivious.Models.BaseModels;

namespace Drivious.Models
{
    public class Driver : BaseEntity
    {
        public string FirstName { get; set; }                    // Ad

        public string LastName { get; set; }                     // Soyad

        public string PhoneNumber { get; set; }                  // Telefon nömrəsi

        public string Email { get; set; }                        // E-poçt ünvanı

        public string IdentityNumber { get; set; }               // Ş/V FIN və ya seriya nömrəsi

        public string DriverLicenseNumber { get; set; }          // Sürücülük vəsiqəsinin nömrəsi

        public DateTime LicenseExpireDate { get; set; }          // Vəsiqənin bitmə tarixi

        public DateTime BirthDate { get; set; }                  // Doğum tarixi

        public DateTime HireDate { get; set; }                   // İşə qəbul tarixi

        public string? Address { get; set; }                      // Yaşayış ünvanı

        public string? Image { get; set; }                        // Şəkil adı

        public string? ImageUrl { get; set; }                     // Şəkilin yolu

        public bool IsActive { get; set; }                       // Aktiv sürücüdür?

        public List<VehicleAssignment> VehicleAssignments { get; set; } = new(); // Təyin olunan maşınlar

        public List<Income> Incomes { get; set; } = new();       // Gəlir tarixçəsi
    }
}
