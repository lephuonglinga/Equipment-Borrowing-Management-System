namespace EquipmentBorrowingManagementSystem.Domain.Entities;

public class BorrowRequestItem : BaseEntity
{
    public int BorrowRequestId { get; set; }
    public int EquipmentId { get; set; }
    public int Quantity { get; set; } = 1;
    public string? HandoverNote { get; set; }
    public string? ReturnNote { get; set; }

    public BorrowRequest BorrowRequest { get; set; } = null!;
    public Equipment Equipment { get; set; } = null!;
}
