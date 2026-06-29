using EquipmentBorrowingManagementSystem.Application.Interfaces;
using EquipmentBorrowingManagementSystem.Application.Interfaces.Repositories;
using EquipmentBorrowingManagementSystem.Infrastructure.Data;
using EquipmentBorrowingManagementSystem.Infrastructure.Repositories;

namespace EquipmentBorrowingManagementSystem.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    private IEquipmentRepository? _equipment;
    private IUserRepository? _users;
    private IRefreshTokenRepository? _refreshTokens;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IEquipmentRepository Equipment =>
        _equipment ??= new EquipmentRepository(_context);

    public IUserRepository Users =>
        _users ??= new UserRepository(_context);

    public IRefreshTokenRepository RefreshTokens =>
        _refreshTokens ??= new RefreshTokenRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
