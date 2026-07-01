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

    public EquipmentService(IUnitOfWork unitOfWork, IMapper mapper)
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
                "Invalid status filter. Must be one of: Available, Borrowed, Maintenance, Retired.",
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
        if (equipment == null)
        {
            return Result<EquipmentDto>.Fail("Equipment not found.", StatusCodes.Status404NotFound);
        }

        return Result<EquipmentDto>.Ok(_mapper.Map<EquipmentDto>(equipment));
    }

    public async Task<Result<EquipmentDto>> CreateAsync(CreateEquipmentDto dto)
    {
        var category = await _unitOfWork.EquipmentCategories.GetByIdAsync(dto.CategoryId);
        if (category == null)
        {
            return Result<EquipmentDto>.Fail("Category not found.", StatusCodes.Status404NotFound);
        }

        if (await _unitOfWork.Equipment.SerialNumberExistsAsync(dto.SerialNumber))
        {
            return Result<EquipmentDto>.Fail("Serial number already exists.", StatusCodes.Status409Conflict);
        }

        var equipment = new Equipment
        {
            Name = dto.Name,
            SerialNumber = dto.SerialNumber,
            CategoryId = dto.CategoryId,
            Status = EquipmentStatus.Available,
            Location = dto.Location,
            Description = dto.Description
        };

        await _unitOfWork.Equipment.AddAsync(equipment);
        await _unitOfWork.SaveChangesAsync();

        var created = await _unitOfWork.Equipment.GetByIdWithCategoryAsync(equipment.Id);
        return Result<EquipmentDto>.Created(_mapper.Map<EquipmentDto>(created));
    }

    public async Task<Result<EquipmentDto>> UpdateAsync(int id, UpdateEquipmentDto dto)
    {
        var equipment = await _unitOfWork.Equipment.GetByIdAsync(id);
        if (equipment == null)
        {
            return Result<EquipmentDto>.Fail("Equipment not found.", StatusCodes.Status404NotFound);
        }

        var category = await _unitOfWork.EquipmentCategories.GetByIdAsync(dto.CategoryId);
        if (category == null)
        {
            return Result<EquipmentDto>.Fail("Category not found.", StatusCodes.Status404NotFound);
        }

        if (!Enum.TryParse<EquipmentStatus>(dto.Status, ignoreCase: true, out var status))
        {
            return Result<EquipmentDto>.Fail("Invalid equipment status.", StatusCodes.Status400BadRequest);
        }

        if (await _unitOfWork.Equipment.SerialNumberExistsAsync(dto.SerialNumber, id))
        {
            return Result<EquipmentDto>.Fail("Serial number already exists.", StatusCodes.Status409Conflict);
        }

        equipment.Name = dto.Name;
        equipment.SerialNumber = dto.SerialNumber;
        equipment.CategoryId = dto.CategoryId;
        equipment.Status = status;
        equipment.Location = dto.Location;
        equipment.Description = dto.Description;

        _unitOfWork.Equipment.Update(equipment);
        await _unitOfWork.SaveChangesAsync();

        var updated = await _unitOfWork.Equipment.GetByIdWithCategoryAsync(id);
        return Result<EquipmentDto>.Ok(_mapper.Map<EquipmentDto>(updated));
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var equipment = await _unitOfWork.Equipment.GetByIdAsync(id);
        if (equipment == null)
        {
            return Result.Fail("Equipment not found.", StatusCodes.Status404NotFound);
        }

        if (await _unitOfWork.Equipment.HasActiveBorrowingsAsync(id))
        {
            return Result.Fail("Cannot delete equipment with active borrow requests.", StatusCodes.Status400BadRequest);
        }

        _unitOfWork.Equipment.Delete(equipment);
        await _unitOfWork.SaveChangesAsync();

        return Result.NoContent("Equipment deleted.");
    }
}
