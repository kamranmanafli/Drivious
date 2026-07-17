using Drivious.DTOs.Maintenance;
using FluentValidation;

namespace Drivious.Validators.Maintenance
{
    public class MaintenanceUpdateDTOValidator : AbstractValidator<MaintenanceUpdateDTO>
    {
        public MaintenanceUpdateDTOValidator()
        {
            RuleFor(x => x.VehicleId)
                .NotEqual(Guid.Empty)
                .WithMessage("Vehicle is required.")
                .When(x => x.VehicleId.HasValue);

            RuleFor(x => x.ServiceType)
                .IsInEnum()
                .WithMessage("Invalid service type.")
                .When(x => x.ServiceType.HasValue);

            RuleFor(x => x.Description)
                .MinimumLength(5)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.Description));

            RuleFor(x => x.Cost)
                .GreaterThan(0)
                .WithMessage("Cost must be greater than 0.")
                .When(x => x.Cost.HasValue);

            RuleFor(x => x.MaintenanceDate)
                .LessThanOrEqualTo(DateTime.Today)
                .WithMessage("Maintenance date cannot be in the future.")
                .When(x => x.MaintenanceDate.HasValue);

            RuleFor(x => x.NextMaintenanceDate)
                .GreaterThan(x => x.MaintenanceDate!.Value)
                .When(x => x.MaintenanceDate.HasValue && x.NextMaintenanceDate.HasValue)
                .WithMessage("Next maintenance date must be later than maintenance date.");

            RuleFor(x => x.Mileage)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Mileage cannot be negative.")
                .When(x => x.Mileage.HasValue);

            RuleFor(x => x.ServiceCenter)
                .MinimumLength(2)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.ServiceCenter));
        }
    }
}
