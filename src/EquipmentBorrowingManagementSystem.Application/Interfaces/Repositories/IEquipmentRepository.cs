using EquipmentBorrowingManagementSystem.Domain.Entities;

namespace EquipmentBorrowingManagementSystem.Application.Interfaces.Repositories;

public interface IEquipmentRepository : IGenericRepository<Equipment>
{
    Task<List<Equipment>> GetAllWithCategoryAsync();
    Task<Equipment?> GetByIdWithCategoryAsync(int id);
}
