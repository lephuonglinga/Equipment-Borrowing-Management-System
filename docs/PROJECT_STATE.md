# PROJECT STATE - Equipment Borrowing Management System (P1 / PRN232)

> File tong hop trang thai du an de tiep tuc o session Cursor moi. Doc file nay + file plan + PROJECT_DOCUMENTATION truoc khi lam tiep.

## 0. Muc tieu

- Hoan thien TOAN BO yeu cau trong file de bai PRN232 (P1 - Equipment Borrowing Management System) + TAT CA tinh nang khuyen khich o Section 15.
- Tieu chi: dung va du theo de bai, KHONG lam thua, KHONG phuc tap hoa. Dieu gi mo ho thi hoi lai user truoc khi lam.
- Bat chuoc cach code va cau truc cay thu muc cua 2 du an tham khao (xem muc 6).

## 1. Cac quyet dinh da chot (confirmed)

- Client: HTML + CSS + vanilla JavaScript (fetch), bat CORS.
- Inter-service: gRPC `NotificationService` (project rieng, API goi khi approve/reject/return; goi non-blocking, loi thi van thanh cong + log).
- Bonus: lam TAT CA Section 15 (refresh token, audit log, soft delete, pagination chuan hoa, global exception handling, Result wrapper, AutoMapper, Repository+UnitOfWork, FluentValidation, simulated notifications, dashboard stats, Docker Compose, Serilog).
- Nghiep vu muon/tra (Plan A — da implement):
  - Duyet -> thiet bi `Reserved` (chua `Borrowed`).
  - Ban giao (`PATCH status=InProgress`) -> ghi `ConditionAtBorrow` + `HandoverNote`; thiet bi `Borrowed`.
  - Tra (`PATCH status=Completed`) -> Good/Fair->`Available`, Damaged->`Maintenance`, Lost->`Lost`; `ReturnRecord.OverallCondition` = xau nhat.
  - Bao tri xong: Staff `PUT` equipment `Maintenance -> Available` (condition Good/Fair).
  - Mat: Staff `PUT` `Lost -> Compensated` (an vinh vien).
  - Tu huy don `Approved` neu qua `BorrowDate` chua ban giao (`BorrowRequestExpirationHostedService`).
- `Equipment.CurrentCondition` tach khoi `Equipment.Status` (Good/Fair/Damaged/Lost/Compensated).
- KHONG co phi muon (muon mien phi). Mat/hong xu ly qua trang thai thiet bi, khong co phi den bu.
- User management: Admin `PATCH /api/users/{id}` voi `{ isActive }`, **KHONG xoa** user.
- API REST: thay doi trang thai qua `PATCH`/`PUT` tren resource, **KHONG** dung URL hanh dong (`/approve`, `/activate`, ...).

## 2. Kien truc & cau truc solution

4 project (giu nguyen, KHONG gop lai); se them 2 thanh phan moi (gRPC project + thu muc `client/`).

```
src/
  EquipmentBorrowingManagementSystem.Domain         (Entities/, Enums/)  -- DONE
  EquipmentBorrowingManagementSystem.Application     (Common/, Interfaces/, DTOs/, Mappings/, Services/, DependencyInjection.cs)
  EquipmentBorrowingManagementSystem.Infrastructure  (Data/, Seeders/, Repositories/, UnitOfWork/, DependencyInjection.cs)
  EquipmentBorrowingManagementSystem.Api             (Controllers/, Middleware/, Program.cs)
```

Phu thuoc: Application -> Domain ; Infrastructure -> Application, Domain ; Api -> Application, Infrastructure.

## 3. Trang thai HIEN TAI (da lam)

### 3a. Tai cau truc Infrastructure (DONE)
- Da doi `Infrastructure/Persistence/` -> `Infrastructure/Data/` (AppDbContext + Configurations/ + Migrations/), namespace `...Infrastructure.Data(.Configurations/.Migrations)`.
- Da doi `DbInitializer.cs` -> `Infrastructure/Seeders/DbInitializer.cs`, namespace `...Infrastructure.Seeders`.
- Da sua using o `Program.cs` va `DependencyInjection.cs`.

