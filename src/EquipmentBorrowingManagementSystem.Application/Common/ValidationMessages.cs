namespace EquipmentBorrowingManagementSystem.Application.Common;

/// <summary>Thông báo validation / nghiệp vụ hiển thị cho client (tiếng Việt).</summary>
public static class ValidationMessages
{
    public const string Required = "Trường này là bắt buộc.";
    public const string InvalidEmail = "Email không hợp lệ.";
    public const string PasswordLength = "Mật khẩu phải từ 6 đến 100 ký tự.";
    public const string InvalidData = "Dữ liệu không hợp lệ.";

    public const string BorrowDateRequired = "Ngày mượn là bắt buộc.";
    public const string ReturnDateRequired = "Ngày trả dự kiến là bắt buộc.";
    public const string ReturnAfterBorrow = "Ngày trả dự kiến phải sau hoặc bằng ngày mượn.";
    public const string PurposeRequired = "Mục đích mượn là bắt buộc.";
    public const string PurposeMaxLength = "Mục đích mượn tối đa 500 ký tự.";
    public const string AtLeastOneEquipment = "Phải chọn ít nhất một thiết bị.";
    public const string DuplicateEquipmentInRequest = "Không được chọn trùng thiết bị trong cùng một yêu cầu.";
    public const string EquipmentIdInvalid = "Mã thiết bị không hợp lệ.";
    public const string QuantityInvalid = "Số lượng phải lớn hơn 0.";

    public const string BorrowStatusInvalid =
        "Trạng thái phải là một trong: Approved, Rejected, Cancelled, InProgress, Completed.";
    public const string RejectReasonRequired = "Lý do từ chối là bắt buộc.";
    public const string RejectReasonMaxLength = "Lý do từ chối tối đa 500 ký tự.";
    public const string HandoverItemsRequired = "Phải gửi danh sách thiết bị khi bàn giao.";
    public const string ReturnItemsRequired = "Phải gửi danh sách thiết bị khi ghi nhận trả.";
    public const string NoteMaxLength = "Ghi chú tối đa 500 ký tự.";
    public const string StaffNoteMaxLength = "Ghi chú nhân viên tối đa 500 ký tự.";

    public const string EquipmentStatusInvalid =
        "Trạng thái phải là một trong: Available, Maintenance, Retired, Compensated.";

    public const string RefreshTokenRequired = "Refresh token là bắt buộc.";
}
