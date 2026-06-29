using EquipmentBorrowingManagementSystem.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBorrowingManagementSystem.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult ToActionResult(Result result)
    {
        if (result.Success && result.StatusCode == StatusCodes.Status204NoContent)
        {
            return NoContent();
        }

        return StatusCode(result.StatusCode, new { message = result.Message });
    }

    protected IActionResult ToActionResult<T>(Result<T> result)
    {
        if (result.Success)
        {
            return StatusCode(result.StatusCode, result.Data);
        }

        return StatusCode(result.StatusCode, new { message = result.Message });
    }
}
