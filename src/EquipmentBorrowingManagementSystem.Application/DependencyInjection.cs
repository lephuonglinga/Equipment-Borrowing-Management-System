using EquipmentBorrowingManagementSystem.Application.Interfaces.Services;
using EquipmentBorrowingManagementSystem.Application.Mappings;
using EquipmentBorrowingManagementSystem.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace EquipmentBorrowingManagementSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => { }, typeof(MappingProfile).Assembly);

        services.AddScoped<IEquipmentService, EquipmentService>();

        return services;
    }
}
