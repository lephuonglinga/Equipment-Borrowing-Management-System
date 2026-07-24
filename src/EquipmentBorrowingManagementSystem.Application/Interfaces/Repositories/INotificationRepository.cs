using EquipmentBorrowingManagementSystem.Domain.Entities;

namespace EquipmentBorrowingManagementSystem.Application.Interfaces.Repositories;

public interface INotificationRepository : IGenericRepository<Notification>
{
    Task SendGrpcAsync(
        int userId,
        string userEmail,
        string title,
        string message,
        string notificationType,
        CancellationToken cancellationToken = default);
}
