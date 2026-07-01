namespace EquipmentBorrowingManagementSystem.Application.DTOs.Borrowing;

public class ReturnBorrowRequestItemDto
{
    public int EquipmentId { get; set; }
    public string ConditionAtReturn { get; set; } = string.Empty;
}

public class ReturnBorrowRequestDto
{
    public string? StaffNote { get; set; }
    public List<ReturnBorrowRequestItemDto> Items { get; set; } = [];
}
