using EquipmentBorrowingManagementSystem.Application.DTOs.Borrowing;
using FluentValidation;

namespace EquipmentBorrowingManagementSystem.Application.Validators;

public class CreateBorrowRequestDtoValidator : AbstractValidator<CreateBorrowRequestDto>
{
    public CreateBorrowRequestDtoValidator()
    {
        RuleFor(x => x.Purpose).NotEmpty().MaximumLength(500);
        RuleFor(x => x.BorrowDate).NotEmpty();
        RuleFor(x => x.ExpectedReturnDate).NotEmpty()
            .GreaterThanOrEqualTo(x => x.BorrowDate)
            .WithMessage("Expected return date must be on or after borrow date.");
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one equipment item is required.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.EquipmentId).GreaterThan(0);
            item.RuleFor(i => i.Quantity).GreaterThan(0);
        });
    }
}
