using EquipmentBorrowingManagementSystem.Web.Infrastructure;
using EquipmentBorrowingManagementSystem.Web.Models;
using EquipmentBorrowingManagementSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBorrowingManagementSystem.Web.Pages.Reports;

public class IndexModel : EbmsPageModel
{
    private readonly EbmsApiClient _api;

    public IndexModel(EbmsApiClient api)
    {
        _api = api;
    }

    public DashboardStatsDto? Dashboard { get; set; }
    public List<OverdueRequestDto> OverdueRequests { get; set; } = [];
    public BorrowSummaryDto? Summary { get; set; }
    public string? ErrorMessage { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? FromDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ToDate { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (EnsureStaffOrAdmin() is IActionResult redirect)
        {
            return redirect;
        }

        try
        {
            Dashboard = await _api.GetAsync<DashboardStatsDto>("api/reports/dashboard", cancellationToken: cancellationToken);
            OverdueRequests = await _api.GetAsync<List<OverdueRequestDto>>("api/reports/overdue-requests", cancellationToken: cancellationToken) ?? [];

            var summaryUrl = "api/reports/borrow-summary?";
            if (!string.IsNullOrWhiteSpace(FromDate))
            {
                summaryUrl += $"fromDate={Uri.EscapeDataString(FromDate)}T00:00:00.000Z&";
            }

            if (!string.IsNullOrWhiteSpace(ToDate))
            {
                summaryUrl += $"toDate={Uri.EscapeDataString(ToDate)}T00:00:00.000Z&";
            }

            Summary = await _api.GetAsync<BorrowSummaryDto>(summaryUrl.TrimEnd('&', '?'), cancellationToken: cancellationToken);
        }
        catch (ApiException ex)
        {
            ErrorMessage = GetApiErrorMessage(ex);
        }

        return Page();
    }
}
