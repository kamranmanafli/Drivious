using Drivious.DTOs.Insurance;
using FluentValidation;

namespace Drivious.Validators.Insurance
{
    public class InsuranceUpdateDTOValidator : AbstractValidator<InsuranceUpdateDTO>
    {
        public InsuranceUpdateDTOValidator()
        {
            RuleFor(x => x.VehicleId)
                .NotEqual(Guid.Empty)
                .WithMessage("Vehicle is required.")
                .When(x => x.VehicleId.HasValue);

            RuleFor(x => x.CompanyName)
                .MinimumLength(2)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.CompanyName));

            RuleFor(x => x.PolicyNumber)
                .MinimumLength(3)
                .MaximumLength(50)
                .When(x => !string.IsNullOrWhiteSpace(x.PolicyNumber));

            RuleFor(x => x.StartDate)
                .LessThanOrEqualTo(x => x.EndDate!.Value)
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
                .WithMessage("Start date cannot be later than end date.");

            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate!.Value)
                .When(x => x.StartDate.HasValue && x.EndDate.HasValue)
                .WithMessage("End date must be later than start date.");

            RuleFor(x => x.Price)
                .GreaterThan(0)
                .WithMessage("Price must be greater than 0.")
                .When(x => x.Price.HasValue);
        }
    }
}
