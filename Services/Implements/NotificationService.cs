using Drivious.Data;
using Drivious.DTOs.Notification;
using Drivious.Models;
using Drivious.Responses;
using Drivious.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Drivious.Services.Implements
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;

        public NotificationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<object>> CreateAsync(NotificationCreateDTO dto)
        {
            Notification notification = new()
            {
                CreatedAt = DateTime.Now,

                Title = dto.Title,
                Message = dto.Message,
                Type = dto.Type,
                IsRead = dto.IsRead,
                NotificationDate = dto.NotificationDate,
            };

            var result = await _context.Notifications.AddAsync(notification);

            if (result.State != EntityState.Added)
            {
                return new ApiResponse<object>(
                    false,
                    "Notification could not be created.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Notification could not be saved.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Notification created successfully.",
                null
            );
        }

        public async Task<ApiResponse<List<NotificationGetDTO>>> GetAllAsync()
        {
            var notifications = await _context.Notifications.ToListAsync();

            var dtos = notifications.Select(n => new NotificationGetDTO
            {
                Id = n.Id,

                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                IsRead = n.IsRead,
                NotificationDate = n.NotificationDate,

                CreatedAt = n.CreatedAt,
                UpdatedAt = n.UpdatedAt,
                DeletedAt = n.DeletedAt,
                IsDeleted = n.IsDeleted
            }).ToList();

            return new ApiResponse<List<NotificationGetDTO>>(
                true,
                "Notifications retrieved successfully.",
                dtos
            );
        }

        public async Task<ApiResponse<NotificationGetDTO>> GetAsync(Guid id)
        {
            var notification = await _context.Notifications.FindAsync(id);

            if (notification == null)
            {
                return new ApiResponse<NotificationGetDTO>(
                    false,
                    "Notification not found.",
                    null
                );
            }

            var dto = new NotificationGetDTO
            {
                Id = notification.Id,

                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                IsRead = notification.IsRead,
                NotificationDate = notification.NotificationDate,

                CreatedAt = notification.CreatedAt,
                UpdatedAt = notification.UpdatedAt,
                DeletedAt = notification.DeletedAt,
                IsDeleted = notification.IsDeleted
            };

            return new ApiResponse<NotificationGetDTO>(
                true,
                "Notification retrieved successfully.",
                dto
            );
        }

        public async Task<ApiResponse<object>> RemoveAsync(Guid id)
        {
            var notification = await _context.Notifications.FindAsync(id);

            if (notification == null)
            {
                return new ApiResponse<object>(
                    false,
                    "Notification not found.",
                    null
                );
            }

            var result = _context.Notifications.Remove(notification);

            if (result.State != EntityState.Deleted)
            {
                return new ApiResponse<object>(
                    false,
                    "Notification could not be deleted.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Notification could not be deleted.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Notification deleted successfully.",
                null
            );
        }

        public async Task<ApiResponse<object>> ToggleAsync(Guid id)
        {
            var notification = await _context.Notifications.FindAsync(id);

            if (notification == null)
            {
                return new ApiResponse<object>(
                    false,
                    "Notification not found.",
                    null
                );
            }

            notification.IsDeleted = !notification.IsDeleted;
            notification.DeletedAt = notification.IsDeleted ? DateTime.Now : null;

            var result = _context.Notifications.Update(notification);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse<object>(
                    false,
                    "Notification status could not be changed.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Notification status could not be changed.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Notification status changed successfully.",
                null
            );
        }

        public async Task<ApiResponse<object>> UpdateAsync(Guid id, NotificationUpdateDTO dto)
        {
            var notification = await _context.Notifications.FindAsync(id);

            if (notification == null)
            {
                return new ApiResponse<object>(
                    false,
                    "Notification not found.",
                    null
                );
            }

            notification.Title = dto.Title ?? notification.Title;
            notification.Message = dto.Message ?? notification.Message;
            notification.Type = dto.Type ?? notification.Type;
            notification.IsRead = dto.IsRead ?? notification.IsRead;
            notification.NotificationDate = dto.NotificationDate ?? notification.NotificationDate;

            notification.UpdatedAt = DateTime.Now;

            var result = _context.Notifications.Update(notification);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse<object>(
                    false,
                    "Notification could not be updated.",
                    null
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse<object>(
                    false,
                    "Notification could not be updated.",
                    null
                );
            }

            return new ApiResponse<object>(
                true,
                "Notification updated successfully.",
                null
            );
        }
    }
}
