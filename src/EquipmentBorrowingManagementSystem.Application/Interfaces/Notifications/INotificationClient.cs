namespace EquipmentBorrowingManagementSystem.Application.Interfaces.Notifications;

public interface INotificationClient
{
    Task SendAsync(
        int userId,
        string userEmail,
        string title,
        string message,
        string notificationType,
        CancellationToken cancellationToken = default);
}
