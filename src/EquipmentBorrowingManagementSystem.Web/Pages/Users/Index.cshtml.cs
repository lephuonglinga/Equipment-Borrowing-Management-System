using EquipmentBorrowingManagementSystem.Web.Infrastructure;
using EquipmentBorrowingManagementSystem.Web.Models;
using EquipmentBorrowingManagementSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBorrowingManagementSystem.Web.Pages.Users;

public class IndexModel : EbmsPageModel
{
    private readonly EbmsApiClient _api;

    public IndexModel(EbmsApiClient api)
    {
        _api = api;
    }

    public List<UserDto> Users { get; set; } = [];
    public string? CurrentEmail => CurrentAuth?.Email;

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (EnsureAdmin() is IActionResult redirect)
        {
            return redirect;
        }

        Users = await _api.GetAsync<List<UserDto>>("api/users", cancellationToken: cancellationToken) ?? [];
        return Page();
    }

    public async Task<IActionResult> OnPostToggleActiveAsync(int id, bool activate, CancellationToken cancellationToken)
    {
        if (EnsureAdmin() is IActionResult redirect) return redirect;

        try
        {
            await _api.PatchAsync($"api/users/{id}", new UpdateUserDto { IsActive = activate }, cancellationToken: cancellationToken);
            SetPageMessage(activate ? "Đã kích hoạt user." : "Đã vô hiệu hóa user.");
        }
        catch (ApiException ex)
        {
            SetPageMessage(GetApiErrorMessage(ex), isError: true);
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostCreateAsync(string fullName, string email, string password, CancellationToken cancellationToken)
    {
        if (EnsureAdmin() is IActionResult redirect) return redirect;

        try
        {
            await _api.PostAsync("api/users", new CreateUserDto
            {
                FullName = fullName.Trim(),
                Email = email.Trim(),
                Password = password
            }, cancellationToken: cancellationToken);
            SetPageMessage("Đã tạo tài khoản Staff.");
        }
        catch (ApiException ex)
        {
            SetPageMessage(GetApiErrorMessage(ex), isError: true);
        }

        return RedirectToPage();
    }
}
