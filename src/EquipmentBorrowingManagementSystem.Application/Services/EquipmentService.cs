using AutoMapper;
using EquipmentBorrowingManagementSystem.Application.Common;
using EquipmentBorrowingManagementSystem.Application.DTOs.Equipment;
using EquipmentBorrowingManagementSystem.Application.Interfaces;
using EquipmentBorrowingManagementSystem.Application.Interfaces.Services;
using EquipmentBorrowingManagementSystem.Domain.Entities;
using EquipmentBorrowingManagementSystem.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace EquipmentBorrowingManagementSystem.Application.Services;

public class EquipmentService : IEquipmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public EquipmentService(IUnitOfWork unitOfWork, AutoMapper.IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<EquipmentDto>>> GetPagedAsync(EquipmentQueryParams query)
    {
        if (!string.IsNullOrWhiteSpace(query.Status) &&
            !Enum.TryParse<EquipmentStatus>(query.Status, ignoreCase: true, out _))
        {
            return Result<PagedResult<EquipmentDto>>.Fail(
                "Bộ lọc trạng thái không hợp lệ.",
                StatusCodes.Status400BadRequest);
        }

        if (!string.IsNullOrWhiteSpace(query.CurrentCondition) &&
            !Enum.TryParse<EquipmentCondition>(query.CurrentCondition, ignoreCase: true, out _))
        {
            return Result<PagedResult<EquipmentDto>>.Fail(
                "Bộ lọc tình trạng không hợp lệ.",
                StatusCodes.Status400BadRequest);
        }

        var (items, totalCount) = await _unitOfWork.Equipment.GetPagedWithCategoryAsync(query);

        var paged = new PagedResult<EquipmentDto>
        {
            Items = _mapper.Map<List<EquipmentDto>>(items),
            TotalCount = totalCount,
            PageNumber = query.NormalizedPageNumber,
            PageSize = query.NormalizedPageSize
        };

        return Result<PagedResult<EquipmentDto>>.Ok(paged);
    }

    public async Task<Result<EquipmentDto>> GetByIdAsync(int id)
    {
        var equipment = await _unitOfWork.Equipment.GetByIdWithCategoryAsync(id);
        if (equipment == null || !EquipmentRules.IsBrowsable(equipment.Status))
        {
            return Result<EquipmentDto>.Fail("Không tìm thấy thiết bị.", StatusCodes.Status404NotFound);
        }

        return Result<EquipmentDto>.Ok(_mapper.Map<EquipmentDto>(equipment));
    }

    public async Task<Result<EquipmentDto>> CreateAsync(CreateEquipmentDto dto)
    {
        var category = await _unitOfWork.EquipmentCategories.GetByIdAsync(dto.CategoryId);
        if (category == null)
        {
            return Result<EquipmentDto>.Fail("Không tìm thấy danh mục.", StatusCodes.Status404NotFound);
        }

        if (await _unitOfWork.Equipment.SerialNumberExistsAsync(dto.SerialNumber))
        {
            return Result<EquipmentDto>.Fail("Số serial đã tồn tại.", StatusCodes.Status409Conflict);
        }

        var equipment = new Equipment
        {
            Name = dto.Name,
            SerialNumber = dto.SerialNumber,
            CategoryId = dto.CategoryId,
            Status = EquipmentStatus.Available,
            CurrentCondition = EquipmentCondition.Good,
            Location = dto.Location,
            Description = dto.Description,
            ImageUrl = dto.ImageUrl
        };

        await _unitOfWork.Equipment.AddAsync(equipment);
        await _unitOfWork.SaveChangesAsync();

        var created = await _unitOfWork.Equipment.GetByIdWithCategoryAsync(equipment.Id);
        return Result<EquipmentDto>.Created(_mapper.Map<EquipmentDto>(created));
    }

    public async Task<Result<EquipmentDto>> UpdateAsync(int id, UpdateEquipmentDto dto)
    {
        var equipment = await _unitOfWork.Equipment.GetByIdAsync(id);
        if (equipment == null || equipment.Status == EquipmentStatus.Compensated)
        {
            return Result<EquipmentDto>.Fail("Không tìm thấy thiết bị.", StatusCodes.Status404NotFound);
        }

        if (!Enum.TryParse<EquipmentStatus>(dto.Status, ignoreCase: true, out var targetStatus))
        {
            return Result<EquipmentDto>.Fail(ValidationMessages.EquipmentStatusInvalid, StatusCodes.Status400BadRequest);
        }

        if (!Enum.TryParse<EquipmentCondition>(dto.CurrentCondition, ignoreCase: true, out var targetCondition))
        {
            return Result<EquipmentDto>.Fail(ValidationMessages.EquipmentConditionInvalid, StatusCodes.Status400BadRequest);
        }

        var transitionError = ValidateEquipmentTransition(equipment, targetStatus, targetCondition);
        if (transitionError != null)
        {
            return Result<EquipmentDto>.Fail(transitionError, StatusCodes.Status400BadRequest);
        }

        if (equipment.Status is EquipmentStatus.Lost or EquipmentStatus.Compensated)
        {
            equipment.Status = targetStatus;
            equipment.CurrentCondition = targetCondition;
            if (!string.IsNullOrWhiteSpace(dto.Description))
            {
                equipment.Description = dto.Description;
            }

            _unitOfWork.Equipment.Update(equipment);
            await _unitOfWork.SaveChangesAsync();
            return Result<EquipmentDto>.Ok(_mapper.Map<EquipmentDto>(equipment));
        }

        var category = await _unitOfWork.EquipmentCategories.GetByIdAsync(dto.CategoryId);
        if (category == null)
        {
            return Result<EquipmentDto>.Fail("Không tìm thấy danh mục.", StatusCodes.Status404NotFound);
        }

        if (await _unitOfWork.Equipment.SerialNumberExistsAsync(dto.SerialNumber, id))
        {
            return Result<EquipmentDto>.Fail("Số serial đã tồn tại.", StatusCodes.Status409Conflict);
        }

        equipment.Name = dto.Name;
        equipment.SerialNumber = dto.SerialNumber;
        equipment.CategoryId = dto.CategoryId;
        equipment.Status = targetStatus;
        equipment.CurrentCondition = targetCondition;
        equipment.Location = dto.Location;
        equipment.Description = dto.Description;
        equipment.ImageUrl = dto.ImageUrl;

        _unitOfWork.Equipment.Update(equipment);
        await _unitOfWork.SaveChangesAsync();

        var updated = await _unitOfWork.Equipment.GetByIdWithCategoryAsync(id);
        return Result<EquipmentDto>.Ok(_mapper.Map<EquipmentDto>(updated!));
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var equipment = await _unitOfWork.Equipment.GetByIdAsync(id);
        if (equipment == null || !EquipmentRules.IsBrowsable(equipment.Status))
        {
            return Result.Fail("Không tìm thấy thiết bị.", StatusCodes.Status404NotFound);
        }

        if (!EquipmentRules.IsEditable(equipment.Status))
        {
            return Result.Fail("Không thể xóa thiết bị ở trạng thái hiện tại.", StatusCodes.Status400BadRequest);
        }

        if (await _unitOfWork.Equipment.HasActiveBorrowingsAsync(id))
        {
            return Result.Fail("Không thể xóa thiết bị đang có yêu cầu mượn active.", StatusCodes.Status400BadRequest);
        }

        _unitOfWork.Equipment.Delete(equipment);
        await _unitOfWork.SaveChangesAsync();

        return Result.NoContent("Equipment deleted.");
    }

    private static string? ValidateEquipmentTransition(
        Equipment equipment,
        EquipmentStatus targetStatus,
        EquipmentCondition targetCondition)
    {
        if (equipment.Status == EquipmentStatus.Lost)
        {
            if (targetStatus != EquipmentStatus.Compensated ||
                targetCondition != EquipmentCondition.Compensated)
            {
                return "Thiết bị Lost chỉ có thể chuyển sang Compensated.";
            }

            return null;
        }

        if (equipment.Status is EquipmentStatus.Borrowed or EquipmentStatus.Reserved)
        {
            if (targetStatus != equipment.Status)
            {
                return "Không thể đổi trạng thái khi thiết bị đang Reserved hoặc Borrowed.";
            }
        }

        if (targetStatus is EquipmentStatus.Lost or EquipmentStatus.Borrowed)
        {
            return "Trạng thái Lost/Borrowed chỉ được đặt qua luồng mượn trả.";
        }

        if (targetStatus == EquipmentStatus.Compensated &&
            (equipment.Status != EquipmentStatus.Lost || targetCondition != EquipmentCondition.Compensated))
        {
            return "Chỉ thiết bị Lost mới có thể chuyển sang Compensated.";
        }

        if (equipment.Status == EquipmentStatus.Maintenance && targetStatus == EquipmentStatus.Available)
        {
            if (!EquipmentRules.IsPostMaintenanceCondition(targetCondition))
            {
                return "Sau bảo trì, tình trạng phải là Good hoặc Fair.";
            }
        }

        if (targetStatus == EquipmentStatus.Maintenance &&
            equipment.Status == EquipmentStatus.Available &&
            targetCondition is EquipmentCondition.Lost or EquipmentCondition.Compensated)
        {
            return "Không thể chuyển thiết bị Available sang bảo trì với tình trạng Lost/Compensated.";
        }

        return null;
    }
}
