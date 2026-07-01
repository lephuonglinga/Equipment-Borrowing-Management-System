using EquipmentBorrowingManagementSystem.Application.DTOs;
using EquipmentBorrowingManagementSystem.Domain.Enums;
using FluentValidation;

namespace EquipmentBorrowingManagementSystem.Application.Validators;

public class ReturnBorrowRequestDtoValidator : AbstractValidator<ReturnBorrowRequestDto>
{
    public ReturnBorrowRequestDtoValidator()
    {
        RuleFor(x => x.StaffNote).MaximumLength(500);
        RuleFor(x => x.Items).NotEmpty().WithMessage("Return condition for each item is required.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.EquipmentId).GreaterThan(0);
            item.RuleFor(i => i.ConditionAtReturn)
                .NotEmpty()
                .Must(s => Enum.TryParse<EquipmentCondition>(s, ignoreCase: true, out _))
                .WithMessage("Condition must be one of: Good, Fair, Damaged, Lost.");
        });
    }
}
