using EquipmentBorrowingManagementSystem.Application.Interfaces.Services;
using EquipmentBorrowingManagementSystem.Application.Mappings;
using EquipmentBorrowingManagementSystem.Application.Services;
using EquipmentBorrowingManagementSystem.Application.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace EquipmentBorrowingManagementSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => { }, typeof(MappingProfile).Assembly);

        services.AddValidatorsFromAssemblyContaining<CreateEquipmentDtoValidator>();

        services.AddScoped<IEquipmentService, EquipmentService>();
        services.AddScoped<IEquipmentCategoryService, EquipmentCategoryService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IBorrowRequestService, BorrowRequestService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<IUserService, UserService>();

        return services;
    }
}
