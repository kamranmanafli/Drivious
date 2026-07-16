using Drivious.DTOs.Expense;
using FluentValidation;

namespace Drivious.Validators.Expense
{
    public class ExpenseCreateDTOValidator : AbstractValidator<ExpenseCreateDTO>
    {
        public ExpenseCreateDTOValidator()
        {
            RuleFor(x => x.VehicleId)
                .NotEqual(Guid.Empty)
                .WithMessage("Vehicle is required.");

            RuleFor(x => x.Category)
                .IsInEnum()
                .WithMessage("Invalid expense category.");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Amount must be greater than 0.");

            RuleFor(x => x.ExpenseDate)
                .LessThanOrEqualTo(DateTime.Today)
                .WithMessage("Expense date cannot be in the future.");

            RuleFor(x => x.Description)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Description is required.")
                .MinimumLength(5)
                .WithMessage("Description must be at least 5 characters.")
                .MaximumLength(500)
                .WithMessage("Description cannot exceed 500 characters.");
        }
    }
}
