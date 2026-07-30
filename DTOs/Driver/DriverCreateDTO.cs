namespace Drivious.DTOs.Driver
{
    public class DriverCreateDTO
    {
        public IFormFile Image { get; set; } = null!;

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string PhoneNumber { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string IdentityNumber { get; set; } = null!;

        public string DriverLicenseNumber { get; set; } = null!;

        public DateTime LicenseExpireDate { get; set; }

        public DateTime BirthDate { get; set; }

        public DateTime HireDate { get; set; }

        public string? Address { get; set; }

        public bool IsActive { get; set; }
    }
}
