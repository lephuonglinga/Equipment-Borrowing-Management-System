using System.Security.Claims;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using EquipmentBorrowingManagementSystem.Application.Constants;
using EquipmentBorrowingManagementSystem.Application.DTOs.OData;
using EquipmentBorrowingManagementSystem.Application.Interfaces.Services;
using EquipmentBorrowingManagementSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowingManagementSystem.Api.OData;

//GET /api/borrow-requests
//GET /odata/BorrowRequests?$filter=status eq 'Approved'&$expand=items&$top=5
[Authorize]
public class BorrowRequestsController : ODataController
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IBorrowRequestService _borrowRequestService;

    public BorrowRequestsController(
        AppDbContext context,
        IMapper mapper,
        IBorrowRequestService borrowRequestService)
    {
        _context = context;
        _mapper = mapper;
        _borrowRequestService = borrowRequestService;
    }

    [EnableQuery(MaxExpansionDepth = 1)]
    public async Task<IActionResult> Get()
    {
        await _borrowRequestService.ProcessOverdueTransitionsAsync();

        var query = _context.BorrowRequests.AsNoTracking().AsQueryable();

        if (!IsStaffOrAdmin())
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var currentUserId))
            {
                return Unauthorized(new { message = "Phiên đăng nhập không hợp lệ." });
            }

            query = query.Where(b => b.UserId == currentUserId);
        }

        var borrowRequests = query.ProjectTo<BorrowRequestODataDto>(_mapper.ConfigurationProvider);
        return Ok(borrowRequests);
    }

    private bool IsStaffOrAdmin() =>
        User.IsInRole(Roles.Admin) || User.IsInRole(Roles.Staff);
}
