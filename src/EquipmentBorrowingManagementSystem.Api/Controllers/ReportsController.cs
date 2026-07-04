using EquipmentBorrowingManagementSystem.Application.Constants;
using EquipmentBorrowingManagementSystem.Application.DTOs.Reports;
using EquipmentBorrowingManagementSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBorrowingManagementSystem.Api.Controllers;

[Authorize(Roles = $"{Roles.Admin},{Roles.Staff}")]
[Route("api/reports")]
public class ReportsController : ApiControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("borrow-summary")]
    public async Task<IActionResult> GetBorrowSummary([FromQuery] BorrowSummaryQueryParams query)
    {
        var result = await _reportService.GetBorrowSummaryAsync(query);
        return ToActionResult(result);
    }

    [HttpGet("overdue-requests")]
    public async Task<IActionResult> GetOverdueRequests()
    {
        var result = await _reportService.GetOverdueRequestsAsync();
        return ToActionResult(result);
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _reportService.GetDashboardStatsAsync();
        return ToActionResult(result);
    }
}
