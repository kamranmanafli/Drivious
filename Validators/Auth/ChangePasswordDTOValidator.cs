using Drivious.DTOs.Auth;
using FluentValidation;

namespace Drivious.Validators.Auth
{
    public class ChangePasswordDTOValidator : AbstractValidator<ChangePasswordDTO>
    {
        public ChangePasswordDTOValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty()
                .WithMessage("Current password is required.");

            RuleFor(x => x.NewPassword)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("New password is required.")
                .MinimumLength(6)
                .WithMessage("New password must be at least 6 characters.")
                .Matches("[A-Z]")
                .WithMessage("New password must contain an uppercase letter.")
                .Matches("[a-z]")
                .WithMessage("New password must contain a lowercase letter.")
                .Matches("[0-9]")
                .WithMessage("New password must contain a digit.")
                .Matches("[^a-zA-Z0-9]")
                .WithMessage("New password must contain a non-alphanumeric character.")
                .NotEqual(x => x.CurrentPassword)
                .WithMessage("New password must differ from the current one.");

            RuleFor(x => x.ConfirmNewPassword)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Password confirmation is required.")
                .Equal(x => x.NewPassword)
                .WithMessage("New passwords do not match.");
        }
    }
}
