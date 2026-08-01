using Drivious.Constants;
using Drivious.DTOs.Auth;
using FluentValidation;

namespace Drivious.Validators.Auth
{
    public class AssignRoleDTOValidator : AbstractValidator<AssignRoleDTO>
    {
        public AssignRoleDTOValidator()
        {
            RuleFor(x => x.UserName)
                .NotEmpty()
                .WithMessage("Username is required.");

            RuleFor(x => x.Role)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Role is required.")
                .Must(x => AppRoles.All.Contains(x, StringComparer.OrdinalIgnoreCase))
                .WithMessage($"Role must be one of: {string.Join(", ", AppRoles.All)}.");
        }
    }
}
