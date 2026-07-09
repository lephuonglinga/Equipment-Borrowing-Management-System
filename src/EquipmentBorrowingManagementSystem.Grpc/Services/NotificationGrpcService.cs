using EquipmentBorrowingManagementSystem.Grpc.Contracts;
using Grpc.Core;

namespace EquipmentBorrowingManagementSystem.Grpc.Services;

public class NotificationGrpcService : EmailNotificationService.EmailNotificationServiceBase
{
    private readonly ILogger<NotificationGrpcService> _logger;

    public NotificationGrpcService(ILogger<NotificationGrpcService> logger)
    {
        _logger = logger;
    }

    public override Task<NotificationReply> Send(
        NotificationRequest request,
        ServerCallContext context)
    {
        _logger.LogInformation(
            "[gRPC Email Simulation] To user {UserId} ({Email}) [{Type}]: {Title} — {Message}",
            request.UserId,
            request.UserEmail,
            request.NotificationType,
            request.Title,
            request.Message);

        return Task.FromResult(new NotificationReply
        {
            Success = true,
            Detail = "Email notification simulated successfully."
        });
    }
}
