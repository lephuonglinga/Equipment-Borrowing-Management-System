using EquipmentBorrowingManagementSystem.Application.DTOs.Users;
using FluentValidation;

namespace EquipmentBorrowingManagementSystem.Application.Validators;

public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserDtoValidator()
    {
        // IsActive is bool — always valid; rules enforced in service (e.g. cannot deactivate self).
    }
}
