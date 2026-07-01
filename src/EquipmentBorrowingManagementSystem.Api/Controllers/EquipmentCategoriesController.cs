using EquipmentBorrowingManagementSystem.Application.Constants;
using EquipmentBorrowingManagementSystem.Application.DTOs.Categories;
using EquipmentBorrowingManagementSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBorrowingManagementSystem.Api.Controllers;

[Authorize]
[Route("api/equipment-categories")]
public class EquipmentCategoriesController : ApiControllerBase
{
    private readonly IEquipmentCategoryService _categoryService;

    public EquipmentCategoriesController(IEquipmentCategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _categoryService.GetAllAsync();
        return ToActionResult(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _categoryService.GetByIdAsync(id);
        return ToActionResult(result);
    }

    [Authorize(Roles = $"{Roles.Admin},{Roles.Staff}")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEquipmentCategoryDto dto)
    {
        var result = await _categoryService.CreateAsync(dto);
        return ToActionResult(result);
    }

    [Authorize(Roles = $"{Roles.Admin},{Roles.Staff}")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEquipmentCategoryDto dto)
    {
        var result = await _categoryService.UpdateAsync(id, dto);
        return ToActionResult(result);
    }

    [Authorize(Roles = $"{Roles.Admin},{Roles.Staff}")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _categoryService.DeleteAsync(id);
        return ToActionResult(result);
    }
}
