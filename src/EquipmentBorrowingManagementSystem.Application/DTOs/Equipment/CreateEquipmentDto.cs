namespace EquipmentBorrowingManagementSystem.Application.DTOs.Equipment;

public class CreateEquipmentDto
{
    public string Name { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
}
