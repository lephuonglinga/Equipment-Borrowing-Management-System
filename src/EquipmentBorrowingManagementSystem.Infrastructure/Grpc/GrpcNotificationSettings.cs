namespace EquipmentBorrowingManagementSystem.Infrastructure.Grpc;

public class GrpcNotificationSettings
{
    public const string SectionName = "GrpcNotification";

    public string Address { get; set; } = "http://localhost:5272";
}
