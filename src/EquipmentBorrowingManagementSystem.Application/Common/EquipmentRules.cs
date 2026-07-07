using EquipmentBorrowingManagementSystem.Domain.Enums;

namespace EquipmentBorrowingManagementSystem.Application.Common;

public static class EquipmentRules
{
    public static bool IsBrowsable(EquipmentStatus status) => status != EquipmentStatus.Compensated;

    public static bool IsEditable(EquipmentStatus status) =>
        status is not (EquipmentStatus.Compensated or EquipmentStatus.Lost);

    public static bool IsBorrowable(EquipmentStatus status, EquipmentCondition condition) =>
        status == EquipmentStatus.Available &&
        condition is EquipmentCondition.Good or EquipmentCondition.Fair;

    public static EquipmentStatus MapReturnConditionToStatus(EquipmentCondition condition) =>
        condition switch
        {
            EquipmentCondition.Good or EquipmentCondition.Fair => EquipmentStatus.Available,
            EquipmentCondition.Damaged => EquipmentStatus.Maintenance,
            EquipmentCondition.Lost => EquipmentStatus.Lost,
            _ => EquipmentStatus.Available
        };

    public static bool CanCompleteMaintenance(EquipmentStatus status) =>
        status == EquipmentStatus.Maintenance;

    public static bool CanConfirmCompensation(EquipmentStatus status) =>
        status == EquipmentStatus.Lost;

    public static bool IsHandoverCondition(EquipmentCondition condition) =>
        condition is EquipmentCondition.Good or EquipmentCondition.Fair or EquipmentCondition.Damaged;

    public static bool IsReturnCondition(EquipmentCondition condition) =>
        condition is EquipmentCondition.Good
            or EquipmentCondition.Fair
            or EquipmentCondition.Damaged
            or EquipmentCondition.Lost;

    public static bool IsPostMaintenanceCondition(EquipmentCondition condition) =>
        condition is EquipmentCondition.Good or EquipmentCondition.Fair;
}
