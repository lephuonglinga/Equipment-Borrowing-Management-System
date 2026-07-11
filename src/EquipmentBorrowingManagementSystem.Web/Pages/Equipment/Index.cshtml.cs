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
    public IReadOnlyList<BorrowCartItem> Cart => CanBorrow ? _cart.GetItems() : [];
    public bool CanBorrow => CurrentAuth?.Role == "User";
    public bool HasOverdueBorrow { get; set; }

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
        if (EnsureUserOnly() is IActionResult redirect)
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
                    if (await UserHasOverdueBorrowAsync(cancellationToken))
                    {
                        SetPageMessage(FormValidation.UserHasOverdueMessage, isError: true);
                        return RedirectToPage(new { Search, CategoryId, Status, PageNumber });
                    }

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
        if (EnsureUserOnly() is IActionResult redirect)
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
        if (EnsureUserOnly() is IActionResult redirect)
        {
            return redirect;
        }

        if (await UserHasOverdueBorrowAsync(cancellationToken))
        {
            _cart.Clear();
            SetPageMessage(FormValidation.UserHasOverdueMessage, isError: true);
            return RedirectToPage(new { Search, CategoryId, Status, PageNumber });
        }

        var items = _cart.ToApiItems();
        if (items.Count == 0)
        {
            SetPageMessage("Chưa chọn thiết bị nào.", isError: true);
            return RedirectToPage(new { Search, CategoryId, Status, PageNumber });
        }

        var borrowDay = borrowDate.Date;
        var returnDay = expectedReturnDate.Date;
        var today = DateTime.Today;

        if (borrowDay < today)
        {
            SetPageMessage("Ngày mượn không được ở trong quá khứ.", isError: true);
            return RedirectToPage(new { Search, CategoryId, Status, PageNumber });
        }

        if (returnDay < borrowDay)
        {
            SetPageMessage("Ngày trả dự kiến phải sau hoặc bằng ngày mượn.", isError: true);
            return RedirectToPage(new { Search, CategoryId, Status, PageNumber });
        }

        if (string.IsNullOrWhiteSpace(purpose))
        {
            SetPageMessage("Mục đích là bắt buộc.", isError: true);
            return RedirectToPage(new { Search, CategoryId, Status, PageNumber });
        }

        try
        {
            await _api.PostAsync(
                "api/borrow-requests",
                new CreateBorrowRequestDto
                {
                    BorrowDate = DateTime.SpecifyKind(borrowDay, DateTimeKind.Utc),
                    ExpectedReturnDate = DateTime.SpecifyKind(returnDay, DateTimeKind.Utc),
                    Purpose = purpose.Trim(),
                    Items = items
                },
                cancellationToken: cancellationToken);

            _cart.Clear();
            SetPageMessage("Đăng ký mượn thành công.");
            return RedirectToPage("/Borrow/Index", new { tab = "pending" });
        }
        catch (ApiException ex)
        {
            var message = !string.IsNullOrWhiteSpace(ex.ApiMessage)
                ? ex.ApiMessage
                : GetApiErrorMessage(ex);
            SetPageMessage(message, isError: true);
            return RedirectToPage(new { Search, CategoryId, Status, PageNumber });
        }
    }

    private async Task LoadPageAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (CanBorrow)
            {
                HasOverdueBorrow = await UserHasOverdueBorrowAsync(cancellationToken);
                if (HasOverdueBorrow && _cart.Count > 0)
                {
                    _cart.Clear();
                }
            }

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

    private async Task<bool> UserHasOverdueBorrowAsync(CancellationToken cancellationToken)
    {
        try
        {
            var requests = await _api.GetAsync<List<BorrowRequestDto>>(
                "api/borrow-requests",
                cancellationToken: cancellationToken) ?? [];
            return requests.Any(r => r.Status == "Overdue");
        }
        catch (ApiException)
        {
            return false;
        }
    }
}
