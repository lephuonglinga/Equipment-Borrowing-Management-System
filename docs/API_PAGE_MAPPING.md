# API ↔ Page Mapping

Web app runs at `http://localhost:5172`. API at `http://localhost:5171`. gRPC notification service at `http://localhost:5272`.

All REST/OData calls are made **server-side** from Razor Page models via `EbmsApiClient` (session bearer token). gRPC is invoked **server-side** from `GrpcNotificationService`.

## Role & function matrix

| Chức năng | User | Staff | Admin |
|-----------|:----:|:-----:|:-----:|
| Xem danh mục / thiết bị, đăng ký mượn | ✓ | ✓ | ✓ |
| Theo dõi yêu cầu mượn của bản thân | ✓ | ✓ | ✓ |
| **Duyệt / từ chối / bàn giao / nhận trả** yêu cầu mượn | | **✓** | ✓ |
| **Quản lý thiết bị & danh mục** (`/Manage`) | | **✓** | ✓ |
| Báo cáo, OData Explorer, gRPC tools | | ✓ | ✓ |
| Quản lý Users (tạo Staff, kích hoạt/vô hiệu) | | | ✓ |

Ghi chú:

- **Staff** là vai trò vận hành chính: duyệt mượn và CRUD thiết bị/danh mục.
- **Admin** có toàn bộ quyền Staff, cộng thêm quản lý tài khoản.
- **User** chỉ mượn thiết bị và theo dõi đơn của mình.

## Page route ↔ API

| Page route | Feature | API endpoints used |
|------------|---------|-------------------|
| `/Account/Login` | Authentication | `POST /api/auth/login` |
| `/Account/Register` | Registration | `POST /api/auth/register` |
| `/Account/Logout` (POST) | Logout | `POST /api/auth/logout` |
| `/Categories` | Category listing | `GET /api/equipment-categories` |
| `/Equipment` | Equipment catalog, filters, borrow cart | `GET /api/equipment-categories`, `GET /api/equipment`, `GET /api/equipment/{id}`, `POST /api/borrow-requests` |
| `/Equipment/{id}` | Equipment detail, cart toggle | `GET /api/equipment/{id}` |
| `/Borrow` | Borrow requests — User: theo dõi; **Staff**/Admin: duyệt/bàn giao/trả | `GET /api/borrow-requests`, `GET /api/borrow-requests/{id}`, `PATCH /api/borrow-requests/{id}` |
| `/Manage` | Equipment & category CRUD (**Staff**/Admin) | `GET/POST/PUT/DELETE /api/equipment`, `GET/POST/PUT/DELETE /api/equipment-categories` |
| `/Reports` | Dashboard & summaries (Staff/Admin) | `GET /api/reports/dashboard`, `GET /api/reports/overdue-requests`, `GET /api/reports/borrow-summary` |
| `/Users` | User list & create Staff (Admin) | `GET /api/users`, `POST /api/users`, `PATCH /api/users/{id}` |
| `/Users/{id}` | User detail & activate/deactivate (Admin) | `GET /api/users/{id}`, `PATCH /api/users/{id}` |
| `/Notifications` | Notification inbox (list + mark read) | `GET /api/notifications`, `PATCH /api/notifications/{id}/read` |
| `/ODataExplorer` | OData query tester (Staff/Admin) | `GET /odata/Equipment`, `GET /odata/BorrowRequests` |
| `/GrpcTools` | gRPC notification sender (Staff/Admin) | gRPC `EmailNotificationService.Send` |

## Session / auth support (all authenticated pages)

| Mechanism | Endpoint |
|-----------|----------|
| Token refresh (automatic on 401 / expired access token) | `POST /api/auth/refresh` |

## Not exposed in Web UI

These API/OData endpoints exist on the server but are not given dedicated pages (by design):

- `GET /api/equipment-categories/{id}` — used indirectly via list/detail flows
- `GET /api/audit-logs` — Audit page removed per requirements
- OData entity sets beyond Equipment/BorrowRequests (none configured in EDM)

## Notes

- **Audit page removed** — no web route calls audit APIs.
- **CORS** — not required for browser→API because the Web app proxies all REST/OData via server-side `HttpClient`.
- **Ports** — Web `5172`, API `5171`, gRPC `5272` (see `appsettings.json` and `Properties/launchSettings.json`).
