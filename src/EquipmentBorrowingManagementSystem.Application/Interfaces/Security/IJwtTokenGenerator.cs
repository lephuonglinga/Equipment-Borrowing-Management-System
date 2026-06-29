using EquipmentBorrowingManagementSystem.Domain.Entities;

namespace EquipmentBorrowingManagementSystem.Application.Interfaces.Security;

public interface IJwtTokenGenerator
{
    string CreateAccessToken(User user);
    string CreateRefreshToken();
    int AccessTokenMinutes { get; }
    int RefreshTokenDays { get; }
}
