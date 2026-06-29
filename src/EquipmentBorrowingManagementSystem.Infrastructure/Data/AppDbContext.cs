using EquipmentBorrowingManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowingManagementSystem.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<EquipmentCategory> EquipmentCategories => Set<EquipmentCategory>();
    public DbSet<Equipment> Equipments => Set<Equipment>();
    public DbSet<BorrowRequest> BorrowRequests => Set<BorrowRequest>();
    public DbSet<BorrowRequestItem> BorrowRequestItems => Set<BorrowRequestItem>();
    public DbSet<ReturnRecord> ReturnRecords => Set<ReturnRecord>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
