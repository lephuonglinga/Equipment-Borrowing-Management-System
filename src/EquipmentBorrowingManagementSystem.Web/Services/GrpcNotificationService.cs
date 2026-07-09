using EquipmentBorrowingManagementSystem.Grpc.Contracts;
using EquipmentBorrowingManagementSystem.Web.Models;
using EquipmentBorrowingManagementSystem.Web.Options;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;

namespace EquipmentBorrowingManagementSystem.Web.Services;

public class GrpcNotificationService
{
    private readonly GrpcNotificationOptions _options;

    public GrpcNotificationService(IOptions<GrpcNotificationOptions> options)
    {
        _options = options.Value;
    }

    public async Task<GrpcSendResult> SendAsync(
        int userId,
        string userEmail,
        string title,
        string message,
        string notificationType,
        CancellationToken cancellationToken = default)
    {
        using var channel = GrpcChannel.ForAddress(_options.Address);
        var client = new EmailNotificationService.EmailNotificationServiceClient(channel);

        var reply = await client.SendAsync(
            new NotificationRequest
            {
                UserId = userId,
                UserEmail = userEmail,
                Title = title,
                Message = message,
                NotificationType = notificationType
            },
            cancellationToken: cancellationToken);

        return new GrpcSendResult
        {
            Success = reply.Success,
            Detail = reply.Detail
        };
    }
}
