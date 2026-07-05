using EquipmentBorrowingManagementSystem.Application.Constants;
using EquipmentBorrowingManagementSystem.Domain.Entities;
using EquipmentBorrowingManagementSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowingManagementSystem.Api.OData;

/// <summary>
/// Line-item level queries across all borrow requests (e.g. damaged returns, active borrows).
/// REST only exposes items nested inside /api/borrow-requests/{id}.
/// </summary>
[Authorize(Roles = $"{Roles.Admin},{Roles.Staff}")]
public class BorrowRequestItemsController : ODataController
{
    private readonly AppDbContext _context;

    public BorrowRequestItemsController(AppDbContext context)
    {
        _context = context;
    }

    [EnableQuery(PageSize = 50, MaxTop = 100)]
    public ActionResult<IQueryable<BorrowRequestItem>> Get()
    {
        return Ok(_context.BorrowRequestItems
            .Include(i => i.Equipment)
            .Include(i => i.BorrowRequest)
            .ThenInclude(b => b.User)
            .AsNoTracking());
    }
}
