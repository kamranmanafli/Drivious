using Drivious.DTOs.VehicleDocumnet;
using FluentValidation;

namespace Drivious.Validators.VehicleDocument
{
    public class VehicleDocumentCreateDTOValidator : AbstractValidator<VehicleDocumentCreateDTO>
    {
        public VehicleDocumentCreateDTOValidator()
        {
            RuleFor(x => x.VehicleId)
                .NotEqual(Guid.Empty)
                .WithMessage("Vehicle is required.");

            RuleFor(x => x.Title)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Title is required.")
                .MinimumLength(3)
                .WithMessage("Title must be at least 3 characters.")
                .MaximumLength(100)
                .WithMessage("Title cannot exceed 100 characters.");

            RuleFor(x => x.DocumentType)
                .IsInEnum()
                .WithMessage("Invalid document type.");

            RuleFor(x => x.File)
                .NotNull()
                .WithMessage("Document file is required.");
        }
    }
}
