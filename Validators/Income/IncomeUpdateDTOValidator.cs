using Drivious.DTOs.Income;
using FluentValidation;

namespace Drivious.Validators.Income
{
    public class IncomeUpdateDTOValidator : AbstractValidator<IncomeUpdateDTO>
    {
        public IncomeUpdateDTOValidator()
        {
            RuleFor(x => x.VehicleId)
                .NotEqual(Guid.Empty)
                .WithMessage("Vehicle is required.")
                .When(x => x.VehicleId.HasValue);

            RuleFor(x => x.DriverId)
                .NotEqual(Guid.Empty)
                .WithMessage("Driver is required.")
                .When(x => x.DriverId.HasValue);

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Amount must be greater than 0.")
                .When(x => x.Amount.HasValue);

            RuleFor(x => x.IncomeDate)
                .LessThanOrEqualTo(DateTime.Today)
                .WithMessage("Income date cannot be in the future.")
                .When(x => x.IncomeDate.HasValue);

            RuleFor(x => x.Description)
                .MinimumLength(5)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.Description));
        }
    }
}
