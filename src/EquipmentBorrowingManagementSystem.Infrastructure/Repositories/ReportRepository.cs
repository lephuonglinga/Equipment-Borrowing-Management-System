using EquipmentBorrowingManagementSystem.Application.DTOs.Reports;
using EquipmentBorrowingManagementSystem.Application.Interfaces.Repositories;
using EquipmentBorrowingManagementSystem.Domain.Enums;
using EquipmentBorrowingManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowingManagementSystem.Infrastructure.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly AppDbContext _context;

    public ReportRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<BorrowSummaryDto> GetBorrowSummaryAsync(DateTime? fromDate, DateTime? toDate)
    {
        var query = _context.BorrowRequests.AsNoTracking();

        if (fromDate.HasValue)
        {
            query = query.Where(b => b.RequestDate.Date >= fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            query = query.Where(b => b.RequestDate.Date <= toDate.Value.Date);
        }

        var statusGroups = await query
            .GroupBy(b => b.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var statusCounts = statusGroups
            .Select(g => new StatusCountDto
            {
                Status = g.Status.ToString(),
                Count = g.Count
            })
            .OrderBy(s => s.Status)
            .ToList();

        var total = statusCounts.Sum(s => s.Count);

        return new BorrowSummaryDto
        {
            FromDate = fromDate,
            ToDate = toDate,
            TotalRequests = total,
            RequestsByStatus = statusCounts,
            CompletedRequests = statusCounts
                .Where(s => s.Status == BorrowRequestStatus.Completed.ToString())
                .Sum(s => s.Count),
            ActiveRequests = statusCounts
                .Where(s => s.Status is nameof(BorrowRequestStatus.Approved)
                    or nameof(BorrowRequestStatus.InProgress)
                    or nameof(BorrowRequestStatus.Overdue)
                    or nameof(BorrowRequestStatus.Returned))
                .Sum(s => s.Count),
            RejectedRequests = statusCounts
                .Where(s => s.Status == BorrowRequestStatus.Rejected.ToString())
                .Sum(s => s.Count),
            CancelledRequests = statusCounts
                .Where(s => s.Status == BorrowRequestStatus.Cancelled.ToString())
                .Sum(s => s.Count)
        };
    }

    public async Task<List<OverdueRequestDto>> GetOverdueRequestsAsync()
    {
        var today = DateTime.UtcNow.Date;

        var requests = await _context.BorrowRequests
            .AsNoTracking()
            .Include(b => b.User)
            .Include(b => b.Items)
            .ThenInclude(i => i.Equipment)
            .Where(b =>
                b.Status == BorrowRequestStatus.Overdue ||
                ((b.Status == BorrowRequestStatus.Approved || b.Status == BorrowRequestStatus.InProgress) &&
                 b.ExpectedReturnDate.Date < today))
            .OrderByDescending(b => b.ExpectedReturnDate)
            .ToListAsync();

        return requests.Select(b => new OverdueRequestDto
        {
            Id = b.Id,
            UserId = b.UserId,
            UserName = b.User.FullName,
            UserEmail = b.User.Email,
            BorrowDate = b.BorrowDate,
            ExpectedReturnDate = b.ExpectedReturnDate,
            DaysOverdue = Math.Max(0, (today - b.ExpectedReturnDate.Date).Days),
            Status = b.Status.ToString(),
            Purpose = b.Purpose,
            Items = b.Items.Select(i => new OverdueRequestItemDto
            {
                EquipmentId = i.EquipmentId,
                EquipmentName = i.Equipment?.Name ?? string.Empty,
                SerialNumber = i.Equipment?.SerialNumber ?? string.Empty,
                Quantity = i.Quantity
            }).ToList()
        }).ToList();
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var equipmentCounts = await _context.Equipments
            .AsNoTracking()
            .GroupBy(e => e.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var borrowStatusGroups = await _context.BorrowRequests
            .AsNoTracking()
            .GroupBy(b => b.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var borrowStatusCounts = borrowStatusGroups
            .Select(g => new StatusCountDto
            {
                Status = g.Status.ToString(),
                Count = g.Count
            })
            .OrderBy(s => s.Status)
            .ToList();

        var today = DateTime.UtcNow.Date;

        var overdueCount = await _context.BorrowRequests
            .AsNoTracking()
            .CountAsync(b =>
                b.Status == BorrowRequestStatus.Overdue ||
                ((b.Status == BorrowRequestStatus.Approved || b.Status == BorrowRequestStatus.InProgress) &&
                 b.ExpectedReturnDate.Date < today));

        var damagedReturnCount = await _context.BorrowRequestItems
            .AsNoTracking()
            .CountAsync(i =>
                i.ConditionAtReturn == EquipmentCondition.Damaged ||
                i.ConditionAtReturn == EquipmentCondition.Lost);

        var maintenanceCount = equipmentCounts
            .Where(e => e.Status == EquipmentStatus.Maintenance)
            .Sum(e => e.Count);

        var mostBorrowedRaw = await _context.BorrowRequestItems
            .AsNoTracking()
            .Where(i =>
                i.BorrowRequest.Status != BorrowRequestStatus.Rejected &&
                i.BorrowRequest.Status != BorrowRequestStatus.Cancelled)
            .GroupBy(i => i.EquipmentId)
            .Select(g => new { EquipmentId = g.Key, BorrowCount = g.Sum(i => i.Quantity) })
            .OrderByDescending(x => x.BorrowCount)
            .Take(5)
            .ToListAsync();

        var equipmentIds = mostBorrowedRaw.Select(x => x.EquipmentId).ToList();
        var equipmentLookup = await _context.Equipments
            .AsNoTracking()
            .Where(e => equipmentIds.Contains(e.Id))
            .ToDictionaryAsync(e => e.Id);

        var mostBorrowed = mostBorrowedRaw
            .Select(x =>
            {
                equipmentLookup.TryGetValue(x.EquipmentId, out var equipment);
                return new MostBorrowedEquipmentDto
                {
                    EquipmentId = x.EquipmentId,
                    EquipmentName = equipment?.Name ?? string.Empty,
                    SerialNumber = equipment?.SerialNumber ?? string.Empty,
                    BorrowCount = x.BorrowCount
                };
            })
            .OrderByDescending(x => x.BorrowCount)
            .ThenBy(x => x.EquipmentName)
            .ToList();

        return new DashboardStatsDto
        {
            EquipmentByStatus = new EquipmentStatusCountDto
            {
                Total = equipmentCounts.Sum(e => e.Count),
                Available = equipmentCounts
                    .Where(e => e.Status == EquipmentStatus.Available)
                    .Sum(e => e.Count),
                Borrowed = equipmentCounts
                    .Where(e => e.Status == EquipmentStatus.Borrowed)
                    .Sum(e => e.Count),
                Maintenance = maintenanceCount,
                Retired = equipmentCounts
                    .Where(e => e.Status == EquipmentStatus.Retired)
                    .Sum(e => e.Count)
            },
            BorrowRequestsByStatus = borrowStatusCounts,
            OverdueRequestCount = overdueCount,
            DamagedReturnItemCount = damagedReturnCount,
            MaintenanceEquipmentCount = maintenanceCount,
            MostBorrowedEquipment = mostBorrowed
        };
    }
}
