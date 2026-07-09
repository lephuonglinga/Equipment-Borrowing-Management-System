namespace EquipmentBorrowingManagementSystem.Application.DTOs.OData;

public class BorrowRequestItemODataDto
{
    public int Id { get; set; }
    public int EquipmentId { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string? HandoverNote { get; set; }
    public string? ReturnNote { get; set; }
}
