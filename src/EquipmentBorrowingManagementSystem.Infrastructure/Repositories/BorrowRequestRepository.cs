using EquipmentBorrowingManagementSystem.Application.Interfaces.Repositories;
using EquipmentBorrowingManagementSystem.Domain.Entities;
using EquipmentBorrowingManagementSystem.Domain.Enums;
using EquipmentBorrowingManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowingManagementSystem.Infrastructure.Repositories;

public class BorrowRequestRepository : GenericRepository<BorrowRequest>, IBorrowRequestRepository
{
    public BorrowRequestRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<List<BorrowRequest>> GetAllWithDetailsAsync(int? userId = null)
    {
        var query = BuildDetailsQuery().AsNoTracking();

        if (userId.HasValue)
        {
            query = query.Where(b => b.UserId == userId.Value);
        }

        return await query
            .OrderByDescending(b => b.RequestDate)
            .ToListAsync();
    }

    public async Task<BorrowRequest?> GetByIdWithDetailsAsync(int id)
    {
        return await BuildDetailsQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<BorrowRequest?> GetByIdForUpdateAsync(int id)
    {
        return await Context.BorrowRequests
            .Include(b => b.Items)
            .ThenInclude(i => i.Equipment)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<bool> UserHasOverdueRequestAsync(int userId)
    {
        return await Context.BorrowRequests.AnyAsync(b =>
            b.UserId == userId && b.Status == BorrowRequestStatus.Overdue);
    }

    private IQueryable<BorrowRequest> BuildDetailsQuery()
    {
        return Context.BorrowRequests
            .Include(b => b.User)
            .Include(b => b.ApprovedBy)
            .Include(b => b.Items)
            .ThenInclude(i => i.Equipment);
    }
}
