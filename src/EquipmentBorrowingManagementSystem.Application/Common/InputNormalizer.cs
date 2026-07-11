namespace EquipmentBorrowingManagementSystem.Application.Common;

public static class InputNormalizer
{
    public static string? TrimToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static string TrimRequired(string? value) =>
        value?.Trim() ?? string.Empty;

    public static bool HasContent(string? value) => !string.IsNullOrWhiteSpace(value);

    /// <summary>
    /// Trims the value. Returns an error message when empty/whitespace-only; otherwise null.
    /// </summary>
    public static string? Require(string? value, out string trimmed, string errorMessage)
    {
        trimmed = TrimRequired(value);
        return HasContent(trimmed) ? null : errorMessage;
    }
}
