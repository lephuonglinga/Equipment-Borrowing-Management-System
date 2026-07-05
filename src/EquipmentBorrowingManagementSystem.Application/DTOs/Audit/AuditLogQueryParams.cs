using EquipmentBorrowingManagementSystem.Application.Common;

namespace EquipmentBorrowingManagementSystem.Application.DTOs.Audit;

public class AuditLogQueryParams : PaginationParams
{
    public string? EntityName { get; set; }
    public string? Action { get; set; }
}
