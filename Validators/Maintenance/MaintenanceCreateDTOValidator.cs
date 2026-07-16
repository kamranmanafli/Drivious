using Drivious.DTOs.Maintenance;
using FluentValidation;

namespace Drivious.Validators.Maintenance
{
    public class MaintenanceCreateDTOValidator : AbstractValidator<MaintenanceCreateDTO>
    {
        public MaintenanceCreateDTOValidator()
        {
            RuleFor(x => x.VehicleId)
                .NotEqual(Guid.Empty)
                .WithMessage("Vehicle is required.");

            RuleFor(x => x.ServiceType)
                .IsInEnum()
                .WithMessage("Invalid service type.");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Description cannot exceed 500 characters.");

            RuleFor(x => x.Cost)
                .GreaterThan(0)
                .WithMessage("Cost must be greater than 0.");

            RuleFor(x => x.MaintenanceDate)
                .LessThanOrEqualTo(DateTime.Today)
                .WithMessage("Maintenance date cannot be in the future.");

            RuleFor(x => x.NextMaintenanceDate)
                .GreaterThan(x => x.MaintenanceDate)
                .When(x => x.NextMaintenanceDate.HasValue)
                .WithMessage("Next maintenance date must be later than maintenance date.");

            RuleFor(x => x.Mileage)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Mileage cannot be negative.");

            RuleFor(x => x.ServiceCenter)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Service center is required.")
                .MinimumLength(2)
                .WithMessage("Service center must be at least 2 characters.")
                .MaximumLength(100)
                .WithMessage("Service center cannot exceed 100 characters.");
        }
    }
}
