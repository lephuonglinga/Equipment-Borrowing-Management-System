namespace EquipmentBorrowingManagementSystem.Application.Interfaces.Security;

public interface ICurrentUser
{
    int? UserId { get; }
    string? Role { get; }
    bool IsAuthenticated { get; }
}
