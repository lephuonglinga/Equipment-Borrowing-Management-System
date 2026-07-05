using EquipmentBorrowingManagementSystem.Application.Common;
using EquipmentBorrowingManagementSystem.Application.DTOs.Users;

namespace EquipmentBorrowingManagementSystem.Application.Interfaces.Services;

public interface IUserService
{
    Task<Result<List<UserDto>>> GetAllAsync();
    Task<Result<UserDto>> GetByIdAsync(int id);
    Task<Result<UserDto>> CreateAsync(CreateUserDto dto);
    Task<Result<UserDto>> DeactivateAsync(int id);
    Task<Result<UserDto>> ActivateAsync(int id);
}