### 3b. Slice 1 - Walking skeleton (DONE, da test chay that)
Files da tao:
- Application: `Common/Result.cs`, `Interfaces/Repositories/IGenericRepository.cs`, `Interfaces/Repositories/IEquipmentRepository.cs`, `Interfaces/IUnitOfWork.cs`, `Interfaces/Services/IEquipmentService.cs`, `DTOs/EquipmentDto.cs`, `Mappings/MappingProfile.cs`, `Services/EquipmentService.cs`, `DependencyInjection.cs`.
- Infrastructure: `Repositories/GenericRepository.cs`, `Repositories/EquipmentRepository.cs`, `UnitOfWork/UnitOfWork.cs`; cap nhat `DependencyInjection.cs` (dang ky IGenericRepository<>, IEquipmentRepository, IUnitOfWork).
- Api: `Controllers/ApiControllerBase.cs`, `Controllers/EquipmentController.cs`, `Middleware/ExceptionHandlingMiddleware.cs`; rewrite `Program.cs` (Serilog + AddApplication + AddInfrastructure + migrate + seed + exception middleware + Swagger).

Ket qua test (LocalDB that):
- `GET /api/equipment` -> 200, tra 12 thiet bi seed (co categoryName, status dang chuoi).
- `GET /api/equipment/{id}` -> 200; id khong ton tai -> 404.
- `dotnet build` toan solution: 0 warning, 0 error.

Bonus da gan o Slice 1: Result wrapper, Repository+UnitOfWork, AutoMapper, global exception handling, Serilog.

### 3c. Slice 2 - Authentication (DONE, da test chay that)
Co che: JWT access token (HS256, song ngan, mac dinh 60 phut) + refresh token (chuoi random 64 byte base64, luu DB, mac dinh 7 ngay) + 3 role + `[Authorize]`. Refresh co ROTATION (refresh cu bi thu hoi khi cap moi duoc phat). Logout = thu hoi refresh token.

Files da tao/sua:
- Domain: `Entities/RefreshToken.cs` (UserId, Token, ExpiresAt, RevokedAt, nav User); them nav `RefreshTokens` vao `User`.
- Application: `Constants/Roles.cs` (Admin/Staff/User); DTOs `RegisterDto`/`LoginDto`/`RefreshRequestDto`/`AuthResponseDto`; `Interfaces/Security/IJwtTokenGenerator.cs`,`ICurrentUser.cs`,`IPasswordHasher.cs`; `Interfaces/Repositories/IUserRepository.cs`,`IRefreshTokenRepository.cs`; `Interfaces/Services/IAuthService.cs`; `Services/AuthService.cs`; cap nhat `Interfaces/IUnitOfWork.cs` (them Users + RefreshTokens) + `DependencyInjection.cs` (dang ky IAuthService).
- Infrastructure: `Data/Configurations/RefreshTokenConfiguration.cs` (Token unique index, FK User cascade) + DbSet trong `AppDbContext`; `Repositories/UserRepository.cs`,`RefreshTokenRepository.cs` (repo refresh token Include(User) va KHONG AsNoTracking de update RevokedAt); cap nhat `UnitOfWork/UnitOfWork.cs`; `Security/JwtSettings.cs`,`JwtTokenGenerator.cs` (claims: jti + NameIdentifier + Email + Name + Role),`CurrentUser.cs` (IHttpContextAccessor),`PasswordHasher.cs` (BCrypt); cap nhat `DependencyInjection.cs` (bind JwtSettings, AddHttpContextAccessor, dang ky generator/hasher/currentuser/2 repo).
- Api: `Controllers/AuthController.cs` (register/login/refresh/logout, [AllowAnonymous]); them `[Authorize]` vao `EquipmentController`; cap nhat `Program.cs` (AddAuthentication + AddJwtBearer voi TokenValidationParameters, UseAuthentication/UseAuthorization, Swagger nut Bearer); them section `Jwt` trong `appsettings.json`.
- Migration: `20260629103656_AddRefreshToken` (bang RefreshTokens) - da apply.

Package moi: `System.IdentityModel.Tokens.Jwt 8.2.1` (Infrastructure), `Microsoft.AspNetCore.Authentication.JwtBearer 8.0.11` (Api).

