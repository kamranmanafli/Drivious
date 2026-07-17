using Drivious.DTOs.FuelLog;
using FluentValidation;

namespace Drivious.Validators.FuelLog
{
    public class FuelLogUpdateDTOValidator : AbstractValidator<FuelLogUpdateDTO>
    {
        public FuelLogUpdateDTOValidator()
        {
            RuleFor(x => x.VehicleId)
                .NotEqual(Guid.Empty)
                .WithMessage("Vehicle is required.")
                .When(x => x.VehicleId.HasValue);

            RuleFor(x => x.Liters)
                .GreaterThan(0)
                .WithMessage("Fuel liters must be greater than 0.")
                .When(x => x.Liters.HasValue);

            RuleFor(x => x.Price)
                .GreaterThan(0)
                .WithMessage("Price must be greater than 0.")
                .When(x => x.Price.HasValue);

            RuleFor(x => x.FuelDate)
                .LessThanOrEqualTo(DateTime.Today)
                .WithMessage("Fuel date cannot be in the future.")
                .When(x => x.FuelDate.HasValue);

            RuleFor(x => x.Mileage)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Mileage cannot be negative.")
                .When(x => x.Mileage.HasValue);

            RuleFor(x => x.StationName)
                .MinimumLength(2)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.StationName));
        }
    }
}
