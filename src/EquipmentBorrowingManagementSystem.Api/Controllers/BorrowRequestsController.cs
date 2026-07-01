using EquipmentBorrowingManagementSystem.Application.Constants;
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

    [Authorize(Roles = $"{Roles.Admin},{Roles.Staff}")]
    [HttpPut("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var result = await _borrowRequestService.ApproveAsync(id);
        return ToActionResult(result);
    }

    [Authorize(Roles = $"{Roles.Admin},{Roles.Staff}")]
    [HttpPut("{id:int}/reject")]
    public async Task<IActionResult> Reject(int id, [FromBody] RejectBorrowRequestDto dto)
    {
        var result = await _borrowRequestService.RejectAsync(id, dto);
        return ToActionResult(result);
    }

    [HttpPut("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await _borrowRequestService.CancelAsync(id);
        return ToActionResult(result);
    }

    [Authorize(Roles = $"{Roles.Admin},{Roles.Staff}")]
    [HttpPut("{id:int}/return")]
    public async Task<IActionResult> Return(int id, [FromBody] ReturnBorrowRequestDto dto)
    {
        var result = await _borrowRequestService.ReturnAsync(id, dto);
        return ToActionResult(result);
    }
}
