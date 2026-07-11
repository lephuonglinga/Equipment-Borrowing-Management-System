using EquipmentBorrowingManagementSystem.Web.Infrastructure;
using EquipmentBorrowingManagementSystem.Web.Models;

namespace EquipmentBorrowingManagementSystem.Web.Services;

public class BorrowCartService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BorrowCartService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ISession Session => _httpContextAccessor.HttpContext!.Session;

    public IReadOnlyList<BorrowCartItem> GetItems() => Session.GetBorrowCart();

    public int Count => GetItems().Count;

    public bool Contains(int equipmentId) => GetItems().Any(i => i.Id == equipmentId);

    public void Add(EquipmentDto equipment)
    {
        var cart = Session.GetBorrowCart();
        if (cart.Any(i => i.Id == equipment.Id))
        {
            return;
        }

        cart.Add(new BorrowCartItem
        {
            Id = equipment.Id,
            Name = equipment.Name,
            SerialNumber = equipment.SerialNumber,
            CategoryName = equipment.CategoryName
        });
        Session.SetBorrowCart(cart);
    }

    public void Remove(int equipmentId)
    {
        var cart = Session.GetBorrowCart().Where(i => i.Id != equipmentId).ToList();
        Session.SetBorrowCart(cart);
    }

    public void Clear() => Session.ClearBorrowCart();

    public List<CreateBorrowRequestItemDto> ToApiItems() =>
        GetItems().Select(i => new CreateBorrowRequestItemDto { EquipmentId = i.Id }).ToList();
}
