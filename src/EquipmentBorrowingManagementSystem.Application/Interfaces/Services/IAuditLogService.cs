using EquipmentBorrowingManagementSystem.Application.Common;
using EquipmentBorrowingManagementSystem.Application.DTOs.Audit;

namespace EquipmentBorrowingManagementSystem.Application.Interfaces.Services;

public interface IAuditLogService
{
    Task<Result<PagedResult<AuditLogDto>>> GetPagedAsync(AuditLogQueryParams query);
}
