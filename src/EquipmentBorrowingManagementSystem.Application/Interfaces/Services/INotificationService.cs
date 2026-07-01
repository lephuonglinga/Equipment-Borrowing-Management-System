using EquipmentBorrowingManagementSystem.Domain.Enums;

namespace EquipmentBorrowingManagementSystem.Application.Interfaces.Services;

public interface INotificationService
{
    Task NotifyAsync(int userId, string title, string message, NotificationType type);
}
