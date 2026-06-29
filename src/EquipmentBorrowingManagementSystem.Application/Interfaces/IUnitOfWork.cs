using EquipmentBorrowingManagementSystem.Application.Interfaces.Repositories;

namespace EquipmentBorrowingManagementSystem.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IEquipmentRepository Equipment { get; }
    Task<int> SaveChangesAsync();
}
