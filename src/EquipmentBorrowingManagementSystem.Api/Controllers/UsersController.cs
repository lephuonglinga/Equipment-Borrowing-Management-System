using EquipmentBorrowingManagementSystem.Application.Constants;
using EquipmentBorrowingManagementSystem.Application.DTOs.Users;
using EquipmentBorrowingManagementSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBorrowingManagementSystem.Api.Controllers;

[Authorize(Roles = Roles.Admin)]
[Route("api/users")]
public class UsersController : ApiControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _userService.GetAllAsync();
        return ToActionResult(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _userService.GetByIdAsync(id);
        return ToActionResult(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateStaff([FromBody] CreateUserDto dto)
    {
        var result = await _userService.CreateAsync(dto);
        return ToActionResult(result);
    }

    [HttpPut("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var result = await _userService.DeactivateAsync(id);
        return ToActionResult(result);
    }

    [HttpPut("{id:int}/activate")]
    public async Task<IActionResult> Activate(int id)
    {
        var result = await _userService.ActivateAsync(id);
        return ToActionResult(result);
    }
}