Ket qua test (LocalDB that):
- `GET /api/equipment` khong token -> 401; co token -> 200 (12 thiet bi).
- register -> 201 + access/refresh; login -> ok; refresh -> cap token moi (access khac + refresh khac).
- Dung lai refresh da thu hoi / da logout -> 401; sai mat khau -> 401.
- `dotnet build`: 0 error.

Bonus da gan o Slice 2: refresh token (Section 15).

### 3d. Slice 3 - Equipment + Category CRUD (DONE, da test chay that)
Full CRUD cho Equipment va EquipmentCategory; phan quyen: GET = Authenticated, POST/PUT/DELETE = Admin + Staff. FluentValidation cho create/update DTOs.

Files da tao/sua:
- Application: DTOs `CreateEquipmentDto`/`UpdateEquipmentDto`/`EquipmentCategoryDto`/`CreateEquipmentCategoryDto`/`UpdateEquipmentCategoryDto`; `IEquipmentCategoryService` + `EquipmentCategoryService`; mo rong `IEquipmentService`/`EquipmentService` (Create/Update/Delete); `IEquipmentCategoryRepository`; mo rong `IEquipmentRepository` (SerialNumberExists, HasActiveBorrowings); `Validators/` (4 validator); cap nhat `IUnitOfWork`, `MappingProfile`, `DependencyInjection` (AddValidatorsFromAssemblyContaining).
- Infrastructure: `EquipmentCategoryRepository`; mo rong `EquipmentRepository`; cap nhat `UnitOfWork`, `DependencyInjection`.
- Api: `EquipmentCategoriesController`; mo rong `EquipmentController` (POST/PUT/DELETE + [Authorize(Roles=Admin,Staff)]); `Program.cs` AddFluentValidationAutoValidation.

Business rules trong service:
- Equipment: SerialNumber unique; khong xoa neu con yeu cau muon active (Pending/Approved/InProgress/Overdue/Returned); tao moi mac dinh status Available.
- Category: ten unique; khong xoa neu con thiet bi thuoc danh muc.

Package moi: FluentValidation 11.11.0 + FluentValidation.DependencyInjectionExtensions (Application), FluentValidation.AspNetCore 11.3.0 (Api).

Ket qua test (LocalDB that):
- User POST equipment -> 403; Staff tao/sua/xoa equipment -> ok; FluentValidation empty name -> 400; duplicate serial -> 409; xoa category con thiet bi -> 400; xoa equipment -> 204; User GET categories -> ok.
- `dotnet build`: 0 error.

Bonus da gan o Slice 3: FluentValidation (Section 15).

### 3e. Slice 4 - Search / filter / sort / pagination (DONE, da test chay that)
`GET /api/equipment` tra `PagedResult<EquipmentDto>` thay vi list phang. Query string: `search`, `categoryId`, `status`, `sortBy`, `sortDirection`, `pageNumber`, `pageSize`.

Files da tao/sua:
- Application: `Common/PagedResult.cs`, `Common/PaginationParams.cs`, `DTOs/EquipmentQueryParams.cs`; `IEquipmentService.GetPagedAsync`; `EquipmentService.GetPagedAsync` (validate status filter); `IEquipmentRepository.GetPagedWithCategoryAsync` (thay GetAllWithCategoryAsync).
- Infrastructure: `EquipmentRepository.GetPagedWithCategoryAsync` (search name/serial, filter category/status, sort whitelist: name/serialNumber/status/categoryName/id, skip/take).
- Api: `EquipmentController.GetAll([FromQuery] EquipmentQueryParams)`.

Ket qua test (LocalDB that):
- Default page -> total=12, page=1, size=10, items=10, totalPages=2.
- pageNumber=2&pageSize=5 -> 5 items, hasPrevious=true.
- status=Available -> loc dung; status=Invalid -> 400.
- categoryId + sortBy=name&sortDirection=desc -> sap xep dung.
- `dotnet build`: 0 error.

Bonus da gan o Slice 4: standardized pagination (Section 15).

### 3f. Slice 5 - Borrow/return workflow (DONE, da mo rong Plan A + REST refactor)
Full borrow lifecycle: create, approve, reject, cancel, handover, return + business rules + in-app notifications + auto-cancel Approved qua han.

