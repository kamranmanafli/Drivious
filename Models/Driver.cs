using Drivious.Models.BaseModels;

namespace Drivious.Models
{
    public class Driver : BaseEntity
    {
        public string FirstName { get; set; } = null!;                    // Ad

        public string LastName { get; set; } = null!;                     // Soyad

        public string PhoneNumber { get; set; } = null!;                  // Telefon nömrəsi

        public string Email { get; set; } = null!;                        // E-poçt ünvanı

        public string IdentityNumber { get; set; } = null!;               // Ş/V FIN və ya seriya nömrəsi

        public string DriverLicenseNumber { get; set; } = null!;          // Sürücülük vəsiqəsinin nömrəsi

        public DateTime LicenseExpireDate { get; set; }          // Vəsiqənin bitmə tarixi

        public DateTime BirthDate { get; set; }                  // Doğum tarixi

        public DateTime HireDate { get; set; }                   // İşə qəbul tarixi

        public string? Address { get; set; }                      // Yaşayış ünvanı

        public string? Image { get; set; }                        // Şəkil adı

        public string? ImageUrl { get; set; }                     // Şəkilin yolu

        public bool IsActive { get; set; }                       // Aktiv sürücüdür?

        public List<VehicleAssignment> VehicleAssignments { get; set; } = new(); // Təyin olunan maşınlar

        public List<Income> Incomes { get; set; } = new();       // Gəlir tarixçəsi

        public AppUser? User { get; set; }                       // Bu sürücüyə bağlı hesab
    }
}
