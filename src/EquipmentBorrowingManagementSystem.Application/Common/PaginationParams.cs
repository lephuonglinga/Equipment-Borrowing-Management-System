namespace EquipmentBorrowingManagementSystem.Application.Common;

public class PaginationParams
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 10;

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = DefaultPageSize;

    public int NormalizedPageNumber => PageNumber < 1 ? 1 : PageNumber;

    public int NormalizedPageSize =>
        PageSize < 1 ? DefaultPageSize : Math.Min(PageSize, MaxPageSize);
}
