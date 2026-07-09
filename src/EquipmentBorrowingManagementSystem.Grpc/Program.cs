using EquipmentBorrowingManagementSystem.Grpc.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<NotificationGrpcService>();
app.MapGet("/", () => "Equipment Borrowing gRPC NotificationService is running.");

app.Run();
