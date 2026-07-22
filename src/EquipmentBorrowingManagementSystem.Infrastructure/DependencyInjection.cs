using EquipmentBorrowingManagementSystem.Application.Interfaces;
using EquipmentBorrowingManagementSystem.Application.Interfaces.Notifications;
using EquipmentBorrowingManagementSystem.Application.Interfaces.Repositories;
using EquipmentBorrowingManagementSystem.Application.Interfaces.Security;
using EquipmentBorrowingManagementSystem.Infrastructure.Data;
using EquipmentBorrowingManagementSystem.Infrastructure.Grpc;
using EquipmentBorrowingManagementSystem.Infrastructure.Repositories;
using EquipmentBorrowingManagementSystem.Infrastructure.Security;
using EquipmentBorrowingManagementSystem.Grpc.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EquipmentBorrowingManagementSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IEquipmentRepository, EquipmentRepository>();
        services.AddScoped<IEquipmentCategoryRepository, EquipmentCategoryRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IBorrowRequestRepository, BorrowRequestRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork.UnitOfWork>();

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.AddHttpContextAccessor();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ICurrentUser, CurrentUser>();

        services.Configure<GrpcNotificationSettings>(configuration.GetSection(GrpcNotificationSettings.SectionName));
        RegisterGrpcNotificationClient(services, configuration);

        return services;
    }

    private static void RegisterGrpcNotificationClient(IServiceCollection services, IConfiguration configuration)
    {
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

        var grpcAddress = configuration
            .GetSection(GrpcNotificationSettings.SectionName)
            .Get<GrpcNotificationSettings>()?.Address ?? "http://localhost:5272";

        services.AddGrpcClient<EmailNotificationService.EmailNotificationServiceClient>(options =>
        {
            options.Address = new Uri(grpcAddress);
        });

        services.AddScoped<INotificationClient, NotificationClient>();
    }
}
