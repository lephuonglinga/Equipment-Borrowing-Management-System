namespace EquipmentBorrowingManagementSystem.Application.DTOs.Borrowing;

public class CreateBorrowRequestItemDto
{
    public int EquipmentId { get; set; }
}

public class CreateBorrowRequestDto
{
    public DateTime BorrowDate { get; set; }
    public DateTime ExpectedReturnDate { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public List<CreateBorrowRequestItemDto> Items { get; set; } = [];
}
