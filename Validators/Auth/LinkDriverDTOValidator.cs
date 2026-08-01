using Drivious.DTOs.Auth;
using FluentValidation;

namespace Drivious.Validators.Auth
{
    public class LinkDriverDTOValidator : AbstractValidator<LinkDriverDTO>
    {
        public LinkDriverDTOValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty()
                .WithMessage("Username is required.");

            // Null is the documented way to unlink, so only an explicitly empty guid
            // is wrong here.
            RuleFor(x => x.DriverId)
                .NotEqual(Guid.Empty)
                .When(x => x.DriverId.HasValue)
                .WithMessage("Driver id is not valid. Omit the field to unlink instead.");
        }
    }
}
