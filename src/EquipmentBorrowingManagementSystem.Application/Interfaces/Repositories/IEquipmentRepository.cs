using EquipmentBorrowingManagementSystem.Domain.Entities;

namespace EquipmentBorrowingManagementSystem.Application.Interfaces.Repositories;

public interface IEquipmentRepository : IGenericRepository<Equipment>
{
    Task<List<Equipment>> GetAllWithCategoryAsync();
    Task<Equipment?> GetByIdWithCategoryAsync(int id);
    Task<bool> SerialNumberExistsAsync(string serialNumber, int? excludeId = null);
    Task<bool> HasActiveBorrowingsAsync(int equipmentId);
}
