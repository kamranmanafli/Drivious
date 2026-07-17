using Drivious.DTOs.Expense;
using FluentValidation;

namespace Drivious.Validators.Expense
{
    public class ExpenseUpdateDTOValidator : AbstractValidator<ExpenseUpdateDTO>
    {
        public ExpenseUpdateDTOValidator()
        {
            RuleFor(x => x.VehicleId)
                .NotEqual(Guid.Empty)
                .WithMessage("Vehicle is required.")
                .When(x => x.VehicleId.HasValue);

            RuleFor(x => x.Category)
                .IsInEnum()
                .WithMessage("Invalid expense category.")
                .When(x => x.Category.HasValue);

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Amount must be greater than 0.")
                .When(x => x.Amount.HasValue);

            RuleFor(x => x.ExpenseDate)
                .LessThanOrEqualTo(DateTime.Today)
                .WithMessage("Expense date cannot be in the future.")
                .When(x => x.ExpenseDate.HasValue);

            RuleFor(x => x.Description)
                .MinimumLength(5)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.Description));
        }
    }
}
