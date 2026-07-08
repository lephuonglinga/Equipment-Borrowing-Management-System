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
            return Result<List<BorrowRequestDto>>.Fail("Phiên đăng nhập không hợp lệ.", StatusCodes.Status401Unauthorized);
        }

        await ProcessExpiredApprovalsAsync();

        var requests = IsStaffOrAdmin(_currentUser.Role)
            ? await _unitOfWork.BorrowRequests.GetAllWithDetailsAsync()
            : await _unitOfWork.BorrowRequests.GetAllWithDetailsAsync(_currentUser.UserId.Value);

        return Result<List<BorrowRequestDto>>.Ok(_mapper.Map<List<BorrowRequestDto>>(requests));
    }

    public async Task<Result<BorrowRequestDto>> GetByIdAsync(int id)
    {
        await ProcessExpiredApprovalsAsync();

        var request = await _unitOfWork.BorrowRequests.GetByIdWithDetailsAsync(id);
        if (request == null)
        {
            return Result<BorrowRequestDto>.Fail("Không tìm thấy yêu cầu mượn.", StatusCodes.Status404NotFound);
        }

        if (!CanAccessRequest(request))
        {
            return Result<BorrowRequestDto>.Fail("Bạn không có quyền xem yêu cầu này.", StatusCodes.Status403Forbidden);
        }

        return Result<BorrowRequestDto>.Ok(_mapper.Map<BorrowRequestDto>(request));
    }

    public async Task<Result<BorrowRequestDto>> CreateAsync(CreateBorrowRequestDto dto)
    {
        if (_currentUser.UserId == null)
        {
            return Result<BorrowRequestDto>.Fail("Phiên đăng nhập không hợp lệ.", StatusCodes.Status401Unauthorized);
        }

        if (dto.ExpectedReturnDate.Date < dto.BorrowDate.Date)
        {
            return Result<BorrowRequestDto>.Fail(
                ValidationMessages.ReturnAfterBorrow,
                StatusCodes.Status400BadRequest);
        }

        if (await _unitOfWork.BorrowRequests.UserHasOverdueRequestAsync(_currentUser.UserId.Value))
        {
            return Result<BorrowRequestDto>.Fail(
                "Bạn đang có yêu cầu mượn quá hạn, không thể tạo yêu cầu mới.",
                StatusCodes.Status400BadRequest);
        }

        var equipmentIds = dto.Items.Select(i => i.EquipmentId).ToList();
        if (equipmentIds.Count != equipmentIds.Distinct().Count())
        {
            return Result<BorrowRequestDto>.Fail(
                ValidationMessages.DuplicateEquipmentInRequest,
                StatusCodes.Status400BadRequest);
        }

        var items = new List<BorrowRequestItem>();
        foreach (var itemDto in dto.Items)
        {
            var equipment = await _unitOfWork.Equipment.GetByIdAsync(itemDto.EquipmentId);
            if (equipment == null)
            {
                return Result<BorrowRequestDto>.Fail(
                    $"Không tìm thấy thiết bị #{itemDto.EquipmentId}.",
                    StatusCodes.Status404NotFound);
            }

            if (!EquipmentRules.IsBorrowable(equipment.Status))
            {
                return Result<BorrowRequestDto>.Fail(
                    DescribeUnavailableEquipment(equipment),
                    StatusCodes.Status400BadRequest);
            }

            if (await _unitOfWork.Equipment.HasActiveBorrowingsAsync(itemDto.EquipmentId))
            {
                return Result<BorrowRequestDto>.Fail(
                    $"Thiết bị '{equipment.Name}' đã nằm trong yêu cầu mượn đang xử lý.",
                    StatusCodes.Status400BadRequest);
            }

            equipment.Status = EquipmentStatus.Reserved;
            _unitOfWork.Equipment.Update(equipment);

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

    public async Task<Result<BorrowRequestDto>> UpdateAsync(int id, UpdateBorrowRequestDto dto)
    {
        if (!Enum.TryParse<BorrowRequestStatus>(dto.Status, ignoreCase: true, out var targetStatus))
        {
            return Result<BorrowRequestDto>.Fail(ValidationMessages.BorrowStatusInvalid, StatusCodes.Status400BadRequest);
        }

        return targetStatus switch
        {
            BorrowRequestStatus.Approved => await ApproveAsync(id),
            BorrowRequestStatus.Rejected => await RejectAsync(id, dto.RejectReason ?? string.Empty),
            BorrowRequestStatus.Cancelled => await CancelAsync(id),
            BorrowRequestStatus.InProgress => await HandoverAsync(id, dto),
            BorrowRequestStatus.Completed => await ReturnAsync(id, dto),
            _ => Result<BorrowRequestDto>.Fail("Chuyển trạng thái không được hỗ trợ.", StatusCodes.Status400BadRequest)
        };
    }

    private async Task<Result<BorrowRequestDto>> ApproveAsync(int id)
    {
        if (!IsStaffOrAdmin(_currentUser.Role))
        {
            return Result<BorrowRequestDto>.Fail("Chỉ Staff/Admin mới được duyệt yêu cầu.", StatusCodes.Status403Forbidden);
        }

        var request = await _unitOfWork.BorrowRequests.GetByIdForUpdateAsync(id);
        if (request == null)
        {
            return Result<BorrowRequestDto>.Fail("Không tìm thấy yêu cầu mượn.", StatusCodes.Status404NotFound);
        }

        if (request.Status != BorrowRequestStatus.Pending)
        {
            return Result<BorrowRequestDto>.Fail(
                "Chỉ có thể duyệt yêu cầu đang ở trạng thái Pending.",
                StatusCodes.Status400BadRequest);
        }

        foreach (var item in request.Items)
        {
            if (item.Equipment.Status != EquipmentStatus.Reserved)
            {
                return Result<BorrowRequestDto>.Fail(
                    $"Thiết bị '{item.Equipment.Name}' không còn ở trạng thái Reserved cho yêu cầu này.",
                    StatusCodes.Status400BadRequest);
            }
        }

        request.Status = BorrowRequestStatus.Approved;
        request.ApprovedById = _currentUser.UserId;
        request.ApprovedAt = DateTime.UtcNow;
        _unitOfWork.BorrowRequests.Update(request);

        await _notificationService.NotifyAsync(
            request.UserId,
            "Borrow request approved",
            $"Your borrow request #{request.Id} has been approved. Please pick up equipment by {request.BorrowDate:yyyy-MM-dd}.",
            NotificationType.RequestApproved);

        await _unitOfWork.SaveChangesAsync();

        var updated = await _unitOfWork.BorrowRequests.GetByIdWithDetailsAsync(id);
        return Result<BorrowRequestDto>.Ok(_mapper.Map<BorrowRequestDto>(updated));
    }

    private async Task<Result<BorrowRequestDto>> HandoverAsync(int id, UpdateBorrowRequestDto dto)
    {
        if (!IsStaffOrAdmin(_currentUser.Role))
        {
            return Result<BorrowRequestDto>.Fail("Chỉ Staff/Admin mới được bàn giao thiết bị.", StatusCodes.Status403Forbidden);
        }

        var request = await _unitOfWork.BorrowRequests.GetByIdForUpdateAsync(id);
        if (request == null)
        {
            return Result<BorrowRequestDto>.Fail("Không tìm thấy yêu cầu mượn.", StatusCodes.Status404NotFound);
        }

        if (request.Status != BorrowRequestStatus.Approved)
        {
            return Result<BorrowRequestDto>.Fail(
                "Chỉ bàn giao được yêu cầu đã duyệt và đang chờ nhận thiết bị.",
                StatusCodes.Status400BadRequest);
        }

        var handoverMap = ParseHandoverMap(dto.Items ?? [], out var parseError);
        if (parseError != null)
        {
            return Result<BorrowRequestDto>.Fail(parseError, StatusCodes.Status400BadRequest);
        }

        var requestEquipmentIds = request.Items.Select(i => i.EquipmentId).ToHashSet();
        if (!requestEquipmentIds.SetEquals(handoverMap.Keys))
        {
            return Result<BorrowRequestDto>.Fail(
                "Danh sách bàn giao phải khớp toàn bộ thiết bị trong yêu cầu.",
                StatusCodes.Status400BadRequest);
        }

        foreach (var item in request.Items)
        {
            var note = handoverMap[item.EquipmentId];
            if (item.Equipment.Status != EquipmentStatus.Reserved)
            {
                return Result<BorrowRequestDto>.Fail(
                    $"Thiết bị '{item.Equipment.Name}' không ở trạng thái Reserved.",
                    StatusCodes.Status400BadRequest);
            }

            item.HandoverNote = note;
            item.Equipment.Status = EquipmentStatus.Borrowed;
            _unitOfWork.Equipment.Update(item.Equipment);
        }

        request.Status = BorrowRequestStatus.InProgress;
        _unitOfWork.BorrowRequests.Update(request);
        await _unitOfWork.SaveChangesAsync();

        var updated = await _unitOfWork.BorrowRequests.GetByIdWithDetailsAsync(id);
        return Result<BorrowRequestDto>.Ok(_mapper.Map<BorrowRequestDto>(updated));
    }

    private async Task<Result<BorrowRequestDto>> RejectAsync(int id, string rejectReason)
    {
        if (!IsStaffOrAdmin(_currentUser.Role))
        {
            return Result<BorrowRequestDto>.Fail("Chỉ Staff/Admin mới được từ chối yêu cầu.", StatusCodes.Status403Forbidden);
        }

        var request = await _unitOfWork.BorrowRequests.GetByIdForUpdateAsync(id);
        if (request == null)
        {
            return Result<BorrowRequestDto>.Fail("Không tìm thấy yêu cầu mượn.", StatusCodes.Status404NotFound);
        }

        if (request.Status != BorrowRequestStatus.Pending)
        {
            return Result<BorrowRequestDto>.Fail(
                "Chỉ có thể từ chối yêu cầu đang ở trạng thái Pending.",
                StatusCodes.Status400BadRequest);
        }

        ReleaseReservedEquipment(request, _unitOfWork);

        request.Status = BorrowRequestStatus.Rejected;
        request.RejectReason = rejectReason;
        request.ApprovedById = _currentUser.UserId;
        request.ApprovedAt = DateTime.UtcNow;
        _unitOfWork.BorrowRequests.Update(request);

        await _notificationService.NotifyAsync(
            request.UserId,
            "Borrow request rejected",
            $"Your borrow request #{request.Id} was rejected. Reason: {rejectReason}",
            NotificationType.RequestRejected);

        await _unitOfWork.SaveChangesAsync();

        var updated = await _unitOfWork.BorrowRequests.GetByIdWithDetailsAsync(id);
        return Result<BorrowRequestDto>.Ok(_mapper.Map<BorrowRequestDto>(updated));
    }

    private async Task<Result<BorrowRequestDto>> CancelAsync(int id)
    {
        if (_currentUser.UserId == null)
        {
            return Result<BorrowRequestDto>.Fail("Unauthorized.", StatusCodes.Status401Unauthorized);
        }

        var request = await _unitOfWork.BorrowRequests.GetByIdForUpdateAsync(id);
        if (request == null)
        {
            return Result<BorrowRequestDto>.Fail("Không tìm thấy yêu cầu mượn.", StatusCodes.Status404NotFound);
        }

        if (request.UserId != _currentUser.UserId.Value)
        {
            return Result<BorrowRequestDto>.Fail("Bạn chỉ có thể hủy yêu cầu mượn của chính mình.", StatusCodes.Status403Forbidden);
        }

        if (request.Status is not (BorrowRequestStatus.Pending or BorrowRequestStatus.Approved))
        {
            return Result<BorrowRequestDto>.Fail(
                "Chỉ hủy được yêu cầu Pending hoặc Approved (chưa bàn giao).",
                StatusCodes.Status400BadRequest);
        }

        ReleaseReservedEquipment(request, _unitOfWork);

        request.Status = BorrowRequestStatus.Cancelled;
        _unitOfWork.BorrowRequests.Update(request);
        await _unitOfWork.SaveChangesAsync();

        var updated = await _unitOfWork.BorrowRequests.GetByIdWithDetailsAsync(id);
        return Result<BorrowRequestDto>.Ok(_mapper.Map<BorrowRequestDto>(updated));
    }

    private async Task<Result<BorrowRequestDto>> ReturnAsync(int id, UpdateBorrowRequestDto dto)
    {
        if (!IsStaffOrAdmin(_currentUser.Role))
        {
            return Result<BorrowRequestDto>.Fail("Chỉ Staff/Admin mới được ghi nhận trả thiết bị.", StatusCodes.Status403Forbidden);
        }

        if (_currentUser.UserId == null)
        {
            return Result<BorrowRequestDto>.Fail("Unauthorized.", StatusCodes.Status401Unauthorized);
        }

        var request = await _unitOfWork.BorrowRequests.GetByIdForUpdateAsync(id);
        if (request == null)
        {
            return Result<BorrowRequestDto>.Fail("Không tìm thấy yêu cầu mượn.", StatusCodes.Status404NotFound);
        }

        if (request.Status is not (BorrowRequestStatus.InProgress or BorrowRequestStatus.Overdue))
        {
            return Result<BorrowRequestDto>.Fail(
                "Chỉ ghi nhận trả được yêu cầu InProgress hoặc Overdue.",
                StatusCodes.Status400BadRequest);
        }

        var returnMap = ParseReturnMap(dto.Items ?? [], out var parseError);
        if (parseError != null)
        {
            return Result<BorrowRequestDto>.Fail(parseError, StatusCodes.Status400BadRequest);
        }

        var requestEquipmentIds = request.Items.Select(i => i.EquipmentId).ToHashSet();
        if (!requestEquipmentIds.SetEquals(returnMap.Keys))
        {
            return Result<BorrowRequestDto>.Fail(
                "Danh sách trả phải khớp toàn bộ thiết bị trong yêu cầu.",
                StatusCodes.Status400BadRequest);
        }

        foreach (var item in request.Items)
        {
            var note = returnMap[item.EquipmentId];
            item.ReturnNote = note;
            item.Equipment.Status = EquipmentStatus.Available;
            _unitOfWork.Equipment.Update(item.Equipment);
        }

        request.Status = BorrowRequestStatus.Completed;
        request.ReturnRecord = new ReturnRecord
        {
            ReturnedById = _currentUser.UserId.Value,
            ReturnedAt = DateTime.UtcNow,
            StaffNote = dto.StaffNote
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

    public async Task ProcessExpiredApprovalsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var expired = await _unitOfWork.BorrowRequests.GetExpiredApprovedAsync(today);
        if (expired.Count == 0)
        {
            return;
        }

        foreach (var request in expired)
        {
            ReleaseReservedEquipment(request, _unitOfWork);
            request.Status = BorrowRequestStatus.Cancelled;
            _unitOfWork.BorrowRequests.Update(request);

            await _notificationService.NotifyAsync(
                request.UserId,
                "Borrow request auto-cancelled",
                $"Your borrow request #{request.Id} was cancelled because equipment was not picked up by {request.BorrowDate:yyyy-MM-dd}.",
                NotificationType.RequestRejected);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private static Dictionary<int, string?> ParseHandoverMap(
        List<UpdateBorrowRequestItemDto> items,
        out string? error)
    {
        error = null;
        var map = new Dictionary<int, string?>();
        foreach (var itemDto in items)
        {
            map[itemDto.EquipmentId] = itemDto.Note?.Trim();
        }

        return map;
    }

    private static Dictionary<int, string?> ParseReturnMap(
        List<UpdateBorrowRequestItemDto> items,
        out string? error)
    {
        error = null;
        var map = new Dictionary<int, string?>();
        foreach (var itemDto in items)
        {
            map[itemDto.EquipmentId] = itemDto.Note?.Trim();
        }

        return map;
    }

    private static string DescribeUnavailableEquipment(Equipment equipment)
    {
        if (equipment.Status == EquipmentStatus.Reserved)
        {
            return $"Thiết bị '{equipment.Name}' đã được giữ chỗ (Reserved) trong yêu cầu mượn khác.";
        }

        if (equipment.Status != EquipmentStatus.Available)
        {
            return $"Thiết bị '{equipment.Name}' không khả dụng (trạng thái: {equipment.Status}).";
        }

        return $"Thiết bị '{equipment.Name}' không đủ điều kiện mượn (trạng thái: {equipment.Status}).";
    }

    private static void ReleaseReservedEquipment(BorrowRequest request, IUnitOfWork unitOfWork)
    {
        foreach (var item in request.Items)
        {
            if (item.Equipment.Status == EquipmentStatus.Reserved)
            {
                item.Equipment.Status = EquipmentStatus.Available;
                unitOfWork.Equipment.Update(item.Equipment);
            }
        }
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
}
