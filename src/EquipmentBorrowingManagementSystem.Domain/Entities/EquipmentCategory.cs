namespace EquipmentBorrowingManagementSystem.Domain.Entities;

public class EquipmentCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Equipment> Equipments { get; set; } = [];
}
