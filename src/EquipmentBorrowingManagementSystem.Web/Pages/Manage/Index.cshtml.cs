using EquipmentBorrowingManagementSystem.Web.Infrastructure;
using EquipmentBorrowingManagementSystem.Web.Models;
using EquipmentBorrowingManagementSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBorrowingManagementSystem.Web.Pages.Manage;

public class IndexModel : EbmsPageModel
{
    private readonly EbmsApiClient _api;

    public IndexModel(EbmsApiClient api)
    {
        _api = api;
    }

    public List<EquipmentCategoryDto> Categories { get; set; } = [];
    public PagedResult<EquipmentDto>? EquipmentPage { get; set; }
    public EquipmentDto? EditEquipment { get; set; }
    public EquipmentCategoryDto? EditCategory { get; set; }

    [BindProperty(SupportsGet = true)]
    public string Tab { get; set; } = "equipment";

    [BindProperty(SupportsGet = true, Name = "status")]
    public string? StatusFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int? EditEquipmentId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? EditCategoryId { get; set; }

    public string ActiveTab => Tab == "categories" ? "categories" : "equipment";
    public bool ShowEquipmentModal => EditEquipmentId.HasValue && EditEquipment is not null;
    public bool ShowCategoryModal => EditCategoryId.HasValue;

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (EnsureStaffOrAdmin() is IActionResult redirect)
        {
            return redirect;
        }

