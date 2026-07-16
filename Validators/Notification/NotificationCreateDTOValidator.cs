using Drivious.DTOs.Notification;
using FluentValidation;

namespace Drivious.Validators.Notification
{
    public class NotificationCreateDTOValidator : AbstractValidator<NotificationCreateDTO>
    {
        public NotificationCreateDTOValidator()
        {
            RuleFor(x => x.Title)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Title is required.")
                .MinimumLength(3)
                .WithMessage("Title must be at least 3 characters.")
                .MaximumLength(100)
                .WithMessage("Title cannot exceed 100 characters.");

            RuleFor(x => x.Message)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Message is required.")
                .MinimumLength(5)
                .WithMessage("Message must be at least 5 characters.")
                .MaximumLength(1000)
                .WithMessage("Message cannot exceed 1000 characters.");

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("Invalid notification type.");

            RuleFor(x => x.NotificationDate)
                .LessThanOrEqualTo(DateTime.Now)
                .WithMessage("Notification date cannot be in the future.");
        }
    }
}
