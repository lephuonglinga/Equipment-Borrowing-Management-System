using EquipmentBorrowingManagementSystem.Application.DTOs.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

namespace EquipmentBorrowingManagementSystem.Api.OData;

public static class ODataEdmModelBuilder
{
    public static IEdmModel GetEdmModel()
    {
        var modelBuilder = new ODataConventionModelBuilder();

        modelBuilder.EntitySet<EquipmentODataDto>("Equipment");
        modelBuilder.EntitySet<BorrowRequestODataDto>("BorrowRequests");

        return modelBuilder.GetEdmModel();
    }
}
