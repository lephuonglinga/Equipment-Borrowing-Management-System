using EquipmentBorrowingManagementSystem.Application.Common;
using EquipmentBorrowingManagementSystem.Application.DTOs.Audit;
using EquipmentBorrowingManagementSystem.Application.Interfaces.Repositories;
using EquipmentBorrowingManagementSystem.Domain.Enums;
using EquipmentBorrowingManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowingManagementSystem.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _context;

    public AuditLogRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<AuditLogDto>> GetPagedAsync(AuditLogQueryParams query)
    {
        var dbQuery = _context.AuditLogs.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.EntityName))
        {
            var entityName = query.EntityName.Trim();
            dbQuery = dbQuery.Where(a => a.EntityName == entityName);
        }

        if (!string.IsNullOrWhiteSpace(query.Action) &&
            Enum.TryParse<AuditAction>(query.Action, ignoreCase: true, out var action))
        {
            dbQuery = dbQuery.Where(a => a.Action == action);
        }

        var totalCount = await dbQuery.CountAsync();

        var items = await dbQuery
            .OrderByDescending(a => a.PerformedAt)
            .Skip((query.NormalizedPageNumber - 1) * query.NormalizedPageSize)
            .Take(query.NormalizedPageSize)
            .Select(a => new AuditLogDto
            {
                Id = a.Id,
                UserId = a.UserId,
                UserEmail = a.UserEmail,
                EntityName = a.EntityName,
                EntityId = a.EntityId,
                Action = a.Action.ToString(),
                Changes = a.Changes,
                PerformedAt = a.PerformedAt
            })
            .ToListAsync();

        return new PagedResult<AuditLogDto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = query.NormalizedPageNumber,
            PageSize = query.NormalizedPageSize
        };
    }
}
