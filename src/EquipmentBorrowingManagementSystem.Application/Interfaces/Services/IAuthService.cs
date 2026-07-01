using EquipmentBorrowingManagementSystem.Application.Common;
using EquipmentBorrowingManagementSystem.Application.DTOs.Auth;

namespace EquipmentBorrowingManagementSystem.Application.Interfaces.Services;

public interface IAuthService
{
    Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto dto);
    Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto);
    Task<Result<AuthResponseDto>> RefreshAsync(RefreshRequestDto dto);
    Task<Result> LogoutAsync(RefreshRequestDto dto);
}
