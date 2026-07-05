using EquipmentBorrowingManagementSystem.Domain.Enums;

namespace EquipmentBorrowingManagementSystem.Domain.Entities;

public class AuditLog : BaseEntity
{
    public int? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public AuditAction Action { get; set; }
    public string? Changes { get; set; }
    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}
