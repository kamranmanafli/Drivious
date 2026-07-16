using Drivious.DTOs.Driver;
using FluentValidation;

namespace Drivious.Validators.Driver
{

    public class DriverCreateDTOValidator : AbstractValidator<DriverCreateDTO>
    {
        public DriverCreateDTOValidator()
        {
            RuleFor(x => x.FirstName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("First name is required.")
                .MinimumLength(2).WithMessage("First name must be at least 2 characters.")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters.");

            RuleFor(x => x.LastName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Last name is required.")
                .MinimumLength(2).WithMessage("Last name must be at least 2 characters.")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters.");

            RuleFor(x => x.PhoneNumber)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^\+994\d{9}$|^0\d{9}$")
                .WithMessage("Phone number format is invalid.");

            RuleFor(x => x.Email)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email address.");

            RuleFor(x => x.IdentityNumber)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Identity number is required.");

            RuleFor(x => x.DriverLicenseNumber)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Driver license number is required.");

            RuleFor(x => x.LicenseExpireDate)
                .GreaterThan(DateTime.Today)
                .WithMessage("License expiration date must be in the future.");

            RuleFor(x => x.BirthDate)
                .LessThan(DateTime.Today)
                .WithMessage("Birth date is invalid.");

            RuleFor(x => x.HireDate)
                .LessThanOrEqualTo(DateTime.Today)
                .WithMessage("Hire date cannot be in the future.");

            RuleFor(x => x.Address)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Address is required.")
                .MinimumLength(5).WithMessage("Address must be at least 5 characters.")
                .MaximumLength(200).WithMessage("Address cannot exceed 200 characters.");

            RuleFor(x => x.Image)
                .NotNull()
                .WithMessage("Driver image is required.");
        }
    }
}