        await LoadDataAsync(cancellationToken);
        return Page();
    }

    public async Task<IActionResult> OnPostSaveEquipmentAsync(
        int? id, string name, string serialNumber, int categoryId, string? location, string? description, string? imageUrl,
        string? statusValue, string tab, string? status, int pageNumber, CancellationToken cancellationToken)
    {
        if (EnsureStaffOrAdmin() is IActionResult redirect) return redirect;

        var nameError = FormValidation.RequireText(name, "Tên thiết bị");
        if (nameError is not null)
        {
            SetPageMessage(nameError, isError: true);
            return RedirectToPage(new { tab, status, pageNumber });
        }

        var serialError = FormValidation.RequireText(serialNumber, "Số serial");
        if (serialError is not null)
        {
            SetPageMessage(serialError, isError: true);
            return RedirectToPage(new { tab, status, pageNumber });
        }

        var locationError = FormValidation.RequireText(location, "Vị trí");
        if (locationError is not null)
        {
            SetPageMessage(locationError, isError: true);
            return RedirectToPage(new { tab, status, pageNumber });
        }

        try
        {
            if (id.HasValue)
            {
                await _api.PutAsync($"api/equipment/{id.Value}", new UpdateEquipmentDto
                {
                    Name = name.Trim(),
                    SerialNumber = serialNumber.Trim(),
                    CategoryId = categoryId,
                    Status = statusValue ?? "Available",
                    Location = location.Trim(),
                    Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
                    ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim()
                }, cancellationToken: cancellationToken);
                SetPageMessage("Đã cập nhật thiết bị.");
            }
            else
            {
                await _api.PostAsync("api/equipment", new CreateEquipmentDto
                {
                    Name = name.Trim(),
                    SerialNumber = serialNumber.Trim(),
                    CategoryId = categoryId,
                    Location = location.Trim(),
                    Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim(),
                    ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim()
                }, cancellationToken: cancellationToken);
                SetPageMessage("Đã thêm thiết bị.");
            }
        }
        catch (ApiException ex)
        {
            SetPageMessage(GetApiErrorMessage(ex), isError: true);
        }

        return RedirectToPage(new { tab, status, pageNumber });
    }

    public async Task<IActionResult> OnPostDeleteEquipmentAsync(int id, string tab, string? status, int pageNumber, CancellationToken cancellationToken)
    {
        if (EnsureStaffOrAdmin() is IActionResult redirect) return redirect;
        try
        {
            await _api.DeleteAsync($"api/equipment/{id}", cancellationToken: cancellationToken);
            SetPageMessage("Đã xóa thiết bị.");
        }
        catch (ApiException ex) { SetPageMessage(GetApiErrorMessage(ex), isError: true); }
        return RedirectToPage(new { tab, status, pageNumber });
    }

    public async Task<IActionResult> OnPostCompleteMaintenanceAsync(
        int id, string targetStatus, string tab, string? status, int pageNumber, CancellationToken cancellationToken)
    {
        if (EnsureStaffOrAdmin() is IActionResult redirect) return redirect;

        if (!FormValidation.MaintenanceCompleteStatuses.Contains(targetStatus))
        {
            SetPageMessage("Sau bảo trì chỉ được chọn Available hoặc Retired.", isError: true);
            return RedirectToPage(new { tab, status, pageNumber });
        }

        return await UpdateEquipmentStatusAsync(id, targetStatus, "Đã hoàn tất bảo trì.", tab, status, pageNumber, cancellationToken);
    }

    public async Task<IActionResult> OnPostSaveCategoryAsync(int? id, string name, string? description, CancellationToken cancellationToken)
    {
        if (EnsureStaffOrAdmin() is IActionResult redirect) return redirect;

        var nameError = FormValidation.RequireText(name, "Tên danh mục");
        if (nameError is not null)
        {
            SetPageMessage(nameError, isError: true);
            return RedirectToPage(new { tab = "categories" });
        }

        try
        {
            var body = new { name = name.Trim(), description = string.IsNullOrWhiteSpace(description) ? null : description.Trim() };
            if (id.HasValue)
            {
                await _api.PutAsync($"api/equipment-categories/{id.Value}", body, cancellationToken: cancellationToken);
                SetPageMessage("Đã cập nhật danh mục.");
            }
            else
            {
                await _api.PostAsync("api/equipment-categories", body, cancellationToken: cancellationToken);
                SetPageMessage("Đã thêm danh mục.");
            }
        }
        catch (ApiException ex) { SetPageMessage(GetApiErrorMessage(ex), isError: true); }

        return RedirectToPage(new { tab = "categories" });
    }

    public async Task<IActionResult> OnPostDeleteCategoryAsync(int id, CancellationToken cancellationToken)
    {
        if (EnsureStaffOrAdmin() is IActionResult redirect) return redirect;
        try
        {
            await _api.DeleteAsync($"api/equipment-categories/{id}", cancellationToken: cancellationToken);
            SetPageMessage("Đã xóa danh mục.");
        }
        catch (ApiException ex) { SetPageMessage(GetApiErrorMessage(ex), isError: true); }
        return RedirectToPage(new { tab = "categories" });
    }

    private async Task<IActionResult> UpdateEquipmentStatusAsync(int id, string status, string message, string tab, string? statusFilter, int pageNumber, CancellationToken cancellationToken)
    {
        try
        {
            var eq = await _api.GetAsync<EquipmentDto>($"api/equipment/{id}", cancellationToken: cancellationToken);
            if (eq is null) throw new ApiException(404, "Không tìm thấy thiết bị.");
            await _api.PutAsync($"api/equipment/{id}", new UpdateEquipmentDto
            {
                Name = eq.Name,
                SerialNumber = eq.SerialNumber,
                CategoryId = eq.CategoryId,
                Status = status,
                Location = eq.Location ?? string.Empty,
                Description = eq.Description,
                ImageUrl = eq.ImageUrl
            }, cancellationToken: cancellationToken);
            SetPageMessage(message);
        }
        catch (ApiException ex) { SetPageMessage(GetApiErrorMessage(ex), isError: true); }
        return RedirectToPage(new { tab, status = statusFilter, pageNumber });
    }

    private async Task LoadDataAsync(CancellationToken cancellationToken)
    {
        Categories = await _api.GetAsync<List<EquipmentCategoryDto>>("api/equipment-categories", cancellationToken: cancellationToken) ?? [];

        if (EditEquipmentId.HasValue)
        {
            EditEquipment = await _api.GetAsync<EquipmentDto>($"api/equipment/{EditEquipmentId.Value}", cancellationToken: cancellationToken);
            if (EditEquipment is not null && !FormValidation.CanEditEquipment(EditEquipment.Status))
            {
                SetPageMessage("Thiết bị này không thể chỉnh sửa.", isError: true);
                EditEquipment = null;
            }
        }

        if (EditCategoryId.HasValue)
        {
            EditCategory = Categories.FirstOrDefault(c => c.Id == EditCategoryId.Value)
                ?? await _api.GetAsync<EquipmentCategoryDto>($"api/equipment-categories/{EditCategoryId.Value}", cancellationToken: cancellationToken);
        }

        var query = $"api/equipment?pageNumber={PageNumber}&pageSize=10&sortBy=name&sortDirection=asc";
        if (!string.IsNullOrWhiteSpace(StatusFilter))
        {
            query += $"&status={Uri.EscapeDataString(StatusFilter)}";
        }

        EquipmentPage = await _api.GetAsync<PagedResult<EquipmentDto>>(query, cancellationToken: cancellationToken);
    }
}
