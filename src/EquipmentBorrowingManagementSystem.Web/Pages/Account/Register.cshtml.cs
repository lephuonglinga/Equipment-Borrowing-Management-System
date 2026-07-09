using System.ComponentModel.DataAnnotations;
using EquipmentBorrowingManagementSystem.Web.Infrastructure;
using EquipmentBorrowingManagementSystem.Web.Models;
using EquipmentBorrowingManagementSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EquipmentBorrowingManagementSystem.Web.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly EbmsApiClient _api;
    private readonly AuthSessionService _authSession;

    public RegisterModel(EbmsApiClient api, AuthSessionService authSession)
    {
        _api = api;
        _authSession = authSession;
    }

    [BindProperty]
    public RegisterInput Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public IActionResult OnGet()
    {
        if (HttpContext.Session.GetAuthSession() is not null)
        {
            return RedirectToPage("/Categories/Index");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var auth = await _api.PostAsync<AuthSession>(
                "api/auth/register",
                new RegisterRequest
                {
                    FullName = Input.FullName,
                    Email = Input.Email,
                    Password = Input.Password
                },
                requireAuth: false,
                cancellationToken);

            if (auth is null)
            {
                ErrorMessage = "Đăng ký thất bại.";
                return Page();
            }

            _authSession.SaveAuth(auth);
            return RedirectToPage("/Categories/Index");
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.ApiMessage ?? "Đăng ký thất bại.";
            return Page();
        }
    }

    public class RegisterInput
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
}
