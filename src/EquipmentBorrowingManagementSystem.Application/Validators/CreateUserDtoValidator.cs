using EquipmentBorrowingManagementSystem.Application.DTOs.Users;
using FluentValidation;

namespace EquipmentBorrowingManagementSystem.Application.Validators;

public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserDtoValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(100);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
    }
}
