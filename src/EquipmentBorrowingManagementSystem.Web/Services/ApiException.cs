namespace EquipmentBorrowingManagementSystem.Web.Services;

public class ApiException : Exception
{
    public int StatusCode { get; }
    public string? ApiMessage { get; }

    public ApiException(int statusCode, string? apiMessage)
        : base(apiMessage ?? $"API request failed with status {statusCode}")
    {
        StatusCode = statusCode;
        ApiMessage = apiMessage;
    }
}
