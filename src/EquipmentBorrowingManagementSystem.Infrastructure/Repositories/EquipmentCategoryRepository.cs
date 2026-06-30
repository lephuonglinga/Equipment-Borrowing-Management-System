using EquipmentBorrowingManagementSystem.Application.Interfaces.Repositories;
using EquipmentBorrowingManagementSystem.Domain.Entities;
using EquipmentBorrowingManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowingManagementSystem.Infrastructure.Repositories;

public class EquipmentCategoryRepository : GenericRepository<EquipmentCategory>, IEquipmentCategoryRepository
{
    public EquipmentCategoryRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<EquipmentCategory?> GetByNameAsync(string name)
    {
        return await Context.EquipmentCategories.FirstOrDefaultAsync(c => c.Name == name);
    }

    public async Task<bool> HasEquipmentsAsync(int categoryId)
    {
        return await Context.Equipments.AnyAsync(e => e.CategoryId == categoryId);
    }
}
