using EquipmentBorrowingManagementSystem.Application.DTOs.Equipment;
using EquipmentBorrowingManagementSystem.Domain.Enums;
using FluentValidation;

namespace EquipmentBorrowingManagementSystem.Application.Validators;

public class UpdateEquipmentDtoValidator : AbstractValidator<UpdateEquipmentDto>
{
    public UpdateEquipmentDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.SerialNumber).NotEmpty().MaximumLength(100);
        RuleFor(x => x.CategoryId).GreaterThan(0);
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(s => Enum.TryParse<EquipmentStatus>(s, ignoreCase: true, out _))
            .WithMessage("Status must be one of: Available, Borrowed, Maintenance, Retired.");
        RuleFor(x => x.Location).MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.ImageUrl).MaximumLength(500);
    }
}
