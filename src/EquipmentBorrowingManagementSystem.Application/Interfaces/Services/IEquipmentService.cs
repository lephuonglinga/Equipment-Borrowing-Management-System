using EquipmentBorrowingManagementSystem.Application.Common;
using EquipmentBorrowingManagementSystem.Application.DTOs;

namespace EquipmentBorrowingManagementSystem.Application.Interfaces.Services;

public interface IEquipmentService
{
    Task<Result<List<EquipmentDto>>> GetAllAsync();
    Task<Result<EquipmentDto>> GetByIdAsync(int id);
    Task<Result<EquipmentDto>> CreateAsync(CreateEquipmentDto dto);
    Task<Result<EquipmentDto>> UpdateAsync(int id, UpdateEquipmentDto dto);
    Task<Result> DeleteAsync(int id);
}
