using EquipmentBorrowingManagementSystem.Domain.Enums;

namespace EquipmentBorrowingManagementSystem.Application.Common;

public static class EquipmentRules
{
    public static readonly EquipmentStatus[] StaffSettableStatuses =
    [
        EquipmentStatus.Available,
        EquipmentStatus.Maintenance,
        EquipmentStatus.Retired,
        EquipmentStatus.Damaged
    ];

    public static readonly EquipmentStatus[] ReturnStatuses =
    [
        EquipmentStatus.Available,
        EquipmentStatus.Damaged,
        EquipmentStatus.Maintenance,
        EquipmentStatus.Retired
    ];

    public static readonly EquipmentStatus[] MaintenanceCompleteStatuses =
    [
        EquipmentStatus.Available,
        EquipmentStatus.Retired
    ];

    public static bool IsBrowsable(EquipmentStatus status) => true;

    public static bool CanDelete(EquipmentStatus status) =>
        status is EquipmentStatus.Available
            or EquipmentStatus.Maintenance
            or EquipmentStatus.Retired
            or EquipmentStatus.Damaged;

    public static bool IsBorrowable(EquipmentStatus status) =>
        status == EquipmentStatus.Available;

    public static bool CanCompleteMaintenance(EquipmentStatus status) =>
        status == EquipmentStatus.Maintenance;

    public static bool IsFlowLocked(EquipmentStatus status) =>
        status is EquipmentStatus.Borrowed or EquipmentStatus.Reserved;

    public static bool IsStaffSettable(EquipmentStatus status) =>
        StaffSettableStatuses.Contains(status);

    public static bool IsValidReturnStatus(EquipmentStatus status) =>
        ReturnStatuses.Contains(status);
}
