using EquipmentBorrowingManagementSystem.Application.Constants;
using EquipmentBorrowingManagementSystem.Application.DTOs.Audit;
using EquipmentBorrowingManagementSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBorrowingManagementSystem.Api.Controllers;

[Authorize(Roles = Roles.Admin)]
[Route("api/audit-logs")]
public class AuditLogsController : ApiControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] AuditLogQueryParams query)
    {
        var result = await _auditLogService.GetPagedAsync(query);
        return ToActionResult(result);
    }
}
