using EquipmentBorrowingManagementSystem.Application.Common;
using EquipmentBorrowingManagementSystem.Application.DTOs.Categories;
using FluentValidation;

namespace EquipmentBorrowingManagementSystem.Application.Validators;

public class CreateEquipmentCategoryDtoValidator : AbstractValidator<CreateEquipmentCategoryDto>
{
    public CreateEquipmentCategoryDtoValidator()
    {
        RuleFor(x => x.Name)
            .Must(v => !string.IsNullOrWhiteSpace(v)).WithMessage(ValidationMessages.Required)
            .MaximumLength(100);

        RuleFor(x => x.Description).MaximumLength(500);
    }
}
