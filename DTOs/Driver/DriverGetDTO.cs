namespace Drivious.DTOs.Driver
{
    public class DriverGetDTO
    {
        public Guid Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        public bool IsDeleted { get; set; }

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

        public string? Image { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; }
    }
}
