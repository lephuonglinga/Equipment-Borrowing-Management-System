using EquipmentBorrowingManagementSystem.Domain.Enums;

namespace EquipmentBorrowingManagementSystem.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public bool IsActive { get; set; } = true;

    public ICollection<BorrowRequest> BorrowRequests { get; set; } = [];
    public ICollection<BorrowRequest> ApprovedRequests { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];
    public ICollection<ReturnRecord> ProcessedReturns { get; set; } = [];
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
}
