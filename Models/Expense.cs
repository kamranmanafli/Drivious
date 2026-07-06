using Drivious.Enums;
using Drivious.Models.BaseModels;

namespace Drivious.Models
{
    public class Expense : BaseEntity
    {
        public Guid VehicleId { get; set; }              // Xərc hansı maşına aiddir

        public ExpenseCategory Category { get; set; }    // Xərcin kateqoriyası

        public decimal Amount { get; set; }              // Xərc məbləği

        public DateTime ExpenseDate { get; set; }        // Xərcin tarixi

        public string Description { get; set; }          // Xərc haqqında qeyd

        public Vehicle Vehicle { get; set; }             // Əlaqəli maşın
    }
}
