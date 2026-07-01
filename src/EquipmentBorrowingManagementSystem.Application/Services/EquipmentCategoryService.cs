using AutoMapper;
using EquipmentBorrowingManagementSystem.Application.Common;
using EquipmentBorrowingManagementSystem.Application.DTOs.Categories;
using EquipmentBorrowingManagementSystem.Application.Interfaces;
using EquipmentBorrowingManagementSystem.Application.Interfaces.Services;
using EquipmentBorrowingManagementSystem.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace EquipmentBorrowingManagementSystem.Application.Services;

public class EquipmentCategoryService : IEquipmentCategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public EquipmentCategoryService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<List<EquipmentCategoryDto>>> GetAllAsync()
    {
        var categories = await _unitOfWork.EquipmentCategories.GetAllAsync();
        return Result<List<EquipmentCategoryDto>>.Ok(_mapper.Map<List<EquipmentCategoryDto>>(categories));
    }

    public async Task<Result<EquipmentCategoryDto>> GetByIdAsync(int id)
    {
        var category = await _unitOfWork.EquipmentCategories.GetByIdAsync(id);
        if (category == null)
        {
            return Result<EquipmentCategoryDto>.Fail("Category not found.", StatusCodes.Status404NotFound);
        }

        return Result<EquipmentCategoryDto>.Ok(_mapper.Map<EquipmentCategoryDto>(category));
    }

    public async Task<Result<EquipmentCategoryDto>> CreateAsync(CreateEquipmentCategoryDto dto)
    {
        var existing = await _unitOfWork.EquipmentCategories.GetByNameAsync(dto.Name);
        if (existing != null)
        {
            return Result<EquipmentCategoryDto>.Fail("Category name already exists.", StatusCodes.Status409Conflict);
        }

        var category = new EquipmentCategory
        {
            Name = dto.Name,
            Description = dto.Description
        };

        await _unitOfWork.EquipmentCategories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return Result<EquipmentCategoryDto>.Created(_mapper.Map<EquipmentCategoryDto>(category));
    }

    public async Task<Result<EquipmentCategoryDto>> UpdateAsync(int id, UpdateEquipmentCategoryDto dto)
    {
        var category = await _unitOfWork.EquipmentCategories.GetByIdAsync(id);
        if (category == null)
        {
            return Result<EquipmentCategoryDto>.Fail("Category not found.", StatusCodes.Status404NotFound);
        }

        var existing = await _unitOfWork.EquipmentCategories.GetByNameAsync(dto.Name);
        if (existing != null && existing.Id != id)
        {
            return Result<EquipmentCategoryDto>.Fail("Category name already exists.", StatusCodes.Status409Conflict);
        }

        category.Name = dto.Name;
        category.Description = dto.Description;

        _unitOfWork.EquipmentCategories.Update(category);
        await _unitOfWork.SaveChangesAsync();

        return Result<EquipmentCategoryDto>.Ok(_mapper.Map<EquipmentCategoryDto>(category));
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var category = await _unitOfWork.EquipmentCategories.GetByIdAsync(id);
        if (category == null)
        {
            return Result.Fail("Category not found.", StatusCodes.Status404NotFound);
        }

        if (await _unitOfWork.EquipmentCategories.HasEquipmentsAsync(id))
        {
            return Result.Fail("Cannot delete category that still has equipment.", StatusCodes.Status400BadRequest);
        }

        _unitOfWork.EquipmentCategories.Delete(category);
        await _unitOfWork.SaveChangesAsync();

        return Result.NoContent("Category deleted.");
    }
}
