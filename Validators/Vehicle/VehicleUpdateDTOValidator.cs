using Drivious.DTOs.Vehicle;
using FluentValidation;

namespace Drivious.Validators.Vehicle
{
    public class VehicleUpdateDTOValidator : AbstractValidator<VehicleUpdateDTO>
    {
        public VehicleUpdateDTOValidator()
        {
            RuleFor(x => x.Brand)
                .MinimumLength(2)
                .MaximumLength(50)
                .When(x => !string.IsNullOrWhiteSpace(x.Brand));

            RuleFor(x => x.Model)
                .MinimumLength(2)
                .MaximumLength(50)
                .When(x => !string.IsNullOrWhiteSpace(x.Model));

            RuleFor(x => x.Year)
                .InclusiveBetween(1900, DateTime.Now.Year + 1)
                .WithMessage($"Year must be between 1900 and {DateTime.Now.Year + 1}.")
                .When(x => x.Year.HasValue);

            RuleFor(x => x.PlateNumber)
                .MaximumLength(20)
                .When(x => !string.IsNullOrWhiteSpace(x.PlateNumber));

            RuleFor(x => x.VIN)
                .Length(17)
                .WithMessage("VIN must be exactly 17 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.VIN));

            RuleFor(x => x.Color)
                .MaximumLength(30)
                .When(x => !string.IsNullOrWhiteSpace(x.Color));

            RuleFor(x => x.FuelType)
                .IsInEnum()
                .WithMessage("Invalid fuel type.")
                .When(x => x.FuelType.HasValue);

            RuleFor(x => x.Mileage)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Mileage cannot be negative.")
                .When(x => x.Mileage.HasValue);

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Invalid vehicle status.")
                .When(x => x.Status.HasValue);
        }
    }
}
