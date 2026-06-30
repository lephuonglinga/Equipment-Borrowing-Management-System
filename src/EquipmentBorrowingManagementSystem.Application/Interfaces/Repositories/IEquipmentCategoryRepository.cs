using EquipmentBorrowingManagementSystem.Domain.Entities;

namespace EquipmentBorrowingManagementSystem.Application.Interfaces.Repositories;

public interface IEquipmentCategoryRepository : IGenericRepository<EquipmentCategory>
{
    Task<EquipmentCategory?> GetByNameAsync(string name);
    Task<bool> HasEquipmentsAsync(int categoryId);
}
