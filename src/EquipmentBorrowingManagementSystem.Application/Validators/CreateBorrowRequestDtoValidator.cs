using EquipmentBorrowingManagementSystem.Application.Common;
using EquipmentBorrowingManagementSystem.Application.DTOs.Borrowing;
using FluentValidation;

namespace EquipmentBorrowingManagementSystem.Application.Validators;

public class CreateBorrowRequestDtoValidator : AbstractValidator<CreateBorrowRequestDto>
{
    public CreateBorrowRequestDtoValidator()
    {
        RuleFor(x => x.Purpose)
            .NotEmpty().WithMessage(ValidationMessages.PurposeRequired)
            .MaximumLength(500).WithMessage(ValidationMessages.PurposeMaxLength);

        RuleFor(x => x.BorrowDate)
            .NotEmpty().WithMessage(ValidationMessages.BorrowDateRequired);

        RuleFor(x => x.ExpectedReturnDate)
            .NotEmpty().WithMessage(ValidationMessages.ReturnDateRequired)
            .GreaterThanOrEqualTo(x => x.BorrowDate)
            .WithMessage(ValidationMessages.ReturnAfterBorrow);

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage(ValidationMessages.AtLeastOneEquipment)
            .Must(items => items.Select(i => i.EquipmentId).Distinct().Count() == items.Count)
            .WithMessage(ValidationMessages.DuplicateEquipmentInRequest);

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.EquipmentId)
                .GreaterThan(0).WithMessage(ValidationMessages.EquipmentIdInvalid);
            item.RuleFor(i => i.Quantity)
                .GreaterThan(0).WithMessage(ValidationMessages.QuantityInvalid);
        });
    }
}
