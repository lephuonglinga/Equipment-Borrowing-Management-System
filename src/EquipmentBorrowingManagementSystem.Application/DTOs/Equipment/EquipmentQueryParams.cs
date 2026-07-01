using EquipmentBorrowingManagementSystem.Application.Common;

namespace EquipmentBorrowingManagementSystem.Application.DTOs.Equipment;

public class EquipmentQueryParams : PaginationParams
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public string? Status { get; set; }
    public string? SortBy { get; set; } = "name";
    public string? SortDirection { get; set; } = "asc";
}
