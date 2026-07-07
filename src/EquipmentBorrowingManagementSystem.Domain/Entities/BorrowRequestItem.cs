using EquipmentBorrowingManagementSystem.Domain.Enums;

namespace EquipmentBorrowingManagementSystem.Domain.Entities;

public class BorrowRequestItem : BaseEntity
{
    public int BorrowRequestId { get; set; }
    public int EquipmentId { get; set; }
    public int Quantity { get; set; } = 1;
    public EquipmentCondition? ConditionAtBorrow { get; set; }
    public EquipmentCondition? ConditionAtReturn { get; set; }
    public string? HandoverNote { get; set; }
    public string? ReturnNote { get; set; }

    public BorrowRequest BorrowRequest { get; set; } = null!;
    public Equipment Equipment { get; set; } = null!;
}
