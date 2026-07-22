namespace EquipmentBorrowingManagementSystem.Web.Options;

public class ApiOptions
{
    public const string SectionName = "ApiBaseUrl";

    public string BaseUrl { get; set; } = "http://localhost:5171";
}
