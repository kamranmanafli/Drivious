namespace Drivious.DTOs.VehicleAssignment
{
    public class VehicleAssignmentUpdateDTO
    {
        public Guid? VehicleId { get; set; }

        public Guid? DriverId { get; set; }

        public DateTime? AssignedDate { get; set; }

        public DateTime? ReturnedDate { get; set; }

        public bool? IsActive { get; set; }

        public string? Note { get; set; }
    }
}
