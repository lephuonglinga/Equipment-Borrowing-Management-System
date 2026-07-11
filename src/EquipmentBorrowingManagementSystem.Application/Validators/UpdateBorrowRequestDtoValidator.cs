using EquipmentBorrowingManagementSystem.Application.Common;
using EquipmentBorrowingManagementSystem.Application.DTOs.Borrowing;
using EquipmentBorrowingManagementSystem.Domain.Enums;
using FluentValidation;

namespace EquipmentBorrowingManagementSystem.Application.Validators;

public class UpdateBorrowRequestDtoValidator : AbstractValidator<UpdateBorrowRequestDto>
{
    private static readonly string[] AllowedTargets =
    [
        nameof(BorrowRequestStatus.Approved),
        nameof(BorrowRequestStatus.Rejected),
        nameof(BorrowRequestStatus.Cancelled),
        nameof(BorrowRequestStatus.InProgress),
        nameof(BorrowRequestStatus.Completed)
    ];

    public UpdateBorrowRequestDtoValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage(ValidationMessages.Required)
            .Must(s => AllowedTargets.Contains(s, StringComparer.OrdinalIgnoreCase))
            .WithMessage(ValidationMessages.BorrowStatusInvalid);

        RuleFor(x => x.RejectReason)
            .Must(value => InputNormalizer.HasContent(value))
            .WithMessage(ValidationMessages.RejectReasonRequired)
            .Must(value => value!.Trim().Length <= 500)
            .WithMessage(ValidationMessages.RejectReasonMaxLength)
            .When(x => string.Equals(x.Status, nameof(BorrowRequestStatus.Rejected), StringComparison.OrdinalIgnoreCase));

        RuleFor(x => x.StaffNote)
            .MaximumLength(500).WithMessage(ValidationMessages.StaffNoteMaxLength);

        RuleFor(x => x)
            .Custom(ValidateTransitionPayload);
    }

    private static void ValidateTransitionPayload(UpdateBorrowRequestDto dto, ValidationContext<UpdateBorrowRequestDto> context)
    {
        if (string.Equals(dto.Status, nameof(BorrowRequestStatus.InProgress), StringComparison.OrdinalIgnoreCase))
        {
            ValidateHandoverItems(dto, context);
            return;
        }

        if (string.Equals(dto.Status, nameof(BorrowRequestStatus.Completed), StringComparison.OrdinalIgnoreCase))
        {
            ValidateReturnItems(dto, context);
        }
    }

    private static void ValidateHandoverItems(
        UpdateBorrowRequestDto dto,
        ValidationContext<UpdateBorrowRequestDto> context)
    {
        if (dto.Items == null || dto.Items.Count == 0)
        {
            context.AddFailure(nameof(dto.Items), ValidationMessages.HandoverItemsRequired);
            return;
        }

        foreach (var item in dto.Items)
        {
            if (item.EquipmentId <= 0)
            {
                context.AddFailure(nameof(dto.Items), ValidationMessages.EquipmentIdInvalid);
            }

            if (item.Note != null && item.Note.Length > 500)
            {
                context.AddFailure(nameof(dto.Items), ValidationMessages.NoteMaxLength);
            }
        }
    }

    private static void ValidateReturnItems(
        UpdateBorrowRequestDto dto,
        ValidationContext<UpdateBorrowRequestDto> context)
    {
        if (dto.Items == null || dto.Items.Count == 0)
        {
            context.AddFailure(nameof(dto.Items), ValidationMessages.ReturnItemsRequired);
            return;
        }

        foreach (var item in dto.Items)
        {
            if (item.EquipmentId <= 0)
            {
                context.AddFailure(nameof(dto.Items), ValidationMessages.EquipmentIdInvalid);
            }

            if (string.IsNullOrWhiteSpace(item.Status))
            {
                context.AddFailure(nameof(dto.Items), ValidationMessages.ReturnEquipmentStatusRequired);
            }
            else if (!Enum.TryParse<EquipmentStatus>(item.Status, ignoreCase: true, out var status) ||
                     !EquipmentRules.IsValidReturnStatus(status))
            {
                context.AddFailure(nameof(dto.Items), ValidationMessages.ReturnEquipmentStatusInvalid);
            }

            if (item.Note != null && item.Note.Length > 500)
            {
                context.AddFailure(nameof(dto.Items), ValidationMessages.NoteMaxLength);
            }
        }
    }
}
