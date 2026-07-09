using EquipmentBorrowingManagementSystem.Application.Constants;
using EquipmentBorrowingManagementSystem.Application.DTOs.Equipment;
using EquipmentBorrowingManagementSystem.Application.Interfaces.Services;
using EquipmentBorrowingManagementSystem.Domain.Entities;
using EquipmentBorrowingManagementSystem.Domain.Enums;
using EquipmentBorrowingManagementSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowingManagementSystem.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[Produces("application/json", "application/xml")]
[Consumes("application/json", "application/xml")]
public class EquipmentController : ApiControllerBase
{
    private readonly IEquipmentService _equipmentService;
    private readonly AppDbContext _dbContext;

    public EquipmentController(IEquipmentService equipmentService, AppDbContext dbContext)
    {
        _equipmentService = equipmentService;
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] EquipmentQueryParams query)
    {
        if (HasODataQuery())
        {
            var entityQuery = _dbContext.Equipments
                .AsNoTracking()
                .Include(e => e.Category)
                .AsQueryable();

            var applied = ApplyEquipmentOData(entityQuery, out var errorMessage);
            if (errorMessage != null)
            {
                return BadRequest(new { message = errorMessage });
            }

            try
            {
                var data = await applied
                    .Select(e => new EquipmentDto
                    {
                        Id = e.Id,
                        Name = e.Name,
                        SerialNumber = e.SerialNumber,
                        CategoryId = e.CategoryId,
                        CategoryName = e.Category.Name,
                        Status = e.Status.ToString(),
                        Location = e.Location,
                        Description = e.Description,
                        ImageUrl = e.ImageUrl
                    })
                    .ToListAsync();
                return Ok(data);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        var result = await _equipmentService.GetPagedAsync(query);
        return ToActionResult(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _equipmentService.GetByIdAsync(id);
        return ToActionResult(result);
    }

    [Authorize(Roles = $"{Roles.Admin},{Roles.Staff}")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEquipmentDto dto)
    {
        var result = await _equipmentService.CreateAsync(dto);
        return ToActionResult(result);
    }

    [Authorize(Roles = $"{Roles.Admin},{Roles.Staff}")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEquipmentDto dto)
    {
        var result = await _equipmentService.UpdateAsync(id, dto);
        return ToActionResult(result);
    }

    [Authorize(Roles = $"{Roles.Admin},{Roles.Staff}")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _equipmentService.DeleteAsync(id);
        return ToActionResult(result);
    }

    private bool HasODataQuery() =>
        Request.Query.Keys.Any(key => key.StartsWith("$", StringComparison.Ordinal));

    private IQueryable<Equipment> ApplyEquipmentOData(IQueryable<Equipment> query, out string? errorMessage)
    {
        errorMessage = null;

        if (Request.Query.TryGetValue("$filter", out var filterValues))
        {
            var filter = filterValues.ToString().Trim();
            var parts = filter.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3 || !parts[0].Equals("status", StringComparison.OrdinalIgnoreCase) || parts[1] != "eq")
            {
                errorMessage = "Chỉ hỗ trợ $filter theo dạng: status eq 'Available'.";
                return query;
            }

            var status = parts[2].Trim('\'', '"');
            if (!Enum.TryParse<EquipmentStatus>(status, true, out var parsedStatus))
            {
                errorMessage = "Giá trị status không hợp lệ.";
                return query;
            }

            query = query.Where(e => e.Status == parsedStatus);
        }

        if (Request.Query.TryGetValue("$orderby", out var orderValues))
        {
            var order = orderValues.ToString().Trim();
            var orderParts = order.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var field = orderParts[0];
            var desc = orderParts.Length > 1 && orderParts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

            query = field.ToLowerInvariant() switch
            {
                "name" => desc ? query.OrderByDescending(e => e.Name) : query.OrderBy(e => e.Name),
                "serialnumber" => desc ? query.OrderByDescending(e => e.SerialNumber) : query.OrderBy(e => e.SerialNumber),
                "status" => desc ? query.OrderByDescending(e => e.Status) : query.OrderBy(e => e.Status),
                _ => throw new InvalidOperationException("Chỉ hỗ trợ $orderby: name, serialNumber, status.")
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
