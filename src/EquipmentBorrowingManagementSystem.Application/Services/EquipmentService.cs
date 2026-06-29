using AutoMapper;
using EquipmentBorrowingManagementSystem.Application.Common;
using EquipmentBorrowingManagementSystem.Application.DTOs;
using EquipmentBorrowingManagementSystem.Application.Interfaces;
using EquipmentBorrowingManagementSystem.Application.Interfaces.Services;
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

    public async Task<Result<List<EquipmentDto>>> GetAllAsync()
    {
        var equipment = await _unitOfWork.Equipment.GetAllWithCategoryAsync();
        return Result<List<EquipmentDto>>.Ok(_mapper.Map<List<EquipmentDto>>(equipment));
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
}
