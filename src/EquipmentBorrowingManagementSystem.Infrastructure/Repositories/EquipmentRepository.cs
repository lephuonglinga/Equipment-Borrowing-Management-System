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

    public async Task<List<Equipment>> GetAllWithCategoryAsync()
    {
        return await Context.Equipments
            .Include(e => e.Category)
            .AsNoTracking()
            .ToListAsync();
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
}
