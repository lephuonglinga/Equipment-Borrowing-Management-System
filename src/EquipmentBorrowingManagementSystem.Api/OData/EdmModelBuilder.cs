using EquipmentBorrowingManagementSystem.Domain.Entities;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace EquipmentBorrowingManagementSystem.Api.OData;

public static class EdmModelBuilder
{
    public static IEdmModel GetEdmModel()
    {
        var builder = new ODataConventionModelBuilder();

        // Catalog: categories with nested equipments (REST only has flat category list).
        builder.EntitySet<EquipmentCategory>("EquipmentCategories");

        // Return history — no dedicated REST list endpoint.
        builder.EntitySet<ReturnRecord>("ReturnRecords");

        // Line-item view across borrow requests — items only via borrow-request detail on REST.
        builder.EntitySet<BorrowRequestItem>("BorrowRequestItems");

        return builder.GetEdmModel();
    }
}
