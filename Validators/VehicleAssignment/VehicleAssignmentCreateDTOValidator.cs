using Drivious.DTOs.VehicleAssignment;
using FluentValidation;

namespace Drivious.Validators.VehicleAssignment
{
    public class VehicleAssignmentCreateDTOValidator : AbstractValidator<VehicleAssignmentCreateDTO>
    {
        public VehicleAssignmentCreateDTOValidator()
        {
            RuleFor(x => x.VehicleId)
                .NotEqual(Guid.Empty)
                .WithMessage("Vehicle is required.");

            RuleFor(x => x.DriverId)
                .NotEqual(Guid.Empty)
                .WithMessage("Driver is required.");

            RuleFor(x => x.AssignedDate)
                .LessThanOrEqualTo(DateTime.Today)
                .WithMessage("Assigned date cannot be in the future.");

            RuleFor(x => x.ReturnedDate)
                .GreaterThanOrEqualTo(x => x.AssignedDate)
                .When(x => x.ReturnedDate.HasValue)
                .WithMessage("Returned date cannot be earlier than assigned date.");

            RuleFor(x => x.Note)
                .MaximumLength(500)
                .WithMessage("Note cannot exceed 500 characters.");
        }
    }
}
