namespace EquipmentBorrowingManagementSystem.Application.DTOs;

public class BorrowRequestItemDto
{
    public int Id { get; set; }
    public int EquipmentId { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? ConditionAtBorrow { get; set; }
    public string? ConditionAtReturn { get; set; }
}
