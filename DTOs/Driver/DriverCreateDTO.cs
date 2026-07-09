namespace Drivious.DTOs.Driver
{
    public class DriverCreateDTO
    {
        public IFormFile Image { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public string IdentityNumber { get; set; }

        public string DriverLicenseNumber { get; set; }

        public DateTime LicenseExpireDate { get; set; }

        public DateTime BirthDate { get; set; }

        public DateTime HireDate { get; set; }

        public string? Address { get; set; }

        public bool IsActive { get; set; }
    }
}
