using EquipmentBorrowingManagementSystem.Application.Common;
using EquipmentBorrowingManagementSystem.Application.DTOs.Equipment;
using EquipmentBorrowingManagementSystem.Domain.Enums;
using FluentValidation;

namespace EquipmentBorrowingManagementSystem.Application.Validators;

public class UpdateEquipmentDtoValidator : AbstractValidator<UpdateEquipmentDto>
{
    private static readonly EquipmentStatus[] StaffSettableStatuses =
    [
        EquipmentStatus.Available,
        EquipmentStatus.Maintenance,
        EquipmentStatus.Retired,
        EquipmentStatus.Compensated
    ];

    public UpdateEquipmentDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage(ValidationMessages.Required).MaximumLength(200);
        RuleFor(x => x.SerialNumber).NotEmpty().WithMessage(ValidationMessages.Required).MaximumLength(100);
        RuleFor(x => x.CategoryId).GreaterThan(0).WithMessage("Danh mục không hợp lệ.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage(ValidationMessages.Required)
            .Must(s => Enum.TryParse<EquipmentStatus>(s, ignoreCase: true, out var status) &&
                       StaffSettableStatuses.Contains(status))
            .WithMessage(ValidationMessages.EquipmentStatusInvalid);

        RuleFor(x => x.CurrentCondition)
            .NotEmpty().WithMessage(ValidationMessages.Required)
            .Must(s => Enum.TryParse<EquipmentCondition>(s, ignoreCase: true, out _))
            .WithMessage(ValidationMessages.EquipmentConditionInvalid);

        RuleFor(x => x)
            .Must(dto =>
                !string.Equals(dto.Status, nameof(EquipmentStatus.Compensated), StringComparison.OrdinalIgnoreCase) ||
                string.Equals(dto.CurrentCondition, nameof(EquipmentCondition.Compensated), StringComparison.OrdinalIgnoreCase))
            .WithMessage(ValidationMessages.CompensatedPairRequired);

        RuleFor(x => x.Location).MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.ImageUrl).MaximumLength(500);
    }
}
