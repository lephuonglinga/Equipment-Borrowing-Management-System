using EquipmentBorrowingManagementSystem.Web.Infrastructure;
using EquipmentBorrowingManagementSystem.Web.Models;
using EquipmentBorrowingManagementSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBorrowingManagementSystem.Web.Pages.Notifications;

public class IndexModel : EbmsPageModel
{
    private readonly EbmsApiClient _api;

    public IndexModel(EbmsApiClient api)
    {
        _api = api;
    }

    public List<NotificationDto> Notifications { get; set; } = [];
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (EnsureAuthenticated() is IActionResult redirect)
        {
            return redirect;
        }

        try
        {
            Notifications = await _api.GetAsync<List<NotificationDto>>("api/notifications", cancellationToken: cancellationToken) ?? [];
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostMarkReadAsync(int id, CancellationToken cancellationToken)
    {
        if (EnsureAuthenticated() is IActionResult redirect)
        {
            return redirect;
        }

        try
        {
            await _api.PatchAsync($"api/notifications/{id}/read", new { }, cancellationToken: cancellationToken);
        }
        catch
        {
            // keep UX simple: silently ignore and reload list
        }

        return RedirectToPage();
    }
}
