using EquipmentBorrowingManagementSystem.Domain.Enums;

namespace EquipmentBorrowingManagementSystem.Domain.Entities;

public class BorrowRequestItem : BaseEntity
{
    public int BorrowRequestId { get; set; }
    public int EquipmentId { get; set; }
    public string? HandoverNote { get; set; }
    public string? ReturnNote { get; set; }

    /// <summary>Snapshot of the equipment status chosen by staff at return time.</summary>
    public EquipmentStatus? ReturnStatus { get; set; }

    public BorrowRequest BorrowRequest { get; set; } = null!;
    public Equipment Equipment { get; set; } = null!;
}
