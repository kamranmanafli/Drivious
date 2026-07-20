using AutoMapper;
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
        private readonly IMapper _mapper;

        public NotificationService(
            AppDbContext context,
            IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ApiResponse<object>> CreateAsync(NotificationCreateDTO dto)
        {
            Notification notification = _mapper.Map<Notification>(dto);

            notification.CreatedAt = DateTime.Now;

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

            var dtos = _mapper.Map<List<NotificationGetDTO>>(notifications);

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

            var dto = _mapper.Map<NotificationGetDTO>(notification);

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

            _mapper.Map(dto, notification);

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
