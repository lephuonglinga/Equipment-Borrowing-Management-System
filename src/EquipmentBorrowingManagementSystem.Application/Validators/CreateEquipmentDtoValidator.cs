using EquipmentBorrowingManagementSystem.Application.Common;
using EquipmentBorrowingManagementSystem.Application.DTOs.Equipment;
using FluentValidation;

namespace EquipmentBorrowingManagementSystem.Application.Validators;

public class CreateEquipmentDtoValidator : AbstractValidator<CreateEquipmentDto>
{
    public CreateEquipmentDtoValidator()
    {
        RuleFor(x => x.Name)
            .Must(v => !string.IsNullOrWhiteSpace(v)).WithMessage(ValidationMessages.Required)
            .MaximumLength(200);

        RuleFor(x => x.SerialNumber)
            .Must(v => !string.IsNullOrWhiteSpace(v)).WithMessage(ValidationMessages.Required)
            .MaximumLength(100);

        RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage("Danh mục không hợp lệ.");

        RuleFor(x => x.Location)
            .Must(v => !string.IsNullOrWhiteSpace(v)).WithMessage(ValidationMessages.LocationRequired)
            .MaximumLength(200);

        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.ImageUrl).MaximumLength(500);
    }
}
