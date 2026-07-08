namespace EquipmentBorrowingManagementSystem.Application.DTOs.Reports;

public class EquipmentStatusCountDto
{
    public int Total { get; set; }
    public int Available { get; set; }
    public int Borrowed { get; set; }
    public int Maintenance { get; set; }
    public int Retired { get; set; }
}

public class MostBorrowedEquipmentDto
{
    public int EquipmentId { get; set; }
    public string EquipmentName { get; set; } = string.Empty;
    public string SerialNumber { get; set; } = string.Empty;
    public int BorrowCount { get; set; }
}

public class DashboardStatsDto
{
    public EquipmentStatusCountDto EquipmentByStatus { get; set; } = new();
    public List<StatusCountDto> BorrowRequestsByStatus { get; set; } = [];
    public int OverdueRequestCount { get; set; }
    /// <summary>Thiết bị đang <c>Status = Lost</c> (chờ xác nhận đền bù).</summary>
    public int LostEquipmentCount { get; set; }
    /// <summary>Thiết bị đang <c>Status = Compensated</c> (đã đền bù).</summary>
    public int CompensatedEquipmentCount { get; set; }
    public int MaintenanceEquipmentCount { get; set; }
    public List<MostBorrowedEquipmentDto> MostBorrowedEquipment { get; set; } = [];
}
