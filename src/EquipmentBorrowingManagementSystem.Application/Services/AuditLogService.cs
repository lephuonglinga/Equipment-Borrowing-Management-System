using EquipmentBorrowingManagementSystem.Application.Common;
using EquipmentBorrowingManagementSystem.Application.DTOs.Audit;
using EquipmentBorrowingManagementSystem.Application.Interfaces;
using EquipmentBorrowingManagementSystem.Application.Interfaces.Services;
using EquipmentBorrowingManagementSystem.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace EquipmentBorrowingManagementSystem.Application.Services;

public class AuditLogService : IAuditLogService
{
    private readonly IUnitOfWork _unitOfWork;

    public AuditLogService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PagedResult<AuditLogDto>>> GetPagedAsync(AuditLogQueryParams query)
    {
        if (!string.IsNullOrWhiteSpace(query.Action) &&
            !Enum.TryParse<AuditAction>(query.Action, ignoreCase: true, out _))
        {
            return Result<PagedResult<AuditLogDto>>.Fail(
                "Bộ lọc action không hợp lệ. Dùng Created, Updated hoặc Deleted.",
                StatusCodes.Status400BadRequest);
        }

        var result = await _unitOfWork.AuditLogs.GetPagedAsync(query);
        return Result<PagedResult<AuditLogDto>>.Ok(result);
    }
}
