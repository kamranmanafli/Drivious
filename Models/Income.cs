using Drivious.Models.BaseModels;

namespace Drivious.Models
{
    public class Income : BaseEntity
    {
        public Guid VehicleId { get; set; }          // Gəlir hansı maşından əldə olunub

        public Guid DriverId { get; set; }           // Gəliri hansı sürücü gətirib

        public decimal Amount { get; set; }          // Gəlirin məbləği

        public DateTime IncomeDate { get; set; }     // Gəlirin tarixi

        public string? Description { get; set; }     // Gəlir haqqında qeyd

        public Vehicle Vehicle { get; set; }         // Əlaqəli maşın

        public Driver Driver { get; set; }           // Əlaqəli sürücü
    }
}
