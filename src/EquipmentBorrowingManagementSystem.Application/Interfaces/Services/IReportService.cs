using EquipmentBorrowingManagementSystem.Application.Common;
using EquipmentBorrowingManagementSystem.Application.DTOs.Reports;

namespace EquipmentBorrowingManagementSystem.Application.Interfaces.Services;

public interface IReportService
{
    Task<Result<BorrowSummaryDto>> GetBorrowSummaryAsync(BorrowSummaryQueryParams query);
    Task<Result<List<OverdueRequestDto>>> GetOverdueRequestsAsync();
    Task<Result<DashboardStatsDto>> GetDashboardStatsAsync();
}
