namespace Drivious.DTOs.VehicleAssignment
{
    public class VehicleAssignmentGetDTO
    {
        public Guid Id { get; set; }

        public Guid VehicleId { get; set; }

        public Guid DriverId { get; set; }

        public DateTime AssignedDate { get; set; }

        public DateTime? ReturnedDate { get; set; }

        public bool IsActive { get; set; }

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        public bool IsDeleted { get; set; }
    }
}
