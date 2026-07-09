using EquipmentBorrowingManagementSystem.Web.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace EquipmentBorrowingManagementSystem.Web.Pages.Notifications;

public class IndexModel : EbmsPageModel
{
    public IActionResult OnGet()
    {
        if (EnsureAuthenticated() is IActionResult redirect)
        {
            return redirect;
        }

        return Page();
    }
}
