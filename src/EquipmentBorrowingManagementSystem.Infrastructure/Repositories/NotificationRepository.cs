using EquipmentBorrowingManagementSystem.Application.Interfaces.Repositories;
using EquipmentBorrowingManagementSystem.Domain.Entities;
using EquipmentBorrowingManagementSystem.Grpc.Contracts;
using EquipmentBorrowingManagementSystem.Infrastructure.Data;
using Grpc.Net.Client;

namespace EquipmentBorrowingManagementSystem.Infrastructure.Repositories;

public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
{
    public NotificationRepository(AppDbContext context) : base(context)
    {
    }

    public async Task SendGrpcAsync(
        int userId,
        string userEmail,
        string title,
        string message,
        string notificationType,
        CancellationToken cancellationToken = default)
    {
        // Demo: allow plaintext HTTP/2 to local gRPC server
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

        using var channel = GrpcChannel.ForAddress("http://localhost:5272");
        var client = new EmailNotificationService.EmailNotificationServiceClient(channel);

        await client.SendAsync(
            new NotificationRequest
            {
                UserId = userId,
                UserEmail = userEmail,
                Title = title,
                Message = message,
                NotificationType = notificationType
            },
            cancellationToken: cancellationToken);
    }
}
