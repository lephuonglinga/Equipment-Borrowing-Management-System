using EquipmentBorrowingManagementSystem.Web.Infrastructure;
using EquipmentBorrowingManagementSystem.Web.Models;
using EquipmentBorrowingManagementSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBorrowingManagementSystem.Web.Pages.Borrow;

public class IndexModel : EbmsPageModel
{
    private static readonly Dictionary<string, string[]> TabStatuses = new()
    {
        ["pending"] = ["Pending"],
        ["pickup"] = ["Approved"],
        ["active"] = ["InProgress", "Overdue"],
        ["history"] = ["Completed", "Rejected", "Cancelled"]
    };

    private readonly EbmsApiClient _api;

    public IndexModel(EbmsApiClient api)
    {
        _api = api;
    }

    public List<BorrowRequestDto> AllRequests { get; set; } = [];
    public List<BorrowRequestDto> FilteredRequests { get; set; } = [];
    public BorrowRequestDto? DetailRequest { get; set; }

    [BindProperty(SupportsGet = true)]
    public string Tab { get; set; } = "pending";

    [BindProperty(SupportsGet = true)]
    public int? DetailId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Action { get; set; }

    public string ActiveTab => TabStatuses.ContainsKey(Tab) ? Tab : "pending";
    public bool IsStaff => CurrentAuth?.Role == "Staff";
    public string? ActionMode => Action;
    public string Subtitle => IsStaff
        ? "Duyệt đơn → Bàn giao → Nhận trả."
        : "Theo dõi trạng thái các yêu cầu mượn thiết bị.";

    public int CountByTab(string tab) =>
        AllRequests.Count(r => TabStatuses.TryGetValue(tab, out var statuses) && statuses.Contains(r.Status));

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (EnsureAuthenticated() is IActionResult redirect)
        {
            return redirect;
        }

        if (CurrentAuth!.IsAdmin)
        {
            return RedirectToPage("/Categories/Index");
        }

        await LoadRequestsAsync(cancellationToken);

        if (DetailId.HasValue)
        {
            DetailRequest = AllRequests.FirstOrDefault(r => r.Id == DetailId.Value)
                ?? await _api.GetAsync<BorrowRequestDto>($"api/borrow-requests/{DetailId.Value}", cancellationToken: cancellationToken);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostCancelAsync(int id, string tab, CancellationToken cancellationToken)
    {
        if (EnsureAuthenticated() is IActionResult redirect) return redirect;
        return await PatchAndRedirectAsync(id, new UpdateBorrowRequestDto { Status = "Cancelled" }, tab, "Đã hủy yêu cầu.", cancellationToken);
    }

    public async Task<IActionResult> OnPostApproveAsync(int id, string tab, CancellationToken cancellationToken)
    {
        if (EnsureStaffOnly() is IActionResult redirect) return redirect;
        return await PatchAndRedirectAsync(id, new UpdateBorrowRequestDto { Status = "Approved" }, tab, "Đã duyệt — chờ bàn giao thiết bị.", cancellationToken);
    }

    public async Task<IActionResult> OnPostRejectAsync(int id, string tab, string rejectReason, CancellationToken cancellationToken)
    {
        if (EnsureStaffOnly() is IActionResult redirect) return redirect;

        if (string.IsNullOrWhiteSpace(rejectReason))
        {
            SetPageMessage("Vui lòng nhập lý do từ chối.", isError: true);
            return RedirectToPage(new { tab });
        }

        return await PatchAndRedirectAsync(id, new UpdateBorrowRequestDto { Status = "Rejected", RejectReason = rejectReason.Trim() }, tab, "Đã từ chối yêu cầu.", cancellationToken);
    }

    public async Task<IActionResult> OnPostHandoverAsync(int id, string tab, Dictionary<int, string> notes, CancellationToken cancellationToken)
    {
        if (EnsureStaffOnly() is IActionResult redirect) return redirect;

        var request = await _api.GetAsync<BorrowRequestDto>($"api/borrow-requests/{id}", cancellationToken: cancellationToken);
        var items = request?.Items.Select(i => new UpdateBorrowRequestItemDto
        {
            EquipmentId = i.EquipmentId,
            Note = notes.TryGetValue(i.EquipmentId, out var note) && !string.IsNullOrWhiteSpace(note) ? note.Trim() : null
        }).ToList();

        return await PatchAndRedirectAsync(id, new UpdateBorrowRequestDto { Status = "InProgress", Items = items }, tab, "Đã bàn giao thiết bị.", cancellationToken);
    }

    public async Task<IActionResult> OnPostReturnAsync(
        int id,
        string tab,
        string? staffNote,
        Dictionary<int, string> notes,
        Dictionary<int, string> statuses,
        CancellationToken cancellationToken)
    {
        if (EnsureStaffOnly() is IActionResult redirect) return redirect;

        var request = await _api.GetAsync<BorrowRequestDto>($"api/borrow-requests/{id}", cancellationToken: cancellationToken);
        if (request is null)
        {
            SetPageMessage("Không tìm thấy yêu cầu mượn.", isError: true);
            return RedirectToPage(new { tab });
        }

        var items = new List<UpdateBorrowRequestItemDto>();
        foreach (var item in request.Items)
        {
            if (!statuses.TryGetValue(item.EquipmentId, out var status) || string.IsNullOrWhiteSpace(status))
            {
                SetPageMessage($"Vui lòng chọn trạng thái cho thiết bị {item.EquipmentName}.", isError: true);
                return RedirectToPage(new { tab, detailId = id, action = "return" });
            }

            if (!FormValidation.ReturnStatuses.Contains(status))
            {
                SetPageMessage("Trạng thái trả không hợp lệ.", isError: true);
                return RedirectToPage(new { tab, detailId = id, action = "return" });
            }

            items.Add(new UpdateBorrowRequestItemDto
            {
                EquipmentId = item.EquipmentId,
                Status = status,
                Note = notes.TryGetValue(item.EquipmentId, out var note) && !string.IsNullOrWhiteSpace(note) ? note.Trim() : null
            });
        }

        return await PatchAndRedirectAsync(id, new UpdateBorrowRequestDto
        {
            Status = "Completed",
            StaffNote = string.IsNullOrWhiteSpace(staffNote) ? null : staffNote.Trim(),
            Items = items
        }, tab, "Đã xác nhận trả thiết bị.", cancellationToken);
    }

    private async Task<IActionResult> PatchAndRedirectAsync(int id, UpdateBorrowRequestDto body, string tab, string message, CancellationToken cancellationToken)
    {
        try
        {
            await _api.PatchAsync($"api/borrow-requests/{id}", body, cancellationToken: cancellationToken);
            SetPageMessage(message);
        }
        catch (ApiException ex)
        {
            SetPageMessage(GetApiErrorMessage(ex), isError: true);
        }

        return RedirectToPage(new { tab });
    }

    private async Task LoadRequestsAsync(CancellationToken cancellationToken)
    {
        try
        {
            AllRequests = await _api.GetAsync<List<BorrowRequestDto>>("api/borrow-requests", cancellationToken: cancellationToken) ?? [];
            var statuses = TabStatuses[ActiveTab];
            FilteredRequests = AllRequests.Where(r => statuses.Contains(r.Status)).ToList();
        }
        catch (ApiException ex)
        {
            SetPageMessage(GetApiErrorMessage(ex), isError: true);
        }
    }
}
