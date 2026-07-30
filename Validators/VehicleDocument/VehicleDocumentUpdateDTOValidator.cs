using Drivious.DTOs.VehicleDocument;
using Drivious.Enums;
using Drivious.Extensions;
using FluentValidation;

namespace Drivious.Validators.VehicleDocument
{
    public class VehicleDocumentUpdateDTOValidator : AbstractValidator<VehicleDocumentUpdateDTO>
    {
        public VehicleDocumentUpdateDTOValidator()
        {
            RuleFor(x => x.File)
                .Must(x => x!.CheckFileSize(FileSize.MB, 10))
                .WithMessage("Document cannot exceed 10 MB.")
                .When(x => x.File != null);

            RuleFor(x => x.VehicleId)
                .NotEqual(Guid.Empty)
                .WithMessage("Vehicle is required.")
                .When(x => x.VehicleId.HasValue);

            RuleFor(x => x.Title)
                .MinimumLength(3)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.Title));

            RuleFor(x => x.DocumentType)
                .IsInEnum()
                .WithMessage("Invalid document type.")
                .When(x => x.DocumentType.HasValue);

            RuleFor(x => x.UploadDate)
                .LessThanOrEqualTo(DateTime.Now)
                .WithMessage("Upload date cannot be in the future.")
                .When(x => x.UploadDate.HasValue);
        }
    }
}
