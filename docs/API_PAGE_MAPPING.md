# API ↔ Page Mapping

Web app: `http://localhost:5172`  
API: `http://localhost:5171`  
gRPC notification: `http://localhost:5272`

Mọi REST/OData gọi **server-side** từ Razor Page models qua `EbmsApiClient` (bearer trong session). gRPC gọi từ `NotificationService` / `GrpcNotificationService`.

## Role & function matrix

| Chức năng | User | Staff | Admin |
|-----------|:----:|:-----:|:-----:|
| Xem danh mục / thiết bị | ✓ | ✓ | ✓ |
| **Đăng ký mượn** (giỏ + tạo đơn) | **✓** | | |
| Theo dõi / hủy yêu cầu mượn của mình | ✓ | | |
| **Duyệt / từ chối / bàn giao / nhận trả** | | **✓** | |
| Vào trang `/Borrow` (Duyệt mượn / Yêu cầu mượn) | ✓ | ✓ | **✗** |
| **Quản lý thiết bị & danh mục** (`/Manage`) | | ✓ | ✓ |
| Báo cáo, OData Explorer, gRPC tools | | ✓ | ✓ |
| Quản lý Users (tạo Staff, kích hoạt/vô hiệu) | | | ✓ |
| Xem thông báo + badge chưa đọc | ✓ | ✓ | ✓ |

Ghi chú:

- **Staff** = vận hành mượn-trả + CRUD thiết bị.
- **Admin** = CRUD thiết bị + Users; **không** duyệt mượn và **không** tự mượn.
- **User** = chỉ mượn và theo dõi đơn của mình.

## Page route ↔ API

| Page route | Feature | API endpoints used |
|------------|---------|-------------------|
| `/Account/Login` | Authentication | `POST /api/auth/login` |
| `/Account/Register` | Registration | `POST /api/auth/register` |
| `/Account/Logout` (POST) | Logout | `POST /api/auth/logout` |
| `/Categories` | Category listing | `GET /api/equipment-categories` |
| `/Equipment` | Catalog, filters, borrow cart (**User** only) | `GET /api/equipment-categories`, `GET /api/equipment`, `GET /api/equipment/{id}`, `GET /api/borrow-requests` (check Overdue), `POST /api/borrow-requests` |
| `/Equipment/{id}` | Detail, cart toggle (**User** only) | `GET /api/equipment/{id}`, `GET /api/borrow-requests` |
| `/Borrow` | User: theo dõi/hủy; **Staff**: duyệt/bàn giao/trả | `GET /api/borrow-requests`, `GET /api/borrow-requests/{id}`, `PATCH /api/borrow-requests/{id}` |
| `/Manage` | Equipment & category CRUD (Staff/Admin) | `GET/POST/PUT/DELETE /api/equipment`, `GET/POST/PUT/DELETE /api/equipment-categories` |
| `/Reports` | Dashboard & summaries (Staff/Admin) | `GET /api/reports/dashboard`, `GET /api/reports/overdue-requests`, `GET /api/reports/borrow-summary` |
| `/Users` | User list & create Staff (Admin) | `GET /api/users`, `POST /api/users`, `PATCH /api/users/{id}` |
| `/Users/{id}` | User detail & activate/deactivate (Admin) | `GET /api/users/{id}`, `PATCH /api/users/{id}` |
| `/Notifications` | Inbox + mark read | `GET /api/notifications`, `PATCH /api/notifications/{id}/read` |
| `/ODataExplorer` | OData tester (Staff/Admin) | `GET /odata/Equipment`, `GET /odata/BorrowRequests` |
| `/GrpcTools` | gRPC sender (Staff/Admin) | gRPC `EmailNotificationService.Send` |

## Session / auth support

| Mechanism | Endpoint |
|-----------|----------|
| Token refresh (automatic on 401 / expired access token) | `POST /api/auth/refresh` |

## Not exposed in Web UI

- `GET /api/equipment-categories/{id}` — dùng gián tiếp qua list
- Audit APIs — đã xóa (không còn bảng AuditLogs)
- OData entity sets khác Equipment / BorrowRequests — không cấu hình trong EDM

## Notes

- **CORS** không cần cho browser→API vì Web proxy qua `HttpClient` server-side.
- Ports: Web `5172`, API `5171`, gRPC `5272`.
- Chi tiết statechart / ERD: xem `PROJECT_DOCUMENTATION.md` và `ERD.dbml`.
