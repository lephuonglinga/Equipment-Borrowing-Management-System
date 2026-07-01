using AutoMapper;
using EquipmentBorrowingManagementSystem.Application.Common;
using EquipmentBorrowingManagementSystem.Application.Constants;
using EquipmentBorrowingManagementSystem.Application.DTOs.Borrowing;
using EquipmentBorrowingManagementSystem.Application.Interfaces;
using EquipmentBorrowingManagementSystem.Application.Interfaces.Security;
using EquipmentBorrowingManagementSystem.Application.Interfaces.Services;
using EquipmentBorrowingManagementSystem.Domain.Entities;
using EquipmentBorrowingManagementSystem.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace EquipmentBorrowingManagementSystem.Application.Services;

public class BorrowRequestService : IBorrowRequestService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly INotificationService _notificationService;
    private readonly IMapper _mapper;

    public BorrowRequestService(
        IUnitOfWork unitOfWork,
        ICurrentUser currentUser,
        INotificationService notificationService,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _notificationService = notificationService;
        _mapper = mapper;
    }

    public async Task<Result<List<BorrowRequestDto>>> GetAllAsync()
    {
        if (_currentUser.UserId == null)
        {
            return Result<List<BorrowRequestDto>>.Fail("Unauthorized.", StatusCodes.Status401Unauthorized);
        }

        var requests = IsStaffOrAdmin(_currentUser.Role)
            ? await _unitOfWork.BorrowRequests.GetAllWithDetailsAsync()
            : await _unitOfWork.BorrowRequests.GetAllWithDetailsAsync(_currentUser.UserId.Value);

        return Result<List<BorrowRequestDto>>.Ok(_mapper.Map<List<BorrowRequestDto>>(requests));
    }

    public async Task<Result<BorrowRequestDto>> GetByIdAsync(int id)
    {
        var request = await _unitOfWork.BorrowRequests.GetByIdWithDetailsAsync(id);
        if (request == null)
        {
            return Result<BorrowRequestDto>.Fail("Borrow request not found.", StatusCodes.Status404NotFound);
        }

        if (!CanAccessRequest(request))
        {
            return Result<BorrowRequestDto>.Fail("Forbidden.", StatusCodes.Status403Forbidden);
        }

        return Result<BorrowRequestDto>.Ok(_mapper.Map<BorrowRequestDto>(request));
    }

    public async Task<Result<BorrowRequestDto>> CreateAsync(CreateBorrowRequestDto dto)
    {
        if (_currentUser.UserId == null)
        {
            return Result<BorrowRequestDto>.Fail("Unauthorized.", StatusCodes.Status401Unauthorized);
        }

        if (dto.ExpectedReturnDate.Date < dto.BorrowDate.Date)
        {
            return Result<BorrowRequestDto>.Fail(
                "Expected return date must be on or after borrow date.",
                StatusCodes.Status400BadRequest);
        }

        if (await _unitOfWork.BorrowRequests.UserHasOverdueRequestAsync(_currentUser.UserId.Value))
        {
            return Result<BorrowRequestDto>.Fail(
                "You have an overdue borrow request and cannot create a new one.",
                StatusCodes.Status400BadRequest);
        }

        var equipmentIds = dto.Items.Select(i => i.EquipmentId).ToList();
        if (equipmentIds.Count != equipmentIds.Distinct().Count())
        {
            return Result<BorrowRequestDto>.Fail(
                "Duplicate equipment in the same request is not allowed.",
                StatusCodes.Status400BadRequest);
        }

        var items = new List<BorrowRequestItem>();
        foreach (var itemDto in dto.Items)
        {
            var equipment = await _unitOfWork.Equipment.GetByIdAsync(itemDto.EquipmentId);
            if (equipment == null)
            {
                return Result<BorrowRequestDto>.Fail(
                    $"Equipment {itemDto.EquipmentId} not found.",
                    StatusCodes.Status404NotFound);
            }

            if (equipment.Status != EquipmentStatus.Available)
            {
                return Result<BorrowRequestDto>.Fail(
                    $"Equipment '{equipment.Name}' is not available.",
                    StatusCodes.Status400BadRequest);
            }

            items.Add(new BorrowRequestItem
            {
                EquipmentId = itemDto.EquipmentId,
                Quantity = itemDto.Quantity
            });
        }

        var request = new BorrowRequest
        {
            UserId = _currentUser.UserId.Value,
            RequestDate = DateTime.UtcNow,
            BorrowDate = dto.BorrowDate.Date,
            ExpectedReturnDate = dto.ExpectedReturnDate.Date,
            Purpose = dto.Purpose,
            Status = BorrowRequestStatus.Pending,
            Items = items
        };

        await _unitOfWork.BorrowRequests.AddAsync(request);
        await _unitOfWork.SaveChangesAsync();

        var created = await _unitOfWork.BorrowRequests.GetByIdWithDetailsAsync(request.Id);
        return Result<BorrowRequestDto>.Created(_mapper.Map<BorrowRequestDto>(created));
    }

    public async Task<Result<BorrowRequestDto>> ApproveAsync(int id)
    {
        if (!IsStaffOrAdmin(_currentUser.Role))
        {
            return Result<BorrowRequestDto>.Fail("Forbidden.", StatusCodes.Status403Forbidden);
        }

        var request = await _unitOfWork.BorrowRequests.GetByIdForUpdateAsync(id);
        if (request == null)
        {
            return Result<BorrowRequestDto>.Fail("Borrow request not found.", StatusCodes.Status404NotFound);
        }

        if (request.Status != BorrowRequestStatus.Pending)
        {
            return Result<BorrowRequestDto>.Fail(
                "Only pending requests can be approved.",
                StatusCodes.Status400BadRequest);
        }

        foreach (var item in request.Items)
        {
            if (item.Equipment.Status != EquipmentStatus.Available)
            {
                return Result<BorrowRequestDto>.Fail(
                    $"Equipment '{item.Equipment.Name}' is no longer available.",
                    StatusCodes.Status400BadRequest);
            }

            item.ConditionAtBorrow = EquipmentCondition.Good;
            item.Equipment.Status = EquipmentStatus.Borrowed;
            _unitOfWork.Equipment.Update(item.Equipment);
        }

        request.Status = BorrowRequestStatus.Approved;
        request.ApprovedById = _currentUser.UserId;
        request.ApprovedAt = DateTime.UtcNow;
        _unitOfWork.BorrowRequests.Update(request);

        await _notificationService.NotifyAsync(
            request.UserId,
            "Borrow request approved",
            $"Your borrow request #{request.Id} has been approved.",
            NotificationType.RequestApproved);

        await _unitOfWork.SaveChangesAsync();

        var updated = await _unitOfWork.BorrowRequests.GetByIdWithDetailsAsync(id);
        return Result<BorrowRequestDto>.Ok(_mapper.Map<BorrowRequestDto>(updated));
    }

    public async Task<Result<BorrowRequestDto>> RejectAsync(int id, RejectBorrowRequestDto dto)
    {
        if (!IsStaffOrAdmin(_currentUser.Role))
        {
            return Result<BorrowRequestDto>.Fail("Forbidden.", StatusCodes.Status403Forbidden);
        }

        var request = await _unitOfWork.BorrowRequests.GetByIdForUpdateAsync(id);
        if (request == null)
        {
            return Result<BorrowRequestDto>.Fail("Borrow request not found.", StatusCodes.Status404NotFound);
        }

        if (request.Status != BorrowRequestStatus.Pending)
        {
            return Result<BorrowRequestDto>.Fail(
                "Only pending requests can be rejected.",
                StatusCodes.Status400BadRequest);
        }

        request.Status = BorrowRequestStatus.Rejected;
        request.RejectReason = dto.RejectReason;
        request.ApprovedById = _currentUser.UserId;
        request.ApprovedAt = DateTime.UtcNow;
        _unitOfWork.BorrowRequests.Update(request);

        await _notificationService.NotifyAsync(
            request.UserId,
            "Borrow request rejected",
            $"Your borrow request #{request.Id} was rejected. Reason: {dto.RejectReason}",
            NotificationType.RequestRejected);

        await _unitOfWork.SaveChangesAsync();

        var updated = await _unitOfWork.BorrowRequests.GetByIdWithDetailsAsync(id);
        return Result<BorrowRequestDto>.Ok(_mapper.Map<BorrowRequestDto>(updated));
    }

    public async Task<Result<BorrowRequestDto>> CancelAsync(int id)
    {
        if (_currentUser.UserId == null)
        {
            return Result<BorrowRequestDto>.Fail("Unauthorized.", StatusCodes.Status401Unauthorized);
        }

        var request = await _unitOfWork.BorrowRequests.GetByIdForUpdateAsync(id);
        if (request == null)
        {
            return Result<BorrowRequestDto>.Fail("Borrow request not found.", StatusCodes.Status404NotFound);
        }

        if (request.UserId != _currentUser.UserId.Value)
        {
            return Result<BorrowRequestDto>.Fail("Forbidden.", StatusCodes.Status403Forbidden);
        }

        if (request.Status != BorrowRequestStatus.Pending)
        {
            return Result<BorrowRequestDto>.Fail(
                "Only pending requests can be cancelled.",
                StatusCodes.Status400BadRequest);
        }

        request.Status = BorrowRequestStatus.Cancelled;
        _unitOfWork.BorrowRequests.Update(request);
        await _unitOfWork.SaveChangesAsync();

        var updated = await _unitOfWork.BorrowRequests.GetByIdWithDetailsAsync(id);
        return Result<BorrowRequestDto>.Ok(_mapper.Map<BorrowRequestDto>(updated));
    }

    public async Task<Result<BorrowRequestDto>> ReturnAsync(int id, ReturnBorrowRequestDto dto)
    {
        if (!IsStaffOrAdmin(_currentUser.Role))
        {
            return Result<BorrowRequestDto>.Fail("Forbidden.", StatusCodes.Status403Forbidden);
        }

        if (_currentUser.UserId == null)
        {
            return Result<BorrowRequestDto>.Fail("Unauthorized.", StatusCodes.Status401Unauthorized);
        }

        var request = await _unitOfWork.BorrowRequests.GetByIdForUpdateAsync(id);
        if (request == null)
        {
            return Result<BorrowRequestDto>.Fail("Borrow request not found.", StatusCodes.Status404NotFound);
        }

        if (request.Status is not (BorrowRequestStatus.Approved or BorrowRequestStatus.InProgress or BorrowRequestStatus.Overdue))
        {
            return Result<BorrowRequestDto>.Fail(
                "Only approved, in-progress, or overdue requests can be returned.",
                StatusCodes.Status400BadRequest);
        }

        var returnMap = new Dictionary<int, EquipmentCondition>();
        foreach (var itemDto in dto.Items)
        {
            if (!Enum.TryParse<EquipmentCondition>(itemDto.ConditionAtReturn, ignoreCase: true, out var condition))
            {
                return Result<BorrowRequestDto>.Fail(
                    "Invalid return condition.",
                    StatusCodes.Status400BadRequest);
            }

            returnMap[itemDto.EquipmentId] = condition;
        }

        var requestEquipmentIds = request.Items.Select(i => i.EquipmentId).ToHashSet();
        if (!requestEquipmentIds.SetEquals(returnMap.Keys))
        {
            return Result<BorrowRequestDto>.Fail(
                "Return items must match all equipment in the borrow request.",
                StatusCodes.Status400BadRequest);
        }

        var worstCondition = EquipmentCondition.Good;
        foreach (var item in request.Items)
        {
            var condition = returnMap[item.EquipmentId];
            item.ConditionAtReturn = condition;
            item.Equipment.Status = MapReturnConditionToEquipmentStatus(condition);
            _unitOfWork.Equipment.Update(item.Equipment);

            if (condition > worstCondition)
            {
                worstCondition = condition;
            }
        }

        request.Status = BorrowRequestStatus.Completed;
        request.ReturnRecord = new ReturnRecord
        {
            ReturnedById = _currentUser.UserId.Value,
            ReturnedAt = DateTime.UtcNow,
            StaffNote = dto.StaffNote,
            OverallCondition = worstCondition
        };
        _unitOfWork.BorrowRequests.Update(request);

        await _notificationService.NotifyAsync(
            request.UserId,
            "Equipment returned",
            $"Your borrow request #{request.Id} has been completed.",
            NotificationType.EquipmentReturned);

        await _unitOfWork.SaveChangesAsync();

        var updated = await _unitOfWork.BorrowRequests.GetByIdWithDetailsAsync(id);
        return Result<BorrowRequestDto>.Ok(_mapper.Map<BorrowRequestDto>(updated));
    }

    private bool CanAccessRequest(BorrowRequest request)
    {
        if (_currentUser.UserId == null)
        {
            return false;
        }

        return IsStaffOrAdmin(_currentUser.Role) || request.UserId == _currentUser.UserId.Value;
    }

    private static bool IsStaffOrAdmin(string? role) =>
        role is Roles.Admin or Roles.Staff;

    private static EquipmentStatus MapReturnConditionToEquipmentStatus(EquipmentCondition condition) =>
        condition switch
        {
            EquipmentCondition.Good or EquipmentCondition.Fair => EquipmentStatus.Available,
            EquipmentCondition.Damaged => EquipmentStatus.Maintenance,
            EquipmentCondition.Lost => EquipmentStatus.Retired,
            _ => EquipmentStatus.Available
        };
}
