using Drivious.DTOs.Notification;
using FluentValidation;

namespace Drivious.Validators.Notification
{
    public class NotificationUpdateDTOValidator : AbstractValidator<NotificationUpdateDTO>
    {
        public NotificationUpdateDTOValidator()
        {
            RuleFor(x => x.Title)
                .MinimumLength(3)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.Title));

            RuleFor(x => x.Message)
                .MinimumLength(5)
                .MaximumLength(1000)
                .When(x => !string.IsNullOrWhiteSpace(x.Message));

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("Invalid notification type.")
                .When(x => x.Type.HasValue);

            RuleFor(x => x.NotificationDate)
                .LessThanOrEqualTo(DateTime.Now)
                .WithMessage("Notification date cannot be in the future.")
                .When(x => x.NotificationDate.HasValue);
        }
    }
}
