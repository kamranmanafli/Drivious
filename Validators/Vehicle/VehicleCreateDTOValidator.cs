using Drivious.DTOs.Vehicle;
using FluentValidation;

namespace Drivious.Validators.Vehicle
{
    public class VehicleCreateDTOValidator : AbstractValidator<VehicleCreateDTO>
    {
        public VehicleCreateDTOValidator()
        {
            RuleFor(x => x.Image)
                .NotNull()
                .WithMessage("Vehicle image is required.");

            RuleFor(x => x.Brand)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Brand is required.")
                .MinimumLength(2)
                .WithMessage("Brand must be at least 2 characters.")
                .MaximumLength(50)
                .WithMessage("Brand cannot exceed 50 characters.");

            RuleFor(x => x.Model)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Model is required.")
                .MinimumLength(2)
                .WithMessage("Model must be at least 2 characters.")
                .MaximumLength(50)
                .WithMessage("Model cannot exceed 50 characters.");

            RuleFor(x => x.Year)
                .InclusiveBetween(1900, DateTime.Now.Year + 1)
                .WithMessage($"Year must be between 1900 and {DateTime.Now.Year + 1}.");

            RuleFor(x => x.PlateNumber)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Plate number is required.")
                .MaximumLength(20)
                .WithMessage("Plate number cannot exceed 20 characters.");

            RuleFor(x => x.VIN)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("VIN is required.")
                .Length(17)
                .WithMessage("VIN must be exactly 17 characters.");

            RuleFor(x => x.Color)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Color is required.")
                .MaximumLength(30)
                .WithMessage("Color cannot exceed 30 characters.");

            RuleFor(x => x.FuelType)
                .IsInEnum()
                .WithMessage("Invalid fuel type.");

            RuleFor(x => x.Mileage)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Mileage cannot be negative.");

            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Invalid vehicle status.");
        }
    }
}
