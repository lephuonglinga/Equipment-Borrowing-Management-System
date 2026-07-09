using System.Net;
using System.Text.Encodings.Web;

namespace EquipmentBorrowingManagementSystem.Web.Helpers;

public static class DisplayHelper
{
    public static string Escape(string? text) => WebUtility.HtmlEncode(text ?? string.Empty);

    public static string FormatDate(DateTime? value) =>
        value.HasValue ? value.Value.ToLocalTime().ToString("dd/MM/yyyy") : "—";

    public static string EquipmentStatusClass(string status) => status switch
    {
        "Available" => "status-available",
        "Borrowed" => "status-borrowed",
        "Maintenance" => "status-maintenance",
        "Retired" => "status-retired",
        "Reserved" => "status-reserved",
        "Lost" => "status-lost",
        "Compensated" => "status-compensated",
        _ => "status-default"
    };

    public static string BorrowStatusClass(string status) => status switch
    {
        "Pending" => "borrow-pending",
        "Approved" => "borrow-approved",
        "Rejected" => "borrow-rejected",
        "Cancelled" => "borrow-cancelled",
        "InProgress" => "borrow-progress",
        "Completed" => "borrow-completed",
        "Overdue" => "borrow-overdue",
        _ => "borrow-default"
    };

    public static string StatusBadge(string status, string type)
    {
        var css = type == "borrow" ? BorrowStatusClass(status) : EquipmentStatusClass(status);
        return $"<span class=\"status-badge {css}\">{Escape(status)}</span>";
    }

    public static bool IsBorrowable(string? status) => status == "Available";

    public static string EquipmentImageUrl(string? imageUrl) =>
        string.IsNullOrWhiteSpace(imageUrl) ? "/images/equipment-default.svg" : imageUrl.Trim();
}
