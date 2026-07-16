using Drivious.DTOs.Insurance;
using FluentValidation;

namespace Drivious.Validators.Insurance
{
    public class InsuranceCreateDTOValidator : AbstractValidator<InsuranceCreateDTO>
    {
        public InsuranceCreateDTOValidator()
        {
            RuleFor(x => x.VehicleId)
                .NotEqual(Guid.Empty)
                .WithMessage("Vehicle is required.");

            RuleFor(x => x.CompanyName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Company name is required.")
                .MinimumLength(2)
                .WithMessage("Company name must be at least 2 characters.")
                .MaximumLength(100)
                .WithMessage("Company name cannot exceed 100 characters.");

            RuleFor(x => x.PolicyNumber)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Policy number is required.")
                .MinimumLength(3)
                .WithMessage("Policy number must be at least 3 characters.")
                .MaximumLength(50)
                .WithMessage("Policy number cannot exceed 50 characters.");

            RuleFor(x => x.StartDate)
                .LessThanOrEqualTo(x => x.EndDate)
                .WithMessage("Start date cannot be later than end date.");

            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate)
                .WithMessage("End date must be later than start date.");

            RuleFor(x => x.Price)
                .GreaterThan(0)
                .WithMessage("Price must be greater than 0.");
        }
    }
}
