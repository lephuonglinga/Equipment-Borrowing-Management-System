using EquipmentBorrowingManagementSystem.Domain.Entities;

namespace EquipmentBorrowingManagementSystem.Application.Interfaces.Repositories;

public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenAsync(string token);
}
