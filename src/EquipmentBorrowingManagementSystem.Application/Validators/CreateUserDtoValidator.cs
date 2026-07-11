using EquipmentBorrowingManagementSystem.Application.Common;
using EquipmentBorrowingManagementSystem.Application.DTOs.Users;
using FluentValidation;

namespace EquipmentBorrowingManagementSystem.Application.Validators;

public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserDtoValidator()
    {
        RuleFor(x => x.Email)
            .Must(v => !string.IsNullOrWhiteSpace(v)).WithMessage(ValidationMessages.Required)
            .EmailAddress().WithMessage(ValidationMessages.InvalidEmail)
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(ValidationMessages.Required)
            .MinimumLength(6).WithMessage(ValidationMessages.PasswordLength)
            .MaximumLength(100).WithMessage(ValidationMessages.PasswordLength);

        RuleFor(x => x.FullName)
            .Must(v => !string.IsNullOrWhiteSpace(v)).WithMessage(ValidationMessages.Required)
            .MaximumLength(200);
    }
}
