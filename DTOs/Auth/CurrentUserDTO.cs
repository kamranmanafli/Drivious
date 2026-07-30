namespace Drivious.DTOs.Auth
{
    public class CurrentUserDTO
    {
        public string Id { get; set; } = null!;

        public string UserName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public Guid? DriverId { get; set; }

        public List<string> Roles { get; set; } = new();
    }
}
