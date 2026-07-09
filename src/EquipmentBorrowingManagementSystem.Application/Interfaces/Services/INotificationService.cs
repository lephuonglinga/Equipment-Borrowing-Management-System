using EquipmentBorrowingManagementSystem.Application.Common;
using EquipmentBorrowingManagementSystem.Application.DTOs.Notifications;
using EquipmentBorrowingManagementSystem.Domain.Enums;

namespace EquipmentBorrowingManagementSystem.Application.Interfaces.Services;

public interface INotificationService
{
    Task NotifyAsync(int userId, string title, string message, NotificationType type);
    Task<Result<List<NotificationDto>>> GetMyNotificationsAsync();
    Task<Result> MarkAsReadAsync(int notificationId);
}
