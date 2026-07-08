using EquipmentBorrowingManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowingManagementSystem.Infrastructure.Data.Configurations;

public class BorrowRequestItemConfiguration : IEntityTypeConfiguration<BorrowRequestItem>
{
    public void Configure(EntityTypeBuilder<BorrowRequestItem> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.HandoverNote).HasMaxLength(500);
        builder.Property(i => i.ReturnNote).HasMaxLength(500);

        builder.HasIndex(i => new { i.BorrowRequestId, i.EquipmentId }).IsUnique();

        builder.HasOne(i => i.BorrowRequest)
            .WithMany(b => b.Items)
            .HasForeignKey(i => i.BorrowRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(i => i.Equipment)
            .WithMany(e => e.BorrowRequestItems)
            .HasForeignKey(i => i.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
