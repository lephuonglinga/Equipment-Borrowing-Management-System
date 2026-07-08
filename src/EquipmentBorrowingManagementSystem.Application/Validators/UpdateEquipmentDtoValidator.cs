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
        EquipmentStatus.Lost,
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
            .WithMessage("Trạng thái phải là một trong: Available, Maintenance, Retired, Lost, Compensated.");

        RuleFor(x => x.Location).MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.ImageUrl).MaximumLength(500);
    }
}
