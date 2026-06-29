using EquipmentBorrowingManagementSystem.Application.Common;
using EquipmentBorrowingManagementSystem.Application.DTOs;

namespace EquipmentBorrowingManagementSystem.Application.Interfaces.Services;

public interface IEquipmentService
{
    Task<Result<List<EquipmentDto>>> GetAllAsync();
    Task<Result<EquipmentDto>> GetByIdAsync(int id);
}
