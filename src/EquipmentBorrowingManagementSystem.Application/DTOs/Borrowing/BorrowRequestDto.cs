namespace EquipmentBorrowingManagementSystem.Application.DTOs.Borrowing;

public class BorrowRequestDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public DateTime BorrowDate { get; set; }
    public DateTime ExpectedReturnDate { get; set; }
    public DateTime? ActualReturnDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string? RejectReason { get; set; }
    public int? ApprovedById { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? StaffNote { get; set; }
    public List<BorrowRequestItemDto> Items { get; set; } = [];
}
