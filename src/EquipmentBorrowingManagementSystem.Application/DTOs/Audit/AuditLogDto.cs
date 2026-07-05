namespace EquipmentBorrowingManagementSystem.Application.DTOs.Audit;

public class AuditLogDto
{
    public int Id { get; set; }
    public int? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Changes { get; set; }
    public DateTime PerformedAt { get; set; }
}
