using EquipmentBorrowingManagementSystem.Domain.Enums;

namespace EquipmentBorrowingManagementSystem.Domain.Entities;

public class BorrowRequest : BaseEntity
{
    public int UserId { get; set; }
    public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    public DateTime BorrowDate { get; set; }
    public DateTime ExpectedReturnDate { get; set; }
    public DateTime? ActualReturnDate { get; set; }
    public BorrowRequestStatus Status { get; set; } = BorrowRequestStatus.Pending;
    public string Purpose { get; set; } = string.Empty;
    public string? RejectReason { get; set; }
    public int? ApprovedById { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public User User { get; set; } = null!;
    public User? ApprovedBy { get; set; }
    public ICollection<BorrowRequestItem> Items { get; set; } = [];
    public ReturnRecord? ReturnRecord { get; set; }
}
