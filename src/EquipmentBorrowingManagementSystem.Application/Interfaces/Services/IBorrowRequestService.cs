using EquipmentBorrowingManagementSystem.Application.Common;
using EquipmentBorrowingManagementSystem.Application.DTOs.Borrowing;

namespace EquipmentBorrowingManagementSystem.Application.Interfaces.Services;

public interface IBorrowRequestService
{
    Task<Result<List<BorrowRequestDto>>> GetAllAsync();
    Task<Result<BorrowRequestDto>> GetByIdAsync(int id);
    Task<Result<BorrowRequestDto>> CreateAsync(CreateBorrowRequestDto dto);
    Task<Result<BorrowRequestDto>> UpdateAsync(int id, UpdateBorrowRequestDto dto);
    Task ProcessOverdueTransitionsAsync();
}
