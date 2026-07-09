using EquipmentBorrowingManagementSystem.Application.Constants;
using EquipmentBorrowingManagementSystem.Application.DTOs.Equipment;
using EquipmentBorrowingManagementSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBorrowingManagementSystem.Api.Controllers;

[Authorize]
[Route("api/[controller]")]
[Produces("application/json", "application/xml")]
[Consumes("application/json", "application/xml")]
public class EquipmentController : ApiControllerBase
{
    private readonly IEquipmentService _equipmentService;

    public EquipmentController(IEquipmentService equipmentService)
    {
        _equipmentService = equipmentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] EquipmentQueryParams query)
    {
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
}
