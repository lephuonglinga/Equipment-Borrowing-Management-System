using EquipmentBorrowingManagementSystem.Domain.Enums;

namespace EquipmentBorrowingManagementSystem.Domain.Entities;

public class ReturnRecord : BaseEntity
{
    public int BorrowRequestId { get; set; }
    public DateTime ReturnedAt { get; set; } = DateTime.UtcNow;
    public int ReturnedById { get; set; }
    public string? StaffNote { get; set; }
    public EquipmentCondition OverallCondition { get; set; } = EquipmentCondition.Good;

    public BorrowRequest BorrowRequest { get; set; } = null!;
    public User ReturnedBy { get; set; } = null!;
}
