# PROJECT STATE — Equipment Borrowing Management System (P1 / PRN232)

## 1. Model hiện tại (đã chốt)

- Workflow **status-only** (không còn Condition).
- `EquipmentStatus`: Available, Borrowed, Maintenance, Retired, Reserved, **Damaged**  
  (đã bỏ Lost / Compensated).
- `BorrowRequestStatus`: Pending, Approved, Rejected, Cancelled, InProgress, Completed, Overdue.
- Không còn `Quantity` trên `BorrowRequestItem`.
- Có `BorrowRequest.ActualReturnDate` và `BorrowRequestItem.ReturnStatus` (snapshot lúc trả).
- Phân quyền:
  - **Chỉ User** tạo yêu cầu mượn
  - **Chỉ Staff** duyệt / từ chối / bàn giao / nhận trả
  - **Admin** quản lý thiết bị + Users; **không** trang Duyệt mượn; **không** mượn
- Auto-transition theo ngày (**chỉ khi Staff truy cập** list/detail borrow — không có job nền):
  - Pending quá `BorrowDate` → Rejected
  - Approved quá `BorrowDate` → Cancelled (+ release Reserved)
  - InProgress quá `ExpectedReturnDate` → Overdue
- User đang Overdue không được tạo đơn mới (API 400 + toast UI).

## 2. Migration / Snapshot

| Migration | Nội dung |
|-----------|----------|
| `RemoveConditionWorkflow` | Drop cột Condition legacy |
| `RemoveBorrowRequestReturnedStatus` | Bỏ status Returned |
| `RemoveAuditLog` | Xóa bảng AuditLogs |
| **`RemoveQuantityAddReturnTracking`** | Drop `Quantity`; thêm `ActualReturnDate`, `ReturnStatus` |

Snapshot `AppDbContextModelSnapshot.cs` đã đồng bộ.

## 3. Documentation / ERD

- `docs/PROJECT_DOCUMENTATION.md` — **đủ 14 mục nộp tài liệu** theo yêu cầu PRN232 §13:
  giới thiệu, role, use case, ERD, business rules, workflow, kiến trúc, service design,
  API list, security matrix, OData demo, content negotiation, gRPC, hướng dẫn chạy.
- `docs/ERD.dbml` — schema hiện tại (không Quantity, có ActualReturnDate/ReturnStatus, không AuditLogs).
- `docs/API_PAGE_MAPPING.md` — map trang Razor ↔ API + ma trận role.
- `docs/MANUAL_TEST_CHECKLIST.md` — checklist test thủ công.
- Postman collection: chưa có file riêng; dùng Swagger + checklist để demo OData/API.
- OData và gRPC **không** còn trang demo trên Web UI (Staff/Admin nav đã gỡ).

## 4. Web UI

- Razor Pages (`EquipmentBorrowingManagementSystem.Web`), **không** còn phụ thuộc `client/*.html` cho flow chính.
- Toast toàn cục; chuông hiện badge unread.
- `/Manage`: filter đủ status; hoàn tất BT chọn Available/Retired; return chọn status từng thiết bị.
- Đã gỡ `/ODataExplorer`, `/GrpcTools` và client gRPC/OData demo phía Web.

## 5. gRPC NotificationService

- Project: `src/EquipmentBorrowingManagementSystem.Grpc`
- Trigger: approve / reject / handover / return / auto-reject / auto-cancel / overdue
- In-app `Notifications` + gRPC non-blocking (side-effect; lỗi gRPC không rollback noti DB)


## 6. Seed

- `DbInitializer` truncate toàn bộ bảng nghiệp vụ rồi seed lại mỗi lần chạy API.
- Tài khoản: `admin@ebms.local`, `staff@ebms.local`, `user@ebms.local` (password tương ứng `*@123`).
- Seed có sẵn đơn Pending / Approved / Completed / Rejected / Overdue với ngày relative `UtcNow`.
