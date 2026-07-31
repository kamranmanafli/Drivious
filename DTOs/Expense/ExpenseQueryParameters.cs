using Drivious.DTOs.Common;
using Drivious.Enums;

namespace Drivious.DTOs.Expense
{
    public class ExpenseQueryParameters : QueryParameters
    {
        /// <summary>Matched against the description and the vehicle's plate number.</summary>
        public Guid? VehicleId { get; set; }

        public ExpenseCategory? Category { get; set; }

        public DateTime? From { get; set; }

        public DateTime? To { get; set; }

        public decimal? MinAmount { get; set; }

        public decimal? MaxAmount { get; set; }
    }
}
