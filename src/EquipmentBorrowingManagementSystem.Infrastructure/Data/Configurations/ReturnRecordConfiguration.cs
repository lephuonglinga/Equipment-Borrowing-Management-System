using EquipmentBorrowingManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowingManagementSystem.Infrastructure.Data.Configurations;

public class ReturnRecordConfiguration : IEntityTypeConfiguration<ReturnRecord>
{
    public void Configure(EntityTypeBuilder<ReturnRecord> builder)
    {
        builder.HasKey(r => r.Id);

        builder.HasIndex(r => r.BorrowRequestId).IsUnique();

        builder.Property(r => r.StaffNote).HasMaxLength(1000);
        builder.Property(r => r.OverallCondition).HasConversion<int>();
    }
}
