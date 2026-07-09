using AutoMapper;
using AutoMapper.QueryableExtensions;
using EquipmentBorrowingManagementSystem.Application.DTOs.OData;
using EquipmentBorrowingManagementSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowingManagementSystem.Api.OData;


//GET /api/equipment?pageNumber=1&pageSize=10&status=Available
//GET /odata/Equipment?$filter=status eq 'Available'&$orderby=name&$top=10&$expand=category
[Authorize]
public class EquipmentController : ODataController
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public EquipmentController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [EnableQuery(MaxExpansionDepth = 1)]
    public IActionResult Get()
    {
        var equipment = _context.Equipments
            .AsNoTracking()
            .ProjectTo<EquipmentODataDto>(_mapper.ConfigurationProvider);

        return Ok(equipment);
    }
}
