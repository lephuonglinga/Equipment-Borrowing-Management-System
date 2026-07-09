using EquipmentBorrowingManagementSystem.Application.Interfaces;
using EquipmentBorrowingManagementSystem.Application.Interfaces.Notifications;
using EquipmentBorrowingManagementSystem.Application.Interfaces.Services;
using EquipmentBorrowingManagementSystem.Domain.Entities;
using EquipmentBorrowingManagementSystem.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace EquipmentBorrowingManagementSystem.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationClient _notificationClient;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IUnitOfWork unitOfWork,
        INotificationClient notificationClient,
        ILogger<NotificationService> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationClient = notificationClient;
        _logger = logger;
    }

    public async Task NotifyAsync(int userId, string title, string message, NotificationType type)
    {
        await _unitOfWork.Notifications.AddAsync(new Notification
        {
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            IsRead = false
        });

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        var email = user?.Email ?? string.Empty;

        _ = SendGrpcNotificationAsync(userId, email, title, message, type.ToString());
    }

    private async Task SendGrpcNotificationAsync(
        int userId,
        string userEmail,
        string title,
        string message,
        string notificationType)
    {
        try
        {
            await _notificationClient.SendAsync(userId, userEmail, title, message, notificationType);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "gRPC notification failed for user {UserId}. In-app notification was still queued.",
                userId);
        }
    }
}
