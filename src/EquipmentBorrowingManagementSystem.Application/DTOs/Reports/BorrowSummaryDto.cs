namespace EquipmentBorrowingManagementSystem.Application.DTOs.Reports;

public class BorrowSummaryDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int TotalRequests { get; set; }
    public List<StatusCountDto> RequestsByStatus { get; set; } = [];
    public int CompletedRequests { get; set; }
    public int ActiveRequests { get; set; }
    public int RejectedRequests { get; set; }
    public int CancelledRequests { get; set; }
}
