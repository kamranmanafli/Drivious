using Drivious.DTOs.VehicleAssignment;
using FluentValidation;

namespace Drivious.Validators.VehicleAssignment
{
    public class VehicleAssignmentUpdateDTOValidator : AbstractValidator<VehicleAssignmentUpdateDTO>
    {
        public VehicleAssignmentUpdateDTOValidator()
        {
            RuleFor(x => x.VehicleId)
                .NotEqual(Guid.Empty)
                .WithMessage("Vehicle is required.")
                .When(x => x.VehicleId.HasValue);

            RuleFor(x => x.DriverId)
                .NotEqual(Guid.Empty)
                .WithMessage("Driver is required.")
                .When(x => x.DriverId.HasValue);

            RuleFor(x => x.AssignedDate)
                .LessThanOrEqualTo(DateTime.Today)
                .WithMessage("Assigned date cannot be in the future.")
                .When(x => x.AssignedDate.HasValue);

            RuleFor(x => x.ReturnedDate)
                .GreaterThanOrEqualTo(x => x.AssignedDate!.Value)
                .When(x => x.AssignedDate.HasValue && x.ReturnedDate.HasValue)
                .WithMessage("Returned date cannot be earlier than assigned date.");

            RuleFor(x => x.Note)
                .MaximumLength(500)
                .WithMessage("Note cannot exceed 500 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Note));
        }
    }
}
