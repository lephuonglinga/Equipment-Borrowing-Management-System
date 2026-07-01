using EquipmentBorrowingManagementSystem.Application.DTOs.Equipment;
using FluentValidation;

namespace EquipmentBorrowingManagementSystem.Application.Validators;

public class CreateEquipmentDtoValidator : AbstractValidator<CreateEquipmentDto>
{
    public CreateEquipmentDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SerialNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.Location).MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}
