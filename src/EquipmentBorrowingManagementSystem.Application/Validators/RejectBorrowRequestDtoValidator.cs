using EquipmentBorrowingManagementSystem.Application.DTOs.Borrowing;
using FluentValidation;

namespace EquipmentBorrowingManagementSystem.Application.Validators;

public class RejectBorrowRequestDtoValidator : AbstractValidator<RejectBorrowRequestDto>
{
    public RejectBorrowRequestDtoValidator()
    {
        RuleFor(x => x.RejectReason).NotEmpty().MaximumLength(500);
    }
}
