using EquipmentBorrowingManagementSystem.Application.Common;
using EquipmentBorrowingManagementSystem.Application.DTOs.Audit;

namespace EquipmentBorrowingManagementSystem.Application.Interfaces.Repositories;

public interface IAuditLogRepository
{
    Task<PagedResult<AuditLogDto>> GetPagedAsync(AuditLogQueryParams query);
}
