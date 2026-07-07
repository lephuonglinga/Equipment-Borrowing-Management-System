using EquipmentBorrowingManagementSystem.Application.Common;
using EquipmentBorrowingManagementSystem.Application.DTOs.Reports;
using EquipmentBorrowingManagementSystem.Application.Interfaces;
using EquipmentBorrowingManagementSystem.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;

namespace EquipmentBorrowingManagementSystem.Application.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReportService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BorrowSummaryDto>> GetBorrowSummaryAsync(BorrowSummaryQueryParams query)
    {
        if (query.FromDate.HasValue && query.ToDate.HasValue && query.FromDate > query.ToDate)
        {
            return Result<BorrowSummaryDto>.Fail(
                "Ngày bắt đầu phải trước hoặc bằng ngày kết thúc.",
                StatusCodes.Status400BadRequest);
        }

        var summary = await _unitOfWork.Reports.GetBorrowSummaryAsync(query.FromDate, query.ToDate);
        return Result<BorrowSummaryDto>.Ok(summary);
    }

    public async Task<Result<List<OverdueRequestDto>>> GetOverdueRequestsAsync()
    {
        var overdue = await _unitOfWork.Reports.GetOverdueRequestsAsync();
        return Result<List<OverdueRequestDto>>.Ok(overdue);
    }

    public async Task<Result<DashboardStatsDto>> GetDashboardStatsAsync()
    {
        var stats = await _unitOfWork.Reports.GetDashboardStatsAsync();
        return Result<DashboardStatsDto>.Ok(stats);
    }
}
