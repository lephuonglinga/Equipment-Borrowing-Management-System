using EquipmentBorrowingManagementSystem.Web.Infrastructure;
using EquipmentBorrowingManagementSystem.Web.Models;
using EquipmentBorrowingManagementSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBorrowingManagementSystem.Web.Pages.Equipment;

public class DetailModel : EbmsPageModel
{
    private readonly EbmsApiClient _api;
    private readonly BorrowCartService _cart;

    public DetailModel(EbmsApiClient api, BorrowCartService cart)
    {
        _api = api;
        _cart = cart;
    }

    public EquipmentDto? Equipment { get; set; }
    public int? CategoryId { get; set; }
    public bool InCart { get; set; }
    public int CartCount => _cart.Count;
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id, int? categoryId, CancellationToken cancellationToken)
    {
        if (EnsureAuthenticated() is IActionResult redirect)
        {
            return redirect;
        }

        CategoryId = categoryId;
        await LoadEquipmentAsync(id, cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostToggleCartAsync(int id, int? categoryId, CancellationToken cancellationToken)
    {
        if (EnsureAuthenticated() is IActionResult redirect)
        {
            return redirect;
        }

        CategoryId = categoryId;
        if (Equipment is null)
        {
            await LoadEquipmentAsync(id, cancellationToken);
        }

        if (Equipment is not null)
        {
            if (_cart.Contains(id))
            {
                _cart.Remove(id);
            }
            else if (Equipment.Status == "Available")
            {
                _cart.Add(Equipment);
            }
        }

        return RedirectToPage(new { id, categoryId });
    }

    public IActionResult OnPostClearCart(int id, int? categoryId)
    {
        if (EnsureAuthenticated() is IActionResult redirect)
        {
            return redirect;
        }

        _cart.Clear();
        return RedirectToPage(new { id, categoryId });
    }

    private async Task LoadEquipmentAsync(int id, CancellationToken cancellationToken)
    {
        try
        {
            Equipment = await _api.GetAsync<EquipmentDto>($"api/equipment/{id}", cancellationToken: cancellationToken);
            InCart = _cart.Contains(id);
        }
        catch (ApiException ex)
        {
            ErrorMessage = GetApiErrorMessage(ex);
        }
    }
}
