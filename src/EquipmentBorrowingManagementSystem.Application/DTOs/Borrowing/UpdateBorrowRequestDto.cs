namespace EquipmentBorrowingManagementSystem.Application.DTOs.Borrowing;

public class UpdateBorrowRequestItemDto
{
    public int EquipmentId { get; set; }
    public string? Note { get; set; }
    /// <summary>Required when completing a return: Available, Damaged, Maintenance, or Retired.</summary>
    public string? Status { get; set; }
}

/// <summary>
/// Partial update for borrow request state transitions (PATCH).
/// Set <see cref="Status"/> to the target state; include fields required for that transition.
/// </summary>
public class UpdateBorrowRequestDto
{
    public string Status { get; set; } = string.Empty;
    public string? RejectReason { get; set; }
    public string? StaffNote { get; set; }
    public List<UpdateBorrowRequestItemDto>? Items { get; set; }
}
