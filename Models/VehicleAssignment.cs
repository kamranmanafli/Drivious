using Drivious.Models.BaseModels;

namespace Drivious.Models
{
    public class VehicleAssignment : BaseEntity
    {
        public Guid VehicleId { get; set; }          // Hansı maşın təyin olunub

        public Guid DriverId { get; set; }           // Hansı sürücüyə təyin olunub

        public DateTime AssignedDate { get; set; }   // Maşının verildiyi tarix

        public DateTime? ReturnedDate { get; set; }  // Maşının qaytarıldığı tarix

        public bool IsActive { get; set; }           // Təyinat aktivdir?

        public string? Note { get; set; }            // Əlavə qeyd

        public Vehicle Vehicle { get; set; }         // Əlaqəli maşın

        public Driver Driver { get; set; }           // Əlaqəli sürücü
    }
}
