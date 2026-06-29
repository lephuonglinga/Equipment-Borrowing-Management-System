using System.ComponentModel.DataAnnotations;

namespace EquipmentBorrowingManagementSystem.Application.DTOs;

public class RefreshRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
