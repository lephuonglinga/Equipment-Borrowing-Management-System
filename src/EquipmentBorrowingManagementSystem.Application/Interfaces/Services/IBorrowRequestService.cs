using EquipmentBorrowingManagementSystem.Application.Common;
using EquipmentBorrowingManagementSystem.Application.DTOs.Borrowing;

namespace EquipmentBorrowingManagementSystem.Application.Interfaces.Services;

public interface IBorrowRequestService
{
    Task<Result<List<BorrowRequestDto>>> GetAllAsync();
    Task<Result<BorrowRequestDto>> GetByIdAsync(int id);
    Task<Result<BorrowRequestDto>> CreateAsync(CreateBorrowRequestDto dto);
    Task<Result<BorrowRequestDto>> ApproveAsync(int id);
    Task<Result<BorrowRequestDto>> RejectAsync(int id, RejectBorrowRequestDto dto);
    Task<Result<BorrowRequestDto>> CancelAsync(int id);
    Task<Result<BorrowRequestDto>> ReturnAsync(int id, ReturnBorrowRequestDto dto);
}
