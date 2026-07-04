using EquipmentBorrowingManagementSystem.Application.DTOs.Reports;

namespace EquipmentBorrowingManagementSystem.Application.Interfaces.Repositories;

public interface IReportRepository
{
    Task<BorrowSummaryDto> GetBorrowSummaryAsync(DateTime? fromDate, DateTime? toDate);
    Task<List<OverdueRequestDto>> GetOverdueRequestsAsync();
    Task<DashboardStatsDto> GetDashboardStatsAsync();
}
