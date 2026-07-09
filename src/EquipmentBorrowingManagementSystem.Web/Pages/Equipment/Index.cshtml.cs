using EquipmentBorrowingManagementSystem.Web.Infrastructure;
using EquipmentBorrowingManagementSystem.Web.Models;
using EquipmentBorrowingManagementSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBorrowingManagementSystem.Web.Pages.Equipment;

public class IndexModel : EbmsPageModel
{
    private readonly EbmsApiClient _api;
    private readonly BorrowCartService _cart;

    public IndexModel(EbmsApiClient api, BorrowCartService cart)
    {
        _api = api;
        _cart = cart;
    }

    public List<EquipmentCategoryDto> Categories { get; set; } = [];
    public PagedResult<EquipmentDto>? EquipmentPage { get; set; }
    public IReadOnlyList<BorrowCartItem> Cart => _cart.GetItems();

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? CategoryId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public string PageTitle { get; set; } = "Equipments";
    public string PageSubtitle { get; set; } = "Tất cả thiết bị.";
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (EnsureAuthenticated() is IActionResult redirect)
        {
            return redirect;
        }

        await LoadPageAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostToggleCartAsync(int equipmentId, CancellationToken cancellationToken)
    {
        if (EnsureAuthenticated() is IActionResult redirect)
        {
            return redirect;
        }

        try
        {
            var equipment = await _api.GetAsync<EquipmentDto>($"api/equipment/{equipmentId}", cancellationToken: cancellationToken);
            if (equipment is not null)
            {
                if (_cart.Contains(equipmentId))
                {
                    _cart.Remove(equipmentId);
                }
                else if (equipment.Status == "Available")
                {
                    _cart.Add(equipment);
                }
            }
        }
        catch (ApiException ex)
        {
            SetPageMessage(GetApiErrorMessage(ex), isError: true);
        }

        return RedirectToPage(new { Search, CategoryId, Status, PageNumber });
    }

    public IActionResult OnPostClearCart()
    {
        if (EnsureAuthenticated() is IActionResult redirect)
        {
            return redirect;
        }

        _cart.Clear();
        return RedirectToPage(new { Search, CategoryId, Status, PageNumber });
    }

    public async Task<IActionResult> OnPostSubmitBorrowAsync(
        DateTime borrowDate,
        DateTime expectedReturnDate,
        string purpose,
        CancellationToken cancellationToken)
    {
        if (EnsureAuthenticated() is IActionResult redirect)
        {
            return redirect;
        }

        var items = _cart.ToApiItems();
        if (items.Count == 0)
        {
            SetPageMessage("Chưa chọn thiết bị nào.", isError: true);
            return RedirectToPage(new { Search, CategoryId, Status, PageNumber });
        }

        try
        {
            await _api.PostAsync(
                "api/borrow-requests",
                new CreateBorrowRequestDto
                {
                    BorrowDate = borrowDate.ToUniversalTime(),
                    ExpectedReturnDate = expectedReturnDate.ToUniversalTime(),
                    Purpose = purpose.Trim(),
                    Items = items
                },
                cancellationToken: cancellationToken);

            _cart.Clear();
            return RedirectToPage("/Borrow/Index", new { tab = "pending" });
        }
        catch (ApiException ex)
        {
            SetPageMessage(GetApiErrorMessage(ex), isError: true);
            return RedirectToPage(new { Search, CategoryId, Status, PageNumber });
        }
    }

    private async Task LoadPageAsync(CancellationToken cancellationToken)
    {
        try
        {
            Categories = await _api.GetAsync<List<EquipmentCategoryDto>>("api/equipment-categories", cancellationToken: cancellationToken) ?? [];

            if (CategoryId.HasValue)
            {
                var cat = Categories.FirstOrDefault(c => c.Id == CategoryId.Value);
                if (cat is not null)
                {
                    PageTitle = $"Equipments — {cat.Name}";
                    PageSubtitle = $"Thiết bị thuộc danh mục {cat.Name}.";
                }
            }

            var query = new List<string>
            {
                $"pageNumber={PageNumber}",
                "pageSize=8",
                "sortBy=name",
                "sortDirection=asc"
            };

            if (!string.IsNullOrWhiteSpace(Search))
            {
                query.Add($"search={Uri.EscapeDataString(Search)}");
            }

            if (CategoryId.HasValue)
            {
                query.Add($"categoryId={CategoryId.Value}");
            }

            if (!string.IsNullOrWhiteSpace(Status))
            {
                query.Add($"status={Uri.EscapeDataString(Status)}");
            }

            EquipmentPage = await _api.GetAsync<PagedResult<EquipmentDto>>(
                $"api/equipment?{string.Join("&", query)}",
                cancellationToken: cancellationToken);
        }
        catch (ApiException ex)
        {
            ErrorMessage = GetApiErrorMessage(ex);
        }
    }
}
