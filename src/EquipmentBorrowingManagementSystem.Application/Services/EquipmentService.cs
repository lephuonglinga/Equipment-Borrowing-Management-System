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
        if (InputNormalizer.Require(dto.Name, out var name, ValidationMessages.Required) is { } nameError)
        {
            return Result<EquipmentDto>.Fail(nameError, StatusCodes.Status400BadRequest);
        }

        if (InputNormalizer.Require(dto.SerialNumber, out var serial, ValidationMessages.Required) is { } serialError)
        {
            return Result<EquipmentDto>.Fail(serialError, StatusCodes.Status400BadRequest);
        }

        if (InputNormalizer.Require(dto.Location, out var location, ValidationMessages.LocationRequired) is { } locationError)
        {
            return Result<EquipmentDto>.Fail(locationError, StatusCodes.Status400BadRequest);
        }

        dto.Name = name;
        dto.SerialNumber = serial;
        dto.Location = location;
        dto.Description = InputNormalizer.TrimToNull(dto.Description);
        dto.ImageUrl = InputNormalizer.TrimToNull(dto.ImageUrl);

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
        if (InputNormalizer.Require(dto.Name, out var name, ValidationMessages.Required) is { } nameError)
        {
            return Result<EquipmentDto>.Fail(nameError, StatusCodes.Status400BadRequest);
        }

        if (InputNormalizer.Require(dto.SerialNumber, out var serial, ValidationMessages.Required) is { } serialError)
        {
            return Result<EquipmentDto>.Fail(serialError, StatusCodes.Status400BadRequest);
        }

        if (InputNormalizer.Require(dto.Location, out var location, ValidationMessages.LocationRequired) is { } locationError)
        {
            return Result<EquipmentDto>.Fail(locationError, StatusCodes.Status400BadRequest);
        }

        dto.Name = name;
        dto.SerialNumber = serial;
        dto.Location = location;
        dto.Description = InputNormalizer.TrimToNull(dto.Description);
        dto.ImageUrl = InputNormalizer.TrimToNull(dto.ImageUrl);

        var equipment = await _unitOfWork.Equipment.GetByIdAsync(id);
        if (equipment == null)
        {
            return Result<EquipmentDto>.Fail("Không tìm thấy thiết bị.", StatusCodes.Status404NotFound);
        }

        if (EquipmentRules.IsFlowLocked(equipment.Status))
        {
            return Result<EquipmentDto>.Fail(
                "Không thể chỉnh sửa thiết bị đang Reserved hoặc Borrowed. Trạng thái này chỉ đổi qua luồng mượn/trả.",
                StatusCodes.Status400BadRequest);
        }

        if (!Enum.TryParse<EquipmentStatus>(dto.Status, ignoreCase: true, out var targetStatus))
        {
            return Result<EquipmentDto>.Fail(ValidationMessages.EquipmentStatusInvalid, StatusCodes.Status400BadRequest);
        }

        var transitionError = ValidateEquipmentTransition(equipment.Status, targetStatus);
        if (transitionError != null)
        {
            return Result<EquipmentDto>.Fail(transitionError, StatusCodes.Status400BadRequest);
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

        if (!EquipmentRules.CanDelete(equipment.Status))
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
        EquipmentStatus current,
        EquipmentStatus target)
    {
        if (EquipmentRules.IsFlowLocked(current))
        {
            return "Không thể đổi trạng thái khi thiết bị đang Reserved hoặc Borrowed.";
        }

        if (!EquipmentRules.IsStaffSettable(target))
        {
            return "Trạng thái Borrowed/Reserved chỉ được đặt qua luồng mượn trả.";
        }

        return null;
    }
}