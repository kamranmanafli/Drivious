using Drivious.DTOs.Driver;
using FluentValidation;

namespace Drivious.Validators.Driver
{
    public class DriverUpdateDTOValidator : AbstractValidator<DriverUpdateDTO>
    {
        public DriverUpdateDTOValidator()
        {
            RuleFor(x => x.FirstName)
                .MinimumLength(2)
                .MaximumLength(50)
                .When(x => !string.IsNullOrWhiteSpace(x.FirstName));

            RuleFor(x => x.LastName)
                .MinimumLength(2)
                .MaximumLength(50)
                .When(x => !string.IsNullOrWhiteSpace(x.LastName));

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+994\d{9}$|^0\d{9}$")
                .WithMessage("Phone number format is invalid.")
                .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessage("Invalid email address.")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.IdentityNumber)
                .MinimumLength(3)
                .MaximumLength(20)
                .When(x => !string.IsNullOrWhiteSpace(x.IdentityNumber));

            RuleFor(x => x.DriverLicenseNumber)
                .MinimumLength(3)
                .MaximumLength(30)
                .When(x => !string.IsNullOrWhiteSpace(x.DriverLicenseNumber));

            RuleFor(x => x.LicenseExpireDate)
                .GreaterThan(DateTime.Today)
                .WithMessage("License expiration date must be in the future.")
                .When(x => x.LicenseExpireDate.HasValue);

            RuleFor(x => x.BirthDate)
                .LessThan(DateTime.Today)
                .WithMessage("Birth date is invalid.")
                .When(x => x.BirthDate.HasValue);

            RuleFor(x => x.HireDate)
                .LessThanOrEqualTo(DateTime.Today)
                .WithMessage("Hire date cannot be in the future.")
                .When(x => x.HireDate.HasValue);

            RuleFor(x => x.Address)
                .MinimumLength(5)
                .MaximumLength(200)
                .When(x => !string.IsNullOrWhiteSpace(x.Address));
        }
    }
}
