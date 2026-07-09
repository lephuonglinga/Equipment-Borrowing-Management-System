using EquipmentBorrowingManagementSystem.Application.Constants;
using EquipmentBorrowingManagementSystem.Application.DTOs.Borrowing;
using EquipmentBorrowingManagementSystem.Application.Interfaces.Services;
using EquipmentBorrowingManagementSystem.Domain.Entities;
using EquipmentBorrowingManagementSystem.Domain.Enums;
using EquipmentBorrowingManagementSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EquipmentBorrowingManagementSystem.Api.Controllers;

[Authorize]
[Route("api/borrow-requests")]
public class BorrowRequestsController : ApiControllerBase
{
    private readonly IBorrowRequestService _borrowRequestService;
    private readonly AppDbContext _dbContext;

    public BorrowRequestsController(IBorrowRequestService borrowRequestService, AppDbContext dbContext)
    {
        _borrowRequestService = borrowRequestService;
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (HasODataQuery())
        {
            await _borrowRequestService.ProcessExpiredApprovalsAsync();

            var query = _dbContext.BorrowRequests
                .AsNoTracking()
                .Include(b => b.User)
                .Include(b => b.ApprovedBy)
                .AsQueryable();

            if (!IsStaffOrAdmin())
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!int.TryParse(userIdClaim, out var currentUserId))
                {
                    return Unauthorized(new { message = "Phiên đăng nhập không hợp lệ." });
                }

                query = query.Where(b => b.UserId == currentUserId);
            }

            var applied = ApplyBorrowRequestOData(query, out var errorMessage);
            if (errorMessage != null)
            {
                return BadRequest(new { message = errorMessage });
            }

            try
            {
                var data = await applied
                    .Select(b => new BorrowRequestDto
                    {
                        Id = b.Id,
                        UserId = b.UserId,
                        UserName = b.User.FullName,
                        RequestDate = b.RequestDate,
                        BorrowDate = b.BorrowDate,
                        ExpectedReturnDate = b.ExpectedReturnDate,
                        Status = b.Status.ToString(),
                        Purpose = b.Purpose,
                        RejectReason = b.RejectReason,
                        ApprovedById = b.ApprovedById,
                        ApprovedByName = b.ApprovedBy != null ? b.ApprovedBy.FullName : null,
                        ApprovedAt = b.ApprovedAt
                    })
                    .ToListAsync();
                return Ok(data);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        var result = await _borrowRequestService.GetAllAsync();
        return ToActionResult(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _borrowRequestService.GetByIdAsync(id);
        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBorrowRequestDto dto)
    {
        var result = await _borrowRequestService.CreateAsync(dto);
        return ToActionResult(result);
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateBorrowRequestDto dto)
    {
        var result = await _borrowRequestService.UpdateAsync(id, dto);
        return ToActionResult(result);
    }

    private bool HasODataQuery() =>
        Request.Query.Keys.Any(key => key.StartsWith("$", StringComparison.Ordinal));

    private bool IsStaffOrAdmin() =>
        User.IsInRole(Roles.Admin) || User.IsInRole(Roles.Staff);

    private IQueryable<BorrowRequest> ApplyBorrowRequestOData(IQueryable<BorrowRequest> query, out string? errorMessage)
    {
        errorMessage = null;

        if (Request.Query.TryGetValue("$filter", out var filterValues))
        {
            var filter = filterValues.ToString().Trim();
            var parts = filter.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3)
            {
                errorMessage = "Chỉ hỗ trợ $filter theo dạng: userId eq 3 hoặc status eq 'Approved'.";
                return query;
            }

            var field = parts[0];
            var op = parts[1];
            var value = parts[2].Trim('\'', '"');
            if (op != "eq")
            {
                errorMessage = "Hiện tại chỉ hỗ trợ toán tử eq cho $filter.";
                return query;
            }

            query = field.ToLowerInvariant() switch
            {
                "userid" when int.TryParse(value, out var userId) => query.Where(r => r.UserId == userId),
                "status" when Enum.TryParse<BorrowRequestStatus>(value, true, out var status) => query.Where(r => r.Status == status),
                _ => throw new InvalidOperationException("Chỉ hỗ trợ $filter cho userId hoặc status.")
            };
        }

        if (Request.Query.TryGetValue("$orderby", out var orderValues))
        {
            var order = orderValues.ToString().Trim();
            var orderParts = order.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var field = orderParts[0];
            var desc = orderParts.Length > 1 && orderParts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

            query = field.ToLowerInvariant() switch
            {
                "requestdate" => desc ? query.OrderByDescending(r => r.RequestDate) : query.OrderBy(r => r.RequestDate),
                "borrowdate" => desc ? query.OrderByDescending(r => r.BorrowDate) : query.OrderBy(r => r.BorrowDate),
                "expectedreturndate" => desc ? query.OrderByDescending(r => r.ExpectedReturnDate) : query.OrderBy(r => r.ExpectedReturnDate),
                _ => throw new InvalidOperationException("Chỉ hỗ trợ $orderby: requestDate, borrowDate, expectedReturnDate.")
            };
        }

        if (Request.Query.TryGetValue("$skip", out var skipValues))
        {
            if (!int.TryParse(skipValues.ToString(), out var skip) || skip < 0)
            {
                errorMessage = "$skip phải là số nguyên >= 0.";
                return query;
            }
            query = query.Skip(skip);
        }

        if (Request.Query.TryGetValue("$top", out var topValues))
        {
            if (!int.TryParse(topValues.ToString(), out var top) || top <= 0)
            {
                errorMessage = "$top phải là số nguyên > 0.";
                return query;
            }
            query = query.Take(top);
        }

        return query;
    }
}
