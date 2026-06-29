using EquipmentBorrowingManagementSystem.Infrastructure;
using EquipmentBorrowingManagementSystem.Infrastructure.Data;
using EquipmentBorrowingManagementSystem.Infrastructure.Seeders;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowingManagementSystem.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddInfrastructure(builder.Configuration);

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync();
            await DbInitializer.SeedAsync(db);
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapControllers();
        await app.RunAsync();
    }
}
