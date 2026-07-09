using EquipmentBorrowingManagementSystem.Web.Infrastructure;
using EquipmentBorrowingManagementSystem.Web.Models;
using EquipmentBorrowingManagementSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBorrowingManagementSystem.Web.Pages.ODataExplorer;

public class IndexModel : EbmsPageModel
{
    private readonly EbmsApiClient _api;

    public IndexModel(EbmsApiClient api)
    {
        _api = api;
    }

    [BindProperty]
    public string EntitySet { get; set; } = "Equipment";

    [BindProperty]
    public string Query { get; set; } = "?$top=5";

    public ODataQueryResult? Result { get; set; }

    public IActionResult OnGet()
    {
        if (EnsureStaffOrAdmin() is IActionResult redirect)
        {
            return redirect;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (EnsureStaffOrAdmin() is IActionResult redirect)
        {
            return redirect;
        }

        var path = $"odata/{EntitySet}{NormalizeQuery(Query)}";
        Result = await _api.QueryODataAsync(path, cancellationToken: cancellationToken);
        return Page();
    }

    private static string NormalizeQuery(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return string.Empty;
        }

        return query.StartsWith('?') ? query : "?" + query;
    }
}
