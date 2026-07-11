namespace EquipmentBorrowingManagementSystem.Application.DTOs.Reports;

public class OverdueRequestItemDto
{
    public int EquipmentId { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
}

public class OverdueRequestDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public DateTime BorrowDate { get; set; }
    public DateTime ExpectedReturnDate { get; set; }
    public int DaysOverdue { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public List<OverdueRequestItemDto> Items { get; set; } = [];
}
