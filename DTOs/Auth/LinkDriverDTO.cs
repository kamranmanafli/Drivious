namespace Drivious.DTOs.Auth
{
    public class LinkDriverDTO
    {
        public string UserName { get; set; } = null!;

        /// <summary>Null unlinks the account from any driver record.</summary>
        public Guid? DriverId { get; set; }
    }
}
