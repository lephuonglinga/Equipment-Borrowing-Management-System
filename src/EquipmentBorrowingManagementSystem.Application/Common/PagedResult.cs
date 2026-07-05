using System.Xml.Serialization;

namespace EquipmentBorrowingManagementSystem.Application.Common;

[XmlRoot("PagedResult")]
public class PagedResult<T>
{
    [XmlElement("Item")]
    public List<T> Items { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;
}
