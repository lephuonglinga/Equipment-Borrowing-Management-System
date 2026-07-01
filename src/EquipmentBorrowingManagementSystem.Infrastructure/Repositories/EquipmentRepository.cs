using EquipmentBorrowingManagementSystem.Application.DTOs.Equipment;
using EquipmentBorrowingManagementSystem.Application.Interfaces.Repositories;
using EquipmentBorrowingManagementSystem.Domain.Entities;
using EquipmentBorrowingManagementSystem.Domain.Enums;
using EquipmentBorrowingManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowingManagementSystem.Infrastructure.Repositories;

public class EquipmentRepository : GenericRepository<Equipment>, IEquipmentRepository
{
    private static readonly BorrowRequestStatus[] ActiveBorrowStatuses =
    [
        BorrowRequestStatus.Pending,
        BorrowRequestStatus.Approved,
        BorrowRequestStatus.InProgress,
        BorrowRequestStatus.Overdue,
        BorrowRequestStatus.Returned
    ];

    public EquipmentRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<(List<Equipment> Items, int TotalCount)> GetPagedWithCategoryAsync(EquipmentQueryParams query)
    {
        var dbQuery = Context.Equipments
            .Include(e => e.Category)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            dbQuery = dbQuery.Where(e =>
                e.Name.Contains(term) || e.SerialNumber.Contains(term));
        }

        if (query.CategoryId.HasValue)
        {
            dbQuery = dbQuery.Where(e => e.CategoryId == query.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status) &&
            Enum.TryParse<EquipmentStatus>(query.Status, ignoreCase: true, out var status))
        {
            dbQuery = dbQuery.Where(e => e.Status == status);
        }

        var totalCount = await dbQuery.CountAsync();

        dbQuery = ApplySorting(dbQuery, query.SortBy, query.SortDirection);

        var items = await dbQuery
            .Skip((query.NormalizedPageNumber - 1) * query.NormalizedPageSize)
            .Take(query.NormalizedPageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Equipment?> GetByIdWithCategoryAsync(int id)
    {
        return await Context.Equipments
            .Include(e => e.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<bool> SerialNumberExistsAsync(string serialNumber, int? excludeId = null)
    {
        return await Context.Equipments.AnyAsync(e =>
            e.SerialNumber == serialNumber && (!excludeId.HasValue || e.Id != excludeId.Value));
    }

    public async Task<bool> HasActiveBorrowingsAsync(int equipmentId)
    {
        return await Context.BorrowRequestItems.AnyAsync(i =>
            i.EquipmentId == equipmentId &&
            ActiveBorrowStatuses.Contains(i.BorrowRequest.Status));
    }

    private static IQueryable<Equipment> ApplySorting(
        IQueryable<Equipment> query, string? sortBy, string? sortDirection)
    {
        var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return (sortBy?.ToLowerInvariant()) switch
        {
            "serialnumber" => descending
                ? query.OrderByDescending(e => e.SerialNumber)
                : query.OrderBy(e => e.SerialNumber),
            "status" => descending
                ? query.OrderByDescending(e => e.Status)
                : query.OrderBy(e => e.Status),
            "categoryname" => descending
                ? query.OrderByDescending(e => e.Category.Name)
                : query.OrderBy(e => e.Category.Name),
            "id" => descending
                ? query.OrderByDescending(e => e.Id)
                : query.OrderBy(e => e.Id),
            _ => descending
                ? query.OrderByDescending(e => e.Name)
                : query.OrderBy(e => e.Name)
        };
    }
}
