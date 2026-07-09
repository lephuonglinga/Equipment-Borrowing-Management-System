using EquipmentBorrowingManagementSystem.Application.DTOs.Borrowing;
using EquipmentBorrowingManagementSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBorrowingManagementSystem.Api.Controllers;

[Authorize]
[Route("api/borrow-requests")]
public class BorrowRequestsController : ApiControllerBase
{
    private readonly IBorrowRequestService _borrowRequestService;

    public BorrowRequestsController(IBorrowRequestService borrowRequestService)
    {
        _borrowRequestService = borrowRequestService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
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
}
