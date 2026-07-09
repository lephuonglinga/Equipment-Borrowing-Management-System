using EquipmentBorrowingManagementSystem.Web.Infrastructure;
using EquipmentBorrowingManagementSystem.Web.Models;
using EquipmentBorrowingManagementSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBorrowingManagementSystem.Web.Pages.Users;

public class DetailModel : EbmsPageModel
{
    private readonly EbmsApiClient _api;

    public DetailModel(EbmsApiClient api)
    {
        _api = api;
    }

    public UserDto? UserDetail { get; set; }
    public bool IsSelf => UserDetail is not null && CurrentAuth is not null &&
        string.Equals(UserDetail.Email, CurrentAuth.Email, StringComparison.OrdinalIgnoreCase);

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken cancellationToken)
    {
        if (EnsureAdmin() is IActionResult redirect)
        {
            return redirect;
        }

        try
        {
            UserDetail = await _api.GetAsync<UserDto>($"api/users/{id}", cancellationToken: cancellationToken);
        }
        catch (ApiException ex)
        {
            SetPageMessage(GetApiErrorMessage(ex), isError: true);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostToggleActiveAsync(int id, bool activate, CancellationToken cancellationToken)
    {
        if (EnsureAdmin() is IActionResult redirect) return redirect;

        try
        {
            UserDetail = await _api.PatchAsync<UserDto>($"api/users/{id}", new UpdateUserDto { IsActive = activate }, cancellationToken: cancellationToken);
            SetPageMessage(activate ? "Đã kích hoạt user." : "Đã vô hiệu hóa user.");
        }
        catch (ApiException ex)
        {
            SetPageMessage(GetApiErrorMessage(ex), isError: true);
        }

        return RedirectToPage(new { id });
    }
}
