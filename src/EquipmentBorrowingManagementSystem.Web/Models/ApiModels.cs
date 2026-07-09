namespace EquipmentBorrowingManagementSystem.Web.Models;

public class EquipmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
}

public class EquipmentCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;
}

public class BorrowRequestItemDto
{
    public int EquipmentId { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string? HandoverNote { get; set; }
    public string? ReturnNote { get; set; }
}

public class BorrowRequestDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime BorrowDate { get; set; }
    public DateTime ExpectedReturnDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public string? RejectReason { get; set; }
    public List<BorrowRequestItemDto> Items { get; set; } = [];
}

public class CreateBorrowRequestDto
{
    public DateTime BorrowDate { get; set; }
    public DateTime ExpectedReturnDate { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public List<CreateBorrowRequestItemDto> Items { get; set; } = [];
}

public class CreateBorrowRequestItemDto
{
    public int EquipmentId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class UpdateBorrowRequestDto
{
    public string Status { get; set; } = string.Empty;
    public string? RejectReason { get; set; }
    public string? StaffNote { get; set; }
    public List<UpdateBorrowRequestItemDto>? Items { get; set; }
}

public class UpdateBorrowRequestItemDto
{
    public int EquipmentId { get; set; }
    public string? Note { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateUserDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class UpdateUserDto
{
    public bool IsActive { get; set; }
}

public class CreateEquipmentDto
{
    public string Name { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
}

public class UpdateEquipmentDto
{
    public string Name { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
}

public class CreateEquipmentCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateEquipmentCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class StatusCountDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class EquipmentStatusCountDto
{
    public int Total { get; set; }
    public int Available { get; set; }
    public int Borrowed { get; set; }
    public int Maintenance { get; set; }
    public int Retired { get; set; }
}

public class MostBorrowedEquipmentDto
{
    public string EquipmentName { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public int BorrowCount { get; set; }
}

public class DashboardStatsDto
{
    public EquipmentStatusCountDto EquipmentByStatus { get; set; } = new();
    public List<StatusCountDto> BorrowRequestsByStatus { get; set; } = [];
    public int OverdueRequestCount { get; set; }
    public int LostEquipmentCount { get; set; }
    public int CompensatedEquipmentCount { get; set; }
    public int MaintenanceEquipmentCount { get; set; }
    public List<MostBorrowedEquipmentDto> MostBorrowedEquipment { get; set; } = [];
}

public class BorrowSummaryDto
{
    public int TotalRequests { get; set; }
    public int CompletedRequests { get; set; }
    public int ActiveRequests { get; set; }
    public int RejectedRequests { get; set; }
    public int CancelledRequests { get; set; }
    public List<StatusCountDto> RequestsByStatus { get; set; } = [];
}

public class OverdueRequestDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public DateTime ExpectedReturnDate { get; set; }
    public int DaysOverdue { get; set; }
    public List<BorrowRequestItemDto> Items { get; set; } = [];
}

public class BorrowCartItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
}

public class ODataQueryResult
{
    public string RawJson { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string? ErrorMessage { get; set; }
}

public class NotificationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GrpcSendResult
{
    public bool Success { get; set; }
    public string Detail { get; set; } = string.Empty;
}
