using Drivious.Data;
using Drivious.DTOs.Notification;
using Drivious.Models;
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

        public async Task<bool> CreateAsync(NotificationCreateDTO dto)
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
                return false;

            var saveCount = await _context.SaveChangesAsync();

            return saveCount > 0;
        }

        public async Task<List<NotificationGetDTO>> GetAllAsync()
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

            return dtos;
        }

        public async Task<NotificationGetDTO> GetAsync(Guid id)
        {
            var notification = await _context.Notifications.FindAsync(id);

            if (notification == null)
            {
                throw new Exception("Notification not found!");
            }

            var dto = new NotificationGetDTO()
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

            return dto;
        }

        public async Task<bool> RemoveAsync(Guid id)
        {
            var notification = await _context.Notifications.FindAsync(id);

            if (notification == null)
                return false;

            var result = _context.Notifications.Remove(notification);

            if (result.State != EntityState.Deleted)
                return false;

            var saveCount = await _context.SaveChangesAsync();

            return saveCount > 0;
        }

        public async Task<bool> ToggleAsync(Guid id)
        {
            var notification = await _context.Notifications.FindAsync(id);

            if (notification == null)
                return false;

            notification.IsDeleted = !notification.IsDeleted;

            notification.DeletedAt = notification.IsDeleted ? DateTime.Now : null;

            var result = _context.Notifications.Update(notification);

            if (result.State != EntityState.Modified)
                return false;

            var saveCount = await _context.SaveChangesAsync();

            return saveCount > 0;
        }

        public async Task<bool> UpdateAsync(Guid id, NotificationUpdateDTO dto)
        {
            var notification = await _context.Notifications.FindAsync(id);

            if (notification == null)
                return false;

            notification.Title = dto.Title ?? notification.Title;

            notification.Message = dto.Message ?? notification.Message;

            notification.Type = dto.Type ?? notification.Type;

            notification.IsRead = dto.IsRead ?? notification.IsRead;

            notification.NotificationDate = dto.NotificationDate ?? notification.NotificationDate;

            notification.UpdatedAt = DateTime.Now;

            var result = _context.Notifications.Update(notification);

            if (result.State != EntityState.Modified)
                return false;

            var saveCount = await _context.SaveChangesAsync();

            return saveCount > 0;
        }
    }
}
