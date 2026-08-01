using Drivious.DTOs.Auth;
using FluentValidation;

namespace Drivious.Validators.Auth
{
    public class LoginDTOValidator : AbstractValidator<LoginDTO>
    {
        public LoginDTOValidator()
        {
            // Only presence is checked here. Telling a caller that a password is too
            // short before it is even compared would leak the rules to whoever asks.
            RuleFor(x => x.UserName)
                .NotEmpty()
                .WithMessage("Username is required.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required.");
        }
    }
}
