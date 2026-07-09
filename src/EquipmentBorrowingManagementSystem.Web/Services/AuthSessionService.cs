using EquipmentBorrowingManagementSystem.Web.Infrastructure;
using EquipmentBorrowingManagementSystem.Web.Models;

namespace EquipmentBorrowingManagementSystem.Web.Services;

public class AuthSessionService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthSessionService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ISession Session => _httpContextAccessor.HttpContext!.Session;

    public AuthSession? GetAuth() => Session.GetAuthSession();

    public void SaveAuth(AuthSession auth) => Session.SetAuthSession(auth);

    public void ClearAuth()
    {
        Session.ClearAuthSession();
        Session.ClearBorrowCart();
    }

    public AuthSession FromAuthResponse(AuthSession response) => response;
}
