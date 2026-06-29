using EquipmentBorrowingManagementSystem.Application.Interfaces.Repositories;
using EquipmentBorrowingManagementSystem.Domain.Entities;
using EquipmentBorrowingManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowingManagementSystem.Infrastructure.Repositories;

public class EquipmentRepository : GenericRepository<Equipment>, IEquipmentRepository
{
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
}
