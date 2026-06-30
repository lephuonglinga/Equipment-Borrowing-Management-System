using EquipmentBorrowingManagementSystem.Application.Common;
using EquipmentBorrowingManagementSystem.Application.DTOs;

namespace EquipmentBorrowingManagementSystem.Application.Interfaces.Services;

public interface IEquipmentCategoryService
{
    Task<Result<List<EquipmentCategoryDto>>> GetAllAsync();
    Task<Result<EquipmentCategoryDto>> GetByIdAsync(int id);
    Task<Result<EquipmentCategoryDto>> CreateAsync(CreateEquipmentCategoryDto dto);
    Task<Result<EquipmentCategoryDto>> UpdateAsync(int id, UpdateEquipmentCategoryDto dto);
    Task<Result> DeleteAsync(int id);
}
