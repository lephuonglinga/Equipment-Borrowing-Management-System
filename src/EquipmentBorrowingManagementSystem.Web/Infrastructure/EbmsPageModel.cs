using EquipmentBorrowingManagementSystem.Web.Infrastructure;
using EquipmentBorrowingManagementSystem.Web.Models;
using EquipmentBorrowingManagementSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EquipmentBorrowingManagementSystem.Web.Infrastructure;

public abstract class EbmsPageModel : PageModel
{
    protected AuthSession? CurrentAuth => HttpContext.Session.GetAuthSession();

    protected IActionResult RedirectToLogin(string? returnUrl = null)
    {
        var url = "/Account/Login";
        if (!string.IsNullOrWhiteSpace(returnUrl))
        {
            url += "?returnUrl=" + Uri.EscapeDataString(returnUrl);
        }

        return Redirect(url);
    }

    protected IActionResult RequireAuth()
    {
        if (CurrentAuth is null || string.IsNullOrWhiteSpace(CurrentAuth.AccessToken))
        {
            HttpContext.Session.ClearAuthSession();
            return RedirectToLogin(Request.Path + Request.QueryString);
        }

        return Page();
    }

    protected IActionResult? EnsureAuthenticated()
    {
        if (CurrentAuth is null || string.IsNullOrWhiteSpace(CurrentAuth.AccessToken))
        {
            HttpContext.Session.ClearAuthSession();
            return RedirectToLogin(Request.Path + Request.QueryString);
        }

        return null;
    }

    protected IActionResult? EnsureStaffOrAdmin()
    {
        var authResult = EnsureAuthenticated();
        if (authResult is not null)
        {
            return authResult;
        }

        if (CurrentAuth!.IsStaffOrAdmin)
        {
            return null;
        }

        return RedirectToPage("/Categories/Index");
    }

    protected IActionResult? EnsureAdmin()
    {
        var authResult = EnsureAuthenticated();
        if (authResult is not null)
        {
            return authResult;
        }

        if (CurrentAuth!.IsAdmin)
        {
            return null;
        }

        return RedirectToPage("/Categories/Index");
    }

    /// <summary>Only the Staff role may approve/reject/handover/return borrow requests. Admin is excluded on purpose.</summary>
    protected IActionResult? EnsureStaffOnly()
    {
        var authResult = EnsureAuthenticated();
        if (authResult is not null)
        {
            return authResult;
        }

        if (CurrentAuth!.Role == "Staff")
        {
            return null;
        }

        return RedirectToPage("/Categories/Index");
    }

    /// <summary>Only the User role may borrow equipment. Staff/Admin are excluded on purpose.</summary>
    protected IActionResult? EnsureUserOnly()
    {
        var authResult = EnsureAuthenticated();
        if (authResult is not null)
        {
            return authResult;
        }

        if (CurrentAuth!.Role == "User")
        {
            return null;
        }

        return RedirectToPage("/Categories/Index");
    }

    protected void SetPageMessage(string message, bool isError = false)
    {
        TempData[isError ? "ErrorMessage" : "SuccessMessage"] = message;
    }

    protected string GetApiErrorMessage(ApiException ex, string context = "")
    {
        if (!string.IsNullOrWhiteSpace(ex.ApiMessage))
        {
            return ex.ApiMessage;
        }

        return ex.StatusCode switch
        {
            401 when context == "login" => "Email hoặc mật khẩu không đúng.",
            401 => "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.",
            403 => "Tài khoản bị vô hiệu hoặc không có quyền truy cập.",
            0 => "Không kết nối được API.",
            _ => $"Đã xảy ra lỗi (HTTP {ex.StatusCode})."
        };
    }
}
