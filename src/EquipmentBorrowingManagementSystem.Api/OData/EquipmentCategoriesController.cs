using EquipmentBorrowingManagementSystem.Domain.Entities;
using EquipmentBorrowingManagementSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowingManagementSystem.Api.OData;

/// <summary>
/// Catalog view: browse categories with nested equipments in one query.
/// REST /api/equipment-categories returns flat categories only.
/// </summary>
[Authorize]
public class EquipmentCategoriesController : ODataController
{
    private readonly AppDbContext _context;

    public EquipmentCategoriesController(AppDbContext context)
    {
        _context = context;
    }

    [EnableQuery(PageSize = 50, MaxTop = 100)]
    public ActionResult<IQueryable<EquipmentCategory>> Get()
    {
        return Ok(_context.EquipmentCategories
            .Include(c => c.Equipments)
            .AsNoTracking());
    }
}
