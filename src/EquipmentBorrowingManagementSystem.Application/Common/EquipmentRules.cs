using EquipmentBorrowingManagementSystem.Domain.Enums;

namespace EquipmentBorrowingManagementSystem.Application.Common;

public static class EquipmentRules
{
    public static bool IsBrowsable(EquipmentStatus status) => true;

    public static bool IsEditable(EquipmentStatus status) =>
        status is not (EquipmentStatus.Compensated or EquipmentStatus.Lost);

    public static bool IsBorrowable(EquipmentStatus status) =>
        status == EquipmentStatus.Available;

    public static bool CanCompleteMaintenance(EquipmentStatus status) =>
        status == EquipmentStatus.Maintenance;

    public static bool CanConfirmCompensation(EquipmentStatus status) =>
        status == EquipmentStatus.Lost;

    // Condition logic removed (status-only workflow).
}
