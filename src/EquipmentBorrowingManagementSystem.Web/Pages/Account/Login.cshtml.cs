using System.ComponentModel.DataAnnotations;
using EquipmentBorrowingManagementSystem.Web.Infrastructure;
using EquipmentBorrowingManagementSystem.Web.Models;
using EquipmentBorrowingManagementSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EquipmentBorrowingManagementSystem.Web.Pages.Account;

public class LoginModel : PageModel
{
    private readonly EbmsApiClient _api;
    private readonly AuthSessionService _authSession;

    public LoginModel(EbmsApiClient api, AuthSessionService authSession)
    {
        _api = api;
        _authSession = authSession;
    }

    [BindProperty]
    public LoginInput Input { get; set; } = new();

    public string? ErrorMessage { get; set; }

    public IActionResult OnGet(string? returnUrl = null, string? expired = null)
    {
        if (HttpContext.Session.GetAuthSession() is not null)
        {
            return RedirectToPage("/Categories/Index");
        }

        Input.ReturnUrl = returnUrl;
        if (expired == "1")
        {
            ErrorMessage = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
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
                "api/auth/login",
                new LoginRequest { Email = Input.Email, Password = Input.Password },
                requireAuth: false,
                cancellationToken);

            if (auth is null)
            {
                ErrorMessage = "Đăng nhập thất bại.";
                return Page();
            }

            _authSession.SaveAuth(auth);
            return Redirect(string.IsNullOrWhiteSpace(Input.ReturnUrl) ? "/Categories" : Input.ReturnUrl);
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.StatusCode switch
            {
                401 => "Email hoặc mật khẩu không đúng.",
                403 => "Tài khoản bị vô hiệu hoặc không có quyền truy cập.",
                _ => ex.ApiMessage ?? "Đăng nhập thất bại."
            };
            return Page();
        }
    }

    public class LoginInput
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public string? ReturnUrl { get; set; }
    }
}
