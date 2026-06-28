using EquipmentBorrowingManagementSystem.Domain.Enums;

namespace EquipmentBorrowingManagementSystem.Domain.Entities;

public class Equipment : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public EquipmentStatus Status { get; set; } = EquipmentStatus.Available;
    public string? Location { get; set; }
    public string? Description { get; set; }

    public EquipmentCategory Category { get; set; } = null!;
    public ICollection<BorrowRequestItem> BorrowRequestItems { get; set; } = [];
}
