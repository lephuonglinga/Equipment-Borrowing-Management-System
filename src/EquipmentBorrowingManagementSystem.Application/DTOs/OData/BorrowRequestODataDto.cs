using EquipmentBorrowingManagementSystem.Domain.Enums;

namespace EquipmentBorrowingManagementSystem.Application.DTOs.OData;

public class BorrowRequestODataDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime RequestDate { get; set; }
    public DateTime BorrowDate { get; set; }
    public DateTime ExpectedReturnDate { get; set; }
    public DateTime? ActualReturnDate { get; set; }
    public BorrowRequestStatus Status { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string? RejectReason { get; set; }
    public int? ApprovedById { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public ICollection<BorrowRequestItemODataDto> Items { get; set; } = [];
}
