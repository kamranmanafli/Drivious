using Drivious.DTOs.Auth;
using FluentValidation;

namespace Drivious.Validators.Auth
{
    public class RegisterDTOValidator : AbstractValidator<RegisterDTO>
    {
        public RegisterDTOValidator()
        {
            RuleFor(x => x.UserName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Username is required.")
                .MinimumLength(3)
                .WithMessage("Username must be at least 3 characters.")
                .MaximumLength(50)
                .WithMessage("Username cannot exceed 50 characters.")
                // Identity rejects anything outside this set by default, and it does so
                // with a message about allowed characters rather than about the field.
                .Matches("^[a-zA-Z0-9._@+-]+$")
                .WithMessage("Username may only contain letters, digits and . _ @ + -");

            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Email is required.")
                .EmailAddress()
                .WithMessage("Email is not a valid address.")
                .MaximumLength(100)
                .WithMessage("Email cannot exceed 100 characters.");

            // Mirrors the Identity password policy so the caller sees one clear list
            // of problems instead of a second round of errors from the user manager.
            RuleFor(x => x.Password)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Password is required.")
                .MinimumLength(6)
                .WithMessage("Password must be at least 6 characters.")
                .Matches("[A-Z]")
                .WithMessage("Password must contain an uppercase letter.")
                .Matches("[a-z]")
                .WithMessage("Password must contain a lowercase letter.")
                .Matches("[0-9]")
                .WithMessage("Password must contain a digit.")
                .Matches("[^a-zA-Z0-9]")
                .WithMessage("Password must contain a non-alphanumeric character.");

            RuleFor(x => x.ConfirmPassword)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Password confirmation is required.")
                .Equal(x => x.Password)
                .WithMessage("Passwords do not match.");
        }
    }
}
