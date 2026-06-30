namespace EquipmentBorrowingManagementSystem.Application.DTOs;

public class CreateEquipmentDto
{
    public string Name { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string? Location { get; set; }
    public string? Description { get; set; }
}
