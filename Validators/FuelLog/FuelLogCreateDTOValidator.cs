using Drivious.DTOs.FuelLog;
using FluentValidation;

namespace Drivious.Validators.FuelLog
{
    public class FuelLogCreateDTOValidator : AbstractValidator<FuelLogCreateDTO>
    {
        public FuelLogCreateDTOValidator()
        {
            RuleFor(x => x.VehicleId)
                .NotEqual(Guid.Empty)
                .WithMessage("Vehicle is required.");

            RuleFor(x => x.Liters)
                .GreaterThan(0)
                .WithMessage("Fuel liters must be greater than 0.");

            RuleFor(x => x.Price)
                .GreaterThan(0)
                .WithMessage("Price must be greater than 0.");

            RuleFor(x => x.FuelDate)
                .LessThanOrEqualTo(DateTime.Today)
                .WithMessage("Fuel date cannot be in the future.");

            RuleFor(x => x.Mileage)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Mileage cannot be negative.");

            RuleFor(x => x.StationName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Station name is required.")
                .MinimumLength(2)
                .WithMessage("Station name must be at least 2 characters.")
                .MaximumLength(100)
                .WithMessage("Station name cannot exceed 100 characters.");
        }
    }
}
