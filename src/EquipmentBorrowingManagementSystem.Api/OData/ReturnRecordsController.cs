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
/// Return history with related borrow request and staff who processed the return.
/// No REST list endpoint for return records.
/// </summary>
[Authorize(Roles = $"{Roles.Admin},{Roles.Staff}")]
[Produces("application/json")]
public class ReturnRecordsController : ODataController
{
    private readonly AppDbContext _context;

    public ReturnRecordsController(AppDbContext context)
    {
        _context = context;
    }

    [EnableQuery(PageSize = 50, MaxTop = 100)]
    public ActionResult<IQueryable<ReturnRecord>> Get()
    {
        return Ok(_context.ReturnRecords
            .Include(r => r.BorrowRequest)
            .ThenInclude(b => b.User)
            .Include(r => r.BorrowRequest)
            .ThenInclude(b => b.Items)
            .ThenInclude(i => i.Equipment)
            .Include(r => r.ReturnedBy)
            .AsNoTracking());
    }
}
