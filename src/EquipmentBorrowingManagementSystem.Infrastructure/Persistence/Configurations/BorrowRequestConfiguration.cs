using EquipmentBorrowingManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowingManagementSystem.Infrastructure.Persistence.Configurations;

public class BorrowRequestConfiguration : IEntityTypeConfiguration<BorrowRequest>
{
    public void Configure(EntityTypeBuilder<BorrowRequest> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Status).HasConversion<int>();
        builder.Property(b => b.Purpose).HasMaxLength(500).IsRequired();
        builder.Property(b => b.RejectReason).HasMaxLength(500);

        builder.HasOne(b => b.ReturnRecord)
            .WithOne(r => r.BorrowRequest)
            .HasForeignKey<ReturnRecord>(r => r.BorrowRequestId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
