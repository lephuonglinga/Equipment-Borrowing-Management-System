using System.ComponentModel.DataAnnotations;

namespace EquipmentBorrowingManagementSystem.Application.DTOs.Auth;

public class RefreshRequestDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
