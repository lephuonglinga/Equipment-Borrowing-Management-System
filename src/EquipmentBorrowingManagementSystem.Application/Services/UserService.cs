using AutoMapper;
using EquipmentBorrowingManagementSystem.Application.Common;
using EquipmentBorrowingManagementSystem.Application.DTOs.Users;
using EquipmentBorrowingManagementSystem.Application.Interfaces;
using EquipmentBorrowingManagementSystem.Application.Interfaces.Security;
using EquipmentBorrowingManagementSystem.Application.Interfaces.Services;
using EquipmentBorrowingManagementSystem.Domain.Entities;
using EquipmentBorrowingManagementSystem.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace EquipmentBorrowingManagementSystem.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUser _currentUser;

    public UserService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IPasswordHasher passwordHasher,
        ICurrentUser currentUser)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
        _currentUser = currentUser;
    }

    public async Task<Result<List<UserDto>>> GetAllAsync()
    {
        var users = await _unitOfWork.Users.GetAllAsync();
        var ordered = users.OrderBy(u => u.FullName).ThenBy(u => u.Email).ToList();
        return Result<List<UserDto>>.Ok(_mapper.Map<List<UserDto>>(ordered));
    }

    public async Task<Result<UserDto>> GetByIdAsync(int id)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
        {
            return Result<UserDto>.Fail("Không tìm thấy người dùng.", StatusCodes.Status404NotFound);
        }

        return Result<UserDto>.Ok(_mapper.Map<UserDto>(user));
    }

    public async Task<Result<UserDto>> CreateAsync(CreateUserDto dto)
    {
        var existing = await _unitOfWork.Users.GetByEmailAsync(dto.Email);
        if (existing != null)
        {
            return Result<UserDto>.Fail("Email đã được đăng ký.", StatusCodes.Status409Conflict);
        }

        var user = new User
        {
            Email = dto.Email.Trim(),
            PasswordHash = _passwordHasher.Hash(dto.Password),
            FullName = dto.FullName.Trim(),
            Role = UserRole.Staff,
            IsActive = true
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return Result<UserDto>.Created(_mapper.Map<UserDto>(user));
    }

    public async Task<Result<UserDto>> UpdateAsync(int id, UpdateUserDto dto)
    {
        if (!dto.IsActive && _currentUser.UserId == id)
        {
            return Result<UserDto>.Fail("Bạn không thể vô hiệu hóa tài khoản của chính mình.", StatusCodes.Status400BadRequest);
        }

        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
        {
            return Result<UserDto>.Fail("Không tìm thấy người dùng.", StatusCodes.Status404NotFound);
        }

        if (user.IsActive == dto.IsActive)
        {
            return Result<UserDto>.Ok(
                _mapper.Map<UserDto>(user),
                dto.IsActive ? "User is already active." : "User is already inactive.");
        }

        user.IsActive = dto.IsActive;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync();

        return Result<UserDto>.Ok(
            _mapper.Map<UserDto>(user),
            dto.IsActive ? "User activated." : "User deactivated.");
    }
}
