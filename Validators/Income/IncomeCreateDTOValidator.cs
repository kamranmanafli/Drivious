using Drivious.DTOs.Income;
using FluentValidation;

namespace Drivious.Validators.Income
{
    public class IncomeCreateDTOValidator : AbstractValidator<IncomeCreateDTO>
    {
        public IncomeCreateDTOValidator()
        {
            RuleFor(x => x.VehicleId)
                .NotEqual(Guid.Empty)
                .WithMessage("Vehicle is required.");

            RuleFor(x => x.DriverId)
                .NotEqual(Guid.Empty)
                .WithMessage("Driver is required.");

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Amount must be greater than 0.");

            RuleFor(x => x.IncomeDate)
                .LessThanOrEqualTo(DateTime.Today)
                .WithMessage("Income date cannot be in the future.");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Description cannot exceed 500 characters.");
        }
    }
}
