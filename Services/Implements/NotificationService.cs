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

        public async Task<ApiResponse> CreateAsync(NotificationCreateDTO dto)
        {
            Notification notification = _mapper.Map<Notification>(dto);

            notification.CreatedAt = DateTime.Now;

            var result = await _context.Notifications.AddAsync(notification);

            if (result.State != EntityState.Added)
            {
                return new ApiResponse(
                    false,
                    "Notification could not be created."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Notification could not be saved."
                );
            }

            return new ApiResponse(
                true,
                "Notification created successfully."
            );
        }

        public async Task<ApiResponse<List<NotificationGetDTO>>> GetAllAsync()
        {
            var notifications = await _context.Notifications
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .ToListAsync();

            var dtos = _mapper.Map<List<NotificationGetDTO>>(notifications);

            return new ApiResponse<List<NotificationGetDTO>>(
                true,
                "Notifications retrieved successfully.",
                dtos
            );
        }

        public async Task<ApiResponse<List<NotificationGetDTO>>> GetDeletedAsync()
        {
            var notifications = await _context.Notifications
                .AsNoTracking()
                .Where(x => x.IsDeleted)
                .ToListAsync();

            var dtos = _mapper.Map<List<NotificationGetDTO>>(notifications);

            return new ApiResponse<List<NotificationGetDTO>>(
                true,
                "Deleted notifications retrieved successfully.",
                dtos
            );
        }

        public async Task<ApiResponse<NotificationGetDTO>> GetAsync(Guid id)
        {
            var notification = await _context.Notifications
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

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

        public async Task<ApiResponse> RemoveAsync(Guid id)
        {
            var notification = await _context.Notifications.FindAsync(id);

            if (notification == null)
            {
                return new ApiResponse(
                    false,
                    "Notification not found."
                );
            }

            var result = _context.Notifications.Remove(notification);

            if (result.State != EntityState.Deleted)
            {
                return new ApiResponse(
                    false,
                    "Notification could not be deleted."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Notification could not be deleted."
                );
            }

            return new ApiResponse(
                true,
                "Notification deleted successfully."
            );
        }

        public async Task<ApiResponse> ToggleAsync(Guid id)
        {
            var notification = await _context.Notifications.FindAsync(id);

            if (notification == null)
            {
                return new ApiResponse(
                    false,
                    "Notification not found."
                );
            }

            notification.IsDeleted = !notification.IsDeleted;
            notification.DeletedAt = notification.IsDeleted ? DateTime.Now : null;

            var result = _context.Notifications.Update(notification);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse(
                    false,
                    "Notification status could not be changed."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Notification status could not be changed."
                );
            }

            return new ApiResponse(
                true,
                "Notification status changed successfully."
            );
        }

        public async Task<ApiResponse> UpdateAsync(Guid id, NotificationUpdateDTO dto)
        {
            var notification = await _context.Notifications.FindAsync(id);

            if (notification == null)
            {
                return new ApiResponse(
                    false,
                    "Notification not found."
                );
            }

            _mapper.Map(dto, notification);

            notification.UpdatedAt = DateTime.Now;

            var result = _context.Notifications.Update(notification);

            if (result.State != EntityState.Modified)
            {
                return new ApiResponse(
                    false,
                    "Notification could not be updated."
                );
            }

            var saveCount = await _context.SaveChangesAsync();

            if (saveCount <= 0)
            {
                return new ApiResponse(
                    false,
                    "Notification could not be updated."
                );
            }

            return new ApiResponse(
                true,
                "Notification updated successfully."
            );
        }
    }
}