Files chinh:
- Application: DTOs `BorrowRequestDto`/`BorrowRequestItemDto`/`CreateBorrowRequestDto`/`UpdateBorrowRequestDto`; `BorrowRequestService.UpdateAsync` dieu phoi theo `status`; private methods Approve/Reject/Cancel/Handover/Return; `EquipmentRules`; validators `CreateBorrowRequestDtoValidator`, `UpdateBorrowRequestDtoValidator`.
- Domain: `Equipment.CurrentCondition`; enum `EquipmentStatus` them Reserved/Lost/Compensated; `BorrowRequestItem.HandoverNote`/`ReturnNote`.
- Infrastructure: `BorrowRequestRepository`, `BorrowRequestExpirationHostedService`; migration `20260707033214_EquipmentConditionWorkflow`.
- Api: `BorrowRequestsController` — GET/POST/**PATCH** `{id}` (khong con PUT /approve|reject|...).

Endpoints (hien tai):
- POST /api/borrow-requests
- GET /api/borrow-requests, GET /api/borrow-requests/{id}
- PATCH /api/borrow-requests/{id} — body `{ status, rejectReason?, staffNote?, items? }`

Transitions PATCH:
| status | Tu trang thai | Quyen |
|---|---|---|
| Approved | Pending | Staff/Admin |
| Rejected | Pending | Staff/Admin (+ rejectReason) |
| Cancelled | Pending, Approved | Owner |
| InProgress | Approved | Staff/Admin (+ items handover) |
| Completed | InProgress, Overdue | Staff/Admin (+ items return) |

Business rules (tom tat): Available+Good/Fair moi muon; duyet -> Reserved; ban giao -> Borrowed; tra -> map condition; Lost -> Lost (khong Retired); job tu huy Approved qua BorrowDate.

Bonus: in-app notifications; hosted service auto-cancel.

### 3g. Slice 6 - Reports + dashboard (DONE, da test chay that)
Bao cao va dashboard cho Staff/Admin: borrow-summary, overdue-requests, dashboard stats.

Files da tao/sua:
- Application: `DTOs/Reports/` (`BorrowSummaryDto`, `BorrowSummaryQueryParams`, `OverdueRequestDto`, `DashboardStatsDto`, `StatusCountDto`, `MostBorrowedEquipmentDto`, `EquipmentStatusCountDto`); `IReportRepository`; `IReportService` + `ReportService`; cap nhat `IUnitOfWork`, `DependencyInjection`.
- Infrastructure: `ReportRepository` (aggregate queries); cap nhat `UnitOfWork`, `DependencyInjection`.
- Api: `ReportsController` ([Authorize(Roles=Admin,Staff)]).

Endpoints:
- GET /api/reports/borrow-summary?fromDate=&toDate= (optional date filter theo RequestDate)
- GET /api/reports/overdue-requests (Status=Overdue hoac Approved/InProgress qua ExpectedReturnDate)
- GET /api/reports/dashboard (equipment by status, borrow by status, overdue count, damaged returns, top 5 most-borrowed)

Ket qua test (LocalDB that):
- User GET reports -> 403; khong token -> 401.
- Staff borrow-summary -> 200 (counts by status, completed/active/rejected/cancelled).
- Staff overdue-requests -> 200 (danh sach qua han kem daysOverdue + items).
- Staff dashboard -> 200 (equipmentByStatus, borrowRequestsByStatus, overdueRequestCount, damagedReturnItemCount, mostBorrowedEquipment).
- fromDate > toDate -> 400.
- `dotnet build`: 0 error.

Bonus da gan o Slice 6: dashboard stats (Section 15).

### 3h. Slice 7 - Audit log + soft delete (DONE, da test chay that)
Soft delete tren BaseEntity + global query filter; audit log tu dong qua SaveChanges interceptor; Admin xem audit logs.

Files da tao/sua:
- Domain: `BaseEntity` them `IsDeleted`, `DeletedAt`; entity `AuditLog`; enum `AuditAction` (Created/Updated/Deleted).
- Application: `DTOs/Audit/` (`AuditLogDto`, `AuditLogQueryParams`); `IAuditLogRepository`, `IAuditLogService`, `AuditLogService`; cap nhat `IUnitOfWork`, `DependencyInjection`.
- Infrastructure: `Audit/AuditSaveChangesInterceptor` (ghi audit sau SaveChanges, dung `ICurrentUser` qua `IHttpContextAccessor`); `AuditLogRepository`; `AuditLogConfiguration`; `AppDbContext` global soft-delete filter + `DbSet<AuditLog>`; `GenericRepository.Delete` -> soft delete; cap nhat `UnitOfWork`, `DependencyInjection` (AddInterceptors).
- Api: `AuditLogsController` ([Authorize(Roles=Admin)]).
- Migration: `20260705030758_AddAuditLogAndSoftDelete` (IsDeleted/DeletedAt tren tat ca bang BaseEntity + bang AuditLogs).

Endpoints:
- GET /api/audit-logs?entityName=&action=&pageNumber=&pageSize= (Admin only)

Cach hoat dong:
- Delete qua repository: set `IsDeleted=true`, `DeletedAt=UtcNow` thay vi xoa row.
- Global query filter: moi entity ke thua BaseEntity tu dong loc `!IsDeleted`.
- Interceptor: Created/Updated/soft-Deleted ghi vao `AuditLogs` (bo qua AuditLog, RefreshToken).

Ket qua test (LocalDB that):
- Staff tao equipment -> audit Created (userId=2, staff@ebms.local).
- Staff DELETE equipment -> GET by id 404 (an khoi list); audit Deleted voi changes JSON.
- Staff GET /api/audit-logs -> 403; Admin -> 200 + PagedResult.
- Filter entityName=Equipment, action=Deleted -> dung.
- `dotnet build`: 0 error; migration apply luc khoi dong.

Bonus da gan o Slice 7: audit log, soft delete (Section 15).

### 3i. Slice 8 (phan OData) - OData EDM + EnableQuery (DONE, da test chay that)
OData doc lap cho cac use case KHONG trung REST — khong con /odata/Equipment hay /odata/BorrowRequests.

Files da tao/sua:
- Api: `OData/EdmModelBuilder.cs`; `OData/EquipmentCategoriesController.cs`, `ReturnRecordsController.cs`, `BorrowRequestItemsController.cs`; cap nhat `Program.cs` (AddOData); csproj (Microsoft.AspNetCore.OData 8.2.5).

Endpoints OData (doc lap voi REST):
| OData | REST tuong ung | Vi sao OData |
|---|---|---|
| GET /odata/EquipmentCategories?$expand=equipments | GET /api/equipment-categories (flat) | Catalog: category + thiet bi nested, filter tren equipments |
| GET /odata/ReturnRecords?$expand=borrowRequest | Khong co REST list | Lich su tra thiet bi, query Staff/Admin |
| GET /odata/BorrowRequestItems?$expand=equipment,borrowRequest | Items chi trong /api/borrow-requests/{id} | Truy van cap dong (vd. item tra Damaged) |

Quyen: EquipmentCategories = Authenticated; ReturnRecords + BorrowRequestItems = Admin/Staff.
Notifications: khong dung OData — Slice 10 REST (`GET/PUT /api/notifications`).

Ket qua test:
- /odata/Equipment, /odata/BorrowRequests, /odata/Notifications -> 404 (da go).
- EquipmentCategories $expand=equipments -> ok; ReturnRecords, BorrowRequestItems -> ok.
- REST /api/equipment van hoat dong.

Phan Slice 8 con lai: XML content negotiation.

### 3j. Slice 8 (phan XML) - Content negotiation JSON/XML (DONE, da test chay that)
REST `/api/equipment` ho tro ca JSON va XML; OData van chi JSON.

Files da tao/sua:
- Api: `Program.cs` (`AddXmlSerializerFormatters`); `Controllers/EquipmentController.cs` (`[Produces]`/`[Consumes]` JSON+XML); `OData/*Controller.cs` (`[Produces("application/json")]`); csproj (`Microsoft.AspNetCore.Mvc.Formatters.Xml` 2.3.11).
- Application: `EquipmentDto` (`[XmlRoot("Equipment")]`); `PagedResult<T>` (`[XmlRoot("PagedResult")]`, `[XmlElement("Item")]` cho list).

Demo:
- `GET /api/equipment` + `Accept: application/json` -> JSON (`PagedResult<EquipmentDto>`).
- `GET /api/equipment` + `Accept: application/xml` -> XML (`<PagedResult><Item>...</Item></PagedResult>`).
- `GET /api/equipment/{id}` + `Accept: application/xml` -> XML (`<Equipment>...</Equipment>`).
- POST/PUT `/api/equipment` chap nhan body JSON hoac XML (`Content-Type` tuong ung).
- `GET /odata/EquipmentCategories` + `Accept: application/xml` -> van JSON (OData khong negotiate XML).

Ket qua test (LocalDB :5171):
- JSON list 200 `application/json`; XML list 200 `application/xml`; XML by-id 200 `application/xml`.
- OData + Accept XML -> 200 `application/json` (dung nhu thiet ke).

### 3k. Slice 10 (phan Users REST) - Admin user management UC-AD1 (DONE)
Admin quan ly user: tao Staff; bat/tat `IsActive` qua PATCH — **KHONG DELETE**.

Files:
- Application: `UserDto`, `CreateUserDto`, `UpdateUserDto`; `UserService.UpdateAsync`; `UpdateUserDtoValidator`.
- Api: `UsersController` — GET/POST/**PATCH** `{id}` (Admin only).

Endpoints:
| Method | Route | Mo ta |
|---|---|---|
| GET | /api/users | Danh sach user |
| GET | /api/users/{id} | Chi tiet user |
| POST | /api/users | Tao Staff |
| PATCH | /api/users/{id} | `{ "isActive": true/false }` |

Rules: chi Admin; email unique; Admin khong tu deactivate chinh minh; IsActive=false khong login.

### 3l. Slice 10 (phan Client) - Vanilla JS + jQuery (DONE)
Multi-file HTML, jQuery CDN + `api.js` wrapper (`$.ajax` + refresh token).

Cau truc `client/`:
- Auth: `login.html`, `register.html`, `index.html` -> redirect
- Feature: `categories.html`, `equipment.html`, `equipment-detail.html`, `borrow.html`, `manage.html`, `users.html`, `user-detail.html`, `reports.html`, `audit-logs.html`
- Placeholder: `notifications.html` (cho REST notifications)
- Shared: `css/styles.css`, `js/config.js`, `api.js`, `auth.js`, `utils.js`, `borrow-cart.js` (+ page-specific JS)

API client:
- Borrow: `PATCH /api/borrow-requests/{id}` (`borrow.js`)
- Users: `PATCH /api/users/{id}` voi `{ isActive }` (`users.js`, `user-detail.js`)
- Equipment workflow: `PUT /api/equipment/{id}` cho bao tri/den bu (`manage.js`)
- Gio muon: `sessionStorage` key `ebms_borrow_cart`

CORS: policy `Client` (AllowAnyOrigin — ho tro file://).
Chay: mo `client/login.html`; API `http://localhost:5171` (`js/config.js`).

### 3m. Workflow redesign Plan A (DONE)
- Reserved sau duyet; ban giao rieng; `CurrentCondition` tren Equipment.
- Enum: EquipmentStatus (+Reserved, Lost, Compensated); EquipmentCondition (+Compensated).
- Migration: `20260707033214_EquipmentConditionWorkflow`.
- Seed cap nhat trong `DbInitializer`.

### 3n. REST API refactor (DONE)
Gop endpoint RPC thanh resource update:
- Borrow: 1 endpoint `PATCH /api/borrow-requests/{id}` thay 5 PUT action URLs.
- Users: `PATCH /api/users/{id}` thay activate/deactivate.
- Equipment: `PUT /api/equipment/{id}` thay complete-maintenance / confirm-compensation.
- Xoa DTO cu: `RejectBorrowRequestDto`, `ReturnBorrowRequestDto`, validators tuong ung.
- Them: `UpdateBorrowRequestDto`, `UpdateUserDto`, `UpdateEquipmentDto.CurrentCondition`.

### 3o. Equipment PUT transitions (DONE)
`EquipmentService.ValidateEquipmentTransition`:
- Maintenance -> Available (condition Good/Fair): hoan tat bao tri.
- Lost -> Compensated: xac nhan den bu.
- Khong doi status khi Reserved/Borrowed; Lost/Borrowed chi qua borrow workflow.

## 4. Cac luu y ky thuat QUAN TRONG (de khong vap lai)

- .NET 8 (net8.0). LocalDB co san: instance `MSSQLLocalDB`. Connection string trong `appsettings.json`: `Server=(localdb)\mssqllocaldb;Database=EquipmentBorrowingDb;...`.
- Chay API: `dotnet run --project src/EquipmentBorrowingManagementSystem.Api --launch-profile http` -> http://localhost:5171 ; Swagger: http://localhost:5171/swagger. App tu dong Migrate + Seed luc khoi dong.
- Package: AutoMapper 16.1.1 (dang ky qua `services.AddAutoMapper(cfg => { }, typeof(MappingProfile).Assembly)`); Serilog.AspNetCore 8.0.3; EF Core 8.0.11; BCrypt.Net-Next 4.0.3 (da co san trong Infrastructure).
- `Application.csproj` co `<FrameworkReference Include="Microsoft.AspNetCore.App" />` de dung `StatusCodes` trong `Result.cs`. KHONG pin cac package `Microsoft.Extensions.*.Abstractions` o Infrastructure nua (da go) vi gay loi NU1605 package downgrade (framework cung cap version moi hon ban pin tay).
- PowerShell tren may nay: KHONG dung `&&` de noi lenh; dung `;` hoac chay rieng. Dung tham so `working_directory` thay vi `cd`.
- Di chuyen file: dung `git mv` (giu history) roi sua namespace.
- Luu y moi truong: che do co the bi reset ve Plan o dau moi luot; neu can sua file code (.cs/.csproj) ma bi chan, goi SwitchMode target=agent roi sua trong cung luot.
- BAY 1 (file lock): build fail voi loi MSB3021/MSB3027 "file is locked / used by another process" = van con tien trinh API cu dang chay. Kill truoc khi build: `Get-Process -Name "EquipmentBorrowingManagementSystem.Api","dotnet" | Stop-Process -Force`.
- BAY 2 (migration): `dotnet ef migrations add` CHI sinh file .cs, KHONG bien dich lai assembly. Neu chay `dotnet run --no-build` ngay sau do, app dung assembly cu va bao "No migrations were applied". PHAI `dotnet build` lai roi moi `dotnet run` thi migration moi duoc apply luc khoi dong.
- JWT: claims dung ClaimTypes.NameIdentifier/Email/Name/Role + `jti` (Guid) de moi access token la duy nhat. `ClockSkew = TimeSpan.Zero`. Key/Issuer/Audience trong appsettings section `Jwt` (Key hien la gia tri dev - doi sang secret that khi deploy).

## 5. Tai khoan mau (da seed)

| Role | Email | Password |
|---|---|---|
| Admin | admin@ebms.local | Admin@123 |
| Staff | staff@ebms.local | Staff@123 |
| User | user@ebms.local | User@123 |

## 6. Du an tham khao (de bat chuoc style/cau truc)

- `D:\Documents\ASP.NET MVC\source\repos\BookManagement\ExtraCurricularManagement` (MVC; co Application/{Constants,DTOs,Interfaces,Mappings,Services}, Infrastructure/{Data,Repositories,Seeders,UnitOfWork}).
- `D:\Downloads\26_05_2026___2d7efe3388c8f847e103a6dac6ae0e07. (1)\LibraryApi_4Layers` (Web API 4 layer; Result pattern, ApiControllerBase, IGenericRepository + UnitOfWork, AutoMapper Profile, service co business rules, DTO co DataAnnotation). DAY LA REFERENCE CHINH.

## 7. Entity & nghiep vu (tom tat - chi tiet o PROJECT_DOCUMENTATION.md)

Entity hien co (Domain): User, EquipmentCategory, Equipment (+ CurrentCondition), BorrowRequest, BorrowRequestItem (+ HandoverNote, ReturnNote), ReturnRecord, Notification, RefreshToken, AuditLog. + BaseEntity (Id, CreatedAt, IsDeleted, DeletedAt).

Enum:
- BorrowRequestStatus: Pending, Approved, Rejected, Cancelled, InProgress, Returned (legacy trong repo queries), Completed, Overdue
- EquipmentStatus: Available, Borrowed, Maintenance, Retired, Reserved, Lost, Compensated
- EquipmentCondition: Good, Fair, Damaged, Lost, Compensated
- NotificationType, UserRole, AuditAction

Business rules (service layer) — xem chi tiet PROJECT_DOCUMENTATION muc 5.

Workflow BorrowRequest: Pending -> Approved -> InProgress -> Completed (+ Rejected, Cancelled, Overdue). Khong con buoc Returned trung gian trong API hien tai.

Workflow Equipment (trong muon): Available -> Reserved -> Borrowed -> (tra) Available/Maintenance/Lost -> (bao tri) Available; Lost -> Compensated.

## 8. CONG VIEC TIEP THEO (cac slice con lai)

Lam tuan tu theo vertical slice, moi slice build xanh + test Swagger truoc khi sang slice sau. Chi tiet day du o file plan: `.cursor/plans/complete_equipment_borrowing_system_c0493f75.plan.md`.

- Slice 1-8: DONE.
- Slice 5 mo rong (Plan A workflow) + REST refactor: DONE (muc 3f, 3m, 3n, 3o).
- Slice 10 Users REST + Client feature pages: DONE (muc 3k, 3l).
- Slice 9 - gRPC: project moi `EquipmentBorrowingManagementSystem.Grpc` + `Infrastructure/Grpc/NotificationClient`.
- Slice 10 (con lai) - Notifications REST: `GET/PATCH /api/notifications` + hoan thien `notifications.html`.
- Slice 11 - Docker Compose (SQL Server + Api + Grpc) + Dockerfile.
- Slice 12 - Docs: cap nhat Postman collection (ERD.dbml da cap nhat).

## 9. PROMPT KHOI DONG CHO SESSION MOI

Copy doan duoi vao Cursor o session moi (mo dung workspace nay):

---
Toi dang lam tiep du an Equipment Borrowing Management System (P1 / PRN232), backend ASP.NET Core 8 Web API chia 4 project (Domain/Application/Infrastructure/Api).

TRUOC TIEN hay doc 3 file de lay context (KHONG hoi lai nhung gi da co trong do):
1. docs/PROJECT_STATE.md  (trang thai hien tai + cac quyet dinh da chot + luu y ky thuat + cong viec con lai)
2. .cursor/plans/complete_equipment_borrowing_system_c0493f75.plan.md  (plan 12 slice chi tiet)
3. docs/PROJECT_DOCUMENTATION.md  (phan tich/thiet ke: role, use case, business rules, workflow, API list, security matrix)

Tham khao style code tu 2 du an: "D:\Documents\ASP.NET MVC\source\repos\BookManagement\ExtraCurricularManagement" va "D:\Downloads\26_05_2026___2d7efe3388c8f847e103a6dac6ae0e07. (1)\LibraryApi_4Layers" (LibraryApi_4Layers la reference chinh: Result pattern, ApiControllerBase, IGenericRepository + UnitOfWork, AutoMapper, service giu business rules).

Nguyen tac: lam dung va du theo de bai + Section 15 bonus, KHONG lam thua, KHONG phuc tap hoa; lam theo tung vertical slice, moi slice phai build xanh (dotnet build) + test endpoint truoc khi sang slice tiep; dieu gi mo ho thi hoi lai toi truoc.

Trang thai: Slice 1-8 DA XONG; Plan A workflow + REST refactor DA XONG; Users REST + Client feature pages DA XONG. Tiep theo: Slice 9 gRPC, Notifications REST, Docker Compose, cap nhat ERD.dbml/Postman.

API REST hien tai (tom tat):
- Borrow: PATCH /api/borrow-requests/{id} (status transitions)
- Users: PATCH /api/users/{id} ({ isActive })
- Equipment: PUT /api/equipment/{id} (status + currentCondition)
- Khong con: /approve, /reject, /cancel, /handover, /return, /activate, /deactivate, /complete-maintenance, /confirm-compensation

Luu y moi truong: Windows PowerShell (khong dung &&, dung working_directory), LocalDB instance MSSQLLocalDB co san, API chay o http://localhost:5171 (Swagger /swagger), tu dong migrate + seed luc khoi dong. Tai khoan mau: admin@ebms.local/Admin@123, staff@ebms.local/Staff@123, user@ebms.local/User@123.
---
