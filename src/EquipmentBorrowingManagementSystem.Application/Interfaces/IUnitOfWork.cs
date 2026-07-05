using EquipmentBorrowingManagementSystem.Application.Interfaces.Repositories;

namespace EquipmentBorrowingManagementSystem.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IEquipmentRepository Equipment { get; }
    IEquipmentCategoryRepository EquipmentCategories { get; }
    IBorrowRequestRepository BorrowRequests { get; }
    INotificationRepository Notifications { get; }
    IReportRepository Reports { get; }
    IAuditLogRepository AuditLogs { get; }
    IUserRepository Users { get; }
    IRefreshTokenRepository RefreshTokens { get; }
    Task<int> SaveChangesAsync();
}
