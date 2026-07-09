using EquipmentBorrowingManagementSystem.Web.Infrastructure;
using EquipmentBorrowingManagementSystem.Web.Models;
using EquipmentBorrowingManagementSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EquipmentBorrowingManagementSystem.Web.Pages.Account;

public class LogoutModel : PageModel
{
    private readonly EbmsApiClient _api;
    private readonly AuthSessionService _authSession;

    public LogoutModel(EbmsApiClient api, AuthSessionService authSession)
    {
        _api = api;
        _authSession = authSession;
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var auth = HttpContext.Session.GetAuthSession();
        if (auth is not null && !string.IsNullOrWhiteSpace(auth.RefreshToken))
        {
            try
            {
                await _api.PostAsync(
                    "api/auth/logout",
                    new RefreshTokenRequest { RefreshToken = auth.RefreshToken },
                    requireAuth: false,
                    cancellationToken);
            }
            catch
            {
                // Ignore logout API errors; still clear local session.
            }
        }

        _authSession.ClearAuth();
        return RedirectToPage("/Account/Login");
    }
}
