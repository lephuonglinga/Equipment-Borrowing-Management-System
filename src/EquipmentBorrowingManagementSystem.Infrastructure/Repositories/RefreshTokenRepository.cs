using EquipmentBorrowingManagementSystem.Application.Interfaces.Repositories;
using EquipmentBorrowingManagementSystem.Domain.Entities;
using EquipmentBorrowingManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowingManagementSystem.Infrastructure.Repositories;

public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await Context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == token);
    }
}
