namespace EquipmentBorrowingManagementSystem.Web.Infrastructure;

public static class FormValidation
{
    public static string? RequireText(string? value, string fieldLabel) =>
        string.IsNullOrWhiteSpace(value) ? $"{fieldLabel} là bắt buộc." : null;

    public static string? RequireEmail(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "Email là bắt buộc." : null;

    public static bool CanEditEquipment(string status) =>
        status is "Available" or "Maintenance" or "Retired" or "Damaged";

    public static bool CanDeleteEquipment(string status) =>
        status is "Available" or "Maintenance" or "Retired" or "Damaged";

    public static readonly string[] StaffSettableStatuses =
        ["Available", "Maintenance", "Retired", "Damaged"];

    public static readonly string[] ReturnStatuses =
        ["Available", "Damaged", "Maintenance", "Retired"];

    public static readonly string[] MaintenanceCompleteStatuses =
        ["Available", "Retired"];
}
