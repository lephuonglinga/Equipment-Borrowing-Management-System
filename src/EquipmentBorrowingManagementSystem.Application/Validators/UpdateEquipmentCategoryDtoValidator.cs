using EquipmentBorrowingManagementSystem.Application.DTOs.Categories;
using FluentValidation;

namespace EquipmentBorrowingManagementSystem.Application.Validators;

public class UpdateEquipmentCategoryDtoValidator : AbstractValidator<UpdateEquipmentCategoryDto>
{
    public UpdateEquipmentCategoryDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
