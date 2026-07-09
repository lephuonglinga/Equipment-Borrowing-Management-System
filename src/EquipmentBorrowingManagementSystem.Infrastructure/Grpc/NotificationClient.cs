using EquipmentBorrowingManagementSystem.Application.Interfaces.Notifications;
using EquipmentBorrowingManagementSystem.Grpc.Contracts;
using Microsoft.Extensions.Logging;

namespace EquipmentBorrowingManagementSystem.Infrastructure.Grpc;

public class NotificationClient : INotificationClient
{
    private readonly EmailNotificationService.EmailNotificationServiceClient _client;
    private readonly ILogger<NotificationClient> _logger;

    public NotificationClient(
        EmailNotificationService.EmailNotificationServiceClient client,
        ILogger<NotificationClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task SendAsync(
        int userId,
        string userEmail,
        string title,
        string message,
        string notificationType,
        CancellationToken cancellationToken = default)
    {
        var reply = await _client.SendAsync(
            new NotificationRequest
            {
                UserId = userId,
                UserEmail = userEmail,
                Title = title,
                Message = message,
                NotificationType = notificationType
            },
            cancellationToken: cancellationToken);

        if (!reply.Success)
        {
            _logger.LogWarning(
                "gRPC notification service returned failure for user {UserId}: {Detail}",
                userId,
                reply.Detail);
        }
    }
}
