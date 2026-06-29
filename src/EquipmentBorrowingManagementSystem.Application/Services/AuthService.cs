using EquipmentBorrowingManagementSystem.Application.Common;
using EquipmentBorrowingManagementSystem.Application.DTOs;
using EquipmentBorrowingManagementSystem.Application.Interfaces;
using EquipmentBorrowingManagementSystem.Application.Interfaces.Security;
using EquipmentBorrowingManagementSystem.Application.Interfaces.Services;
using EquipmentBorrowingManagementSystem.Domain.Entities;
using EquipmentBorrowingManagementSystem.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace EquipmentBorrowingManagementSystem.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(IUnitOfWork unitOfWork, IJwtTokenGenerator tokenGenerator, IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _tokenGenerator = tokenGenerator;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<AuthResponseDto>> RegisterAsync(RegisterDto dto)
    {
        var existing = await _unitOfWork.Users.GetByEmailAsync(dto.Email);
        if (existing != null)
        {
            return Result<AuthResponseDto>.Fail("Email is already registered.", StatusCodes.Status409Conflict);
        }

        var user = new User
        {
            Email = dto.Email,
            PasswordHash = _passwordHasher.Hash(dto.Password),
            FullName = dto.FullName,
            Role = UserRole.User,
            IsActive = true
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        var response = await IssueTokensAsync(user);
        return Result<AuthResponseDto>.Created(response);
    }

    public async Task<Result<AuthResponseDto>> LoginAsync(LoginDto dto)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(dto.Email);
        if (user == null || !_passwordHasher.Verify(dto.Password, user.PasswordHash))
        {
            return Result<AuthResponseDto>.Fail("Invalid email or password.", StatusCodes.Status401Unauthorized);
        }

        if (!user.IsActive)
        {
            return Result<AuthResponseDto>.Fail("Account is disabled.", StatusCodes.Status403Forbidden);
        }

        var response = await IssueTokensAsync(user);
        return Result<AuthResponseDto>.Ok(response);
    }

    public async Task<Result<AuthResponseDto>> RefreshAsync(RefreshRequestDto dto)
    {
        var stored = await _unitOfWork.RefreshTokens.GetByTokenAsync(dto.RefreshToken);
        if (stored == null || stored.RevokedAt != null || stored.ExpiresAt <= DateTime.UtcNow)
        {
            return Result<AuthResponseDto>.Fail("Invalid or expired refresh token.", StatusCodes.Status401Unauthorized);
        }

        var user = stored.User;
        if (user == null || !user.IsActive)
        {
            return Result<AuthResponseDto>.Fail("Account is not available.", StatusCodes.Status401Unauthorized);
        }

        stored.RevokedAt = DateTime.UtcNow;
        _unitOfWork.RefreshTokens.Update(stored);

        var response = await IssueTokensAsync(user);
        return Result<AuthResponseDto>.Ok(response);
    }

    public async Task<Result> LogoutAsync(RefreshRequestDto dto)
    {
        var stored = await _unitOfWork.RefreshTokens.GetByTokenAsync(dto.RefreshToken);
        if (stored != null && stored.RevokedAt == null)
        {
            stored.RevokedAt = DateTime.UtcNow;
            _unitOfWork.RefreshTokens.Update(stored);
            await _unitOfWork.SaveChangesAsync();
        }

        return Result.NoContent("Logged out.");
    }

    private async Task<AuthResponseDto> IssueTokensAsync(User user)
    {
        var accessToken = _tokenGenerator.CreateAccessToken(user);
        var refreshToken = _tokenGenerator.CreateRefreshToken();

        await _unitOfWork.RefreshTokens.AddAsync(new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(_tokenGenerator.RefreshTokenDays)
        });
        await _unitOfWork.SaveChangesAsync();

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_tokenGenerator.AccessTokenMinutes),
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role.ToString()
        };
    }
}
