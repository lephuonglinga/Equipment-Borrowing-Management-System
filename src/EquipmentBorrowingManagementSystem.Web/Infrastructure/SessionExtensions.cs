using System.Text.Json;
using EquipmentBorrowingManagementSystem.Web.Models;

namespace EquipmentBorrowingManagementSystem.Web.Infrastructure;

public static class SessionKeys
{
    public const string Auth = "ebms_auth";
    public const string BorrowCart = "ebms_borrow_cart";
}

public static class SessionExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static AuthSession? GetAuthSession(this ISession session)
    {
        var raw = session.GetString(SessionKeys.Auth);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        return JsonSerializer.Deserialize<AuthSession>(raw, JsonOptions);
    }

    public static void SetAuthSession(this ISession session, AuthSession auth)
    {
        session.SetString(SessionKeys.Auth, JsonSerializer.Serialize(auth, JsonOptions));
    }

    public static void ClearAuthSession(this ISession session)
    {
        session.Remove(SessionKeys.Auth);
    }

    public static List<BorrowCartItem> GetBorrowCart(this ISession session)
    {
        var raw = session.GetString(SessionKeys.BorrowCart);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<BorrowCartItem>>(raw, JsonOptions) ?? [];
    }

    public static void SetBorrowCart(this ISession session, List<BorrowCartItem> cart)
    {
        session.SetString(SessionKeys.BorrowCart, JsonSerializer.Serialize(cart, JsonOptions));
    }

    public static void ClearBorrowCart(this ISession session)
    {
        session.Remove(SessionKeys.BorrowCart);
    }
}
