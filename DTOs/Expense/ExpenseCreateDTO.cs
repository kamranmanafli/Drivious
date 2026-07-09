using Drivious.Enums;

namespace Drivious.DTOs.Expense
{
    public class ExpenseCreateDTO
    {
        public Guid VehicleId { get; set; }

        public ExpenseCategory Category { get; set; }

        public decimal Amount { get; set; }

        public DateTime ExpenseDate { get; set; }

        public string Description { get; set; }
    }
}
