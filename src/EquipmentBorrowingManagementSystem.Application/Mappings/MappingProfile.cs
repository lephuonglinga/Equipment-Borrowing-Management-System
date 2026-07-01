using AutoMapper;
using EquipmentBorrowingManagementSystem.Application.DTOs;
using EquipmentBorrowingManagementSystem.Domain.Entities;

namespace EquipmentBorrowingManagementSystem.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Equipment, EquipmentDto>()
            .ForMember(dest => dest.CategoryName,
                opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<EquipmentCategory, EquipmentCategoryDto>();

        CreateMap<BorrowRequestItem, BorrowRequestItemDto>()
            .ForMember(dest => dest.EquipmentName,
                opt => opt.MapFrom(src => src.Equipment != null ? src.Equipment.Name : string.Empty))
            .ForMember(dest => dest.SerialNumber,
                opt => opt.MapFrom(src => src.Equipment != null ? src.Equipment.SerialNumber : string.Empty))
            .ForMember(dest => dest.ConditionAtBorrow,
                opt => opt.MapFrom(src => src.ConditionAtBorrow != null ? src.ConditionAtBorrow.ToString() : null))
            .ForMember(dest => dest.ConditionAtReturn,
                opt => opt.MapFrom(src => src.ConditionAtReturn != null ? src.ConditionAtReturn.ToString() : null));

        CreateMap<BorrowRequest, BorrowRequestDto>()
            .ForMember(dest => dest.UserName,
                opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty))
            .ForMember(dest => dest.Status,
                opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.ApprovedByName,
                opt => opt.MapFrom(src => src.ApprovedBy != null ? src.ApprovedBy.FullName : null));
    }
}
