using Drivious.Enums;

namespace Drivious.DTOs.Expense
{
    public class ExpenseGetDTO
    {
        public Guid Id { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        public bool IsDeleted { get; set; }

        public Guid VehicleId { get; set; }

        /// <summary>Projected from the vehicle so a list can be rendered in one call.</summary>
        public string? VehiclePlateNumber { get; set; }

        public string? VehicleName { get; set; }

        public ExpenseCategory Category { get; set; }

        public decimal Amount { get; set; }

        public DateTime ExpenseDate { get; set; }

        public string Description { get; set; } = null!;
    }
}
