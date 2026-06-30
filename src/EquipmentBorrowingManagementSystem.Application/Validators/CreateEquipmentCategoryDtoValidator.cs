using EquipmentBorrowingManagementSystem.Application.DTOs;
using FluentValidation;

namespace EquipmentBorrowingManagementSystem.Application.Validators;

public class CreateEquipmentCategoryDtoValidator : AbstractValidator<CreateEquipmentCategoryDto>
{
    public CreateEquipmentCategoryDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
