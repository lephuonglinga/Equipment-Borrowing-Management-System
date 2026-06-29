using EquipmentBorrowingManagementSystem.Domain.Entities;

namespace EquipmentBorrowingManagementSystem.Application.Interfaces.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
}
