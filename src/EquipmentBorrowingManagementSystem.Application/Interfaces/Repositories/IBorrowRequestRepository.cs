using EquipmentBorrowingManagementSystem.Domain.Entities;

namespace EquipmentBorrowingManagementSystem.Application.Interfaces.Repositories;

public interface IBorrowRequestRepository : IGenericRepository<BorrowRequest>
{
    Task<List<BorrowRequest>> GetAllWithDetailsAsync(int? userId = null);
    Task<BorrowRequest?> GetByIdWithDetailsAsync(int id);
    Task<BorrowRequest?> GetByIdForUpdateAsync(int id);
    Task<bool> UserHasOverdueRequestAsync(int userId);
    Task<List<BorrowRequest>> GetExpiredApprovedAsync(DateTime utcToday);
}
