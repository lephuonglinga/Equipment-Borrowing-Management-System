using EquipmentBorrowingManagementSystem.Application.Common;
using EquipmentBorrowingManagementSystem.Application.DTOs.Notifications;
using EquipmentBorrowingManagementSystem.Application.Interfaces;
using EquipmentBorrowingManagementSystem.Application.Interfaces.Security;
using EquipmentBorrowingManagementSystem.Application.Interfaces.Services;
using EquipmentBorrowingManagementSystem.Domain.Entities;
using EquipmentBorrowingManagementSystem.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace EquipmentBorrowingManagementSystem.Application.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        ILogger<NotificationService> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
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

        try
        {
            await _unitOfWork.Notifications.SendGrpcAsync(
                userId, email, title, message, type.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "gRPC notification failed for user {UserId}. In-app notification was still queued.",
                userId);
        }
    }

    public async Task<Result<List<NotificationDto>>> GetMyNotificationsAsync()
    {
        if (_currentUser.UserId == null)
        {
            return Result<List<NotificationDto>>.Fail(
                "Phiên đăng nhập không hợp lệ.",
                StatusCodes.Status401Unauthorized);
        }

        var rows = (await _unitOfWork.Notifications.GetAllAsync())
            .Where(n => n.UserId == _currentUser.UserId.Value)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type.ToString(),
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt
            })
            .ToList();

        return Result<List<NotificationDto>>.Ok(rows);
    }

    public async Task<Result> MarkAsReadAsync(int notificationId)
    {
        if (_currentUser.UserId == null)
        {
            return Result.Fail("Phiên đăng nhập không hợp lệ.", StatusCodes.Status401Unauthorized);
        }

        var notification = await _unitOfWork.Notifications.GetByIdAsync(notificationId);
        if (notification == null || notification.UserId != _currentUser.UserId.Value)
        {
            return Result.Fail("Không tìm thấy thông báo.", StatusCodes.Status404NotFound);
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            _unitOfWork.Notifications.Update(notification);
            await _unitOfWork.SaveChangesAsync();
        }

        return Result.Ok("Đã đánh dấu đã đọc.");
    }
}
