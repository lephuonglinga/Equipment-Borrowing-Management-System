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
- Nghiep vu tra thiet bi: co kiem tra tinh trang tung item. Good/Fair -> Equipment.Available; Damaged -> Maintenance; Lost -> Retired. `ReturnRecord.OverallCondition` = tinh trang xau nhat (Good < Fair < Damaged < Lost) + staff note. Khong co quy trinh sua chua/den bu rieng.
- KHONG co phi muon, KHONG co phu phi mat/hong (muon mien phi). Mat/hong chi xu ly qua trang thai thiet bi.

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

## 4. Cac luu y ky thuat QUAN TRONG (de khong vap lai)

- .NET 8 (net8.0). LocalDB co san: instance `MSSQLLocalDB`. Connection string trong `appsettings.json`: `Server=(localdb)\mssqllocaldb;Database=EquipmentBorrowingDb;...`.
- Chay API: `dotnet run --project src/EquipmentBorrowingManagementSystem.Api --launch-profile http` -> http://localhost:5171 ; Swagger: http://localhost:5171/swagger. App tu dong Migrate + Seed luc khoi dong.
- Package: AutoMapper 16.1.1 (dang ky qua `services.AddAutoMapper(cfg => { }, typeof(MappingProfile).Assembly)`); Serilog.AspNetCore 8.0.3; EF Core 8.0.11; BCrypt.Net-Next 4.0.3 (da co san trong Infrastructure).
- `Application.csproj` co `<FrameworkReference Include="Microsoft.AspNetCore.App" />` de dung `StatusCodes` trong `Result.cs`. KHONG pin cac package `Microsoft.Extensions.*.Abstractions` o Infrastructure nua (da go) vi gay loi NU1605 package downgrade (framework cung cap version moi hon ban pin tay).
- PowerShell tren may nay: KHONG dung `&&` de noi lenh; dung `;` hoac chay rieng. Dung tham so `working_directory` thay vi `cd`.
- Di chuyen file: dung `git mv` (giu history) roi sua namespace.
- Luu y moi truong: che do co the bi reset ve Plan o dau moi luot; neu can sua file code (.cs/.csproj) ma bi chan, goi SwitchMode target=agent roi sua trong cung luot.

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

Entity hien co (Domain): User, EquipmentCategory, Equipment, BorrowRequest, BorrowRequestItem (bang trung gian n-n co thuoc tinh), ReturnRecord (1-1 voi BorrowRequest), Notification. + BaseEntity (Id, CreatedAt).
Enum: BorrowRequestStatus (Pending/Approved/Rejected/Cancelled/InProgress/Returned/Completed/Overdue), EquipmentCondition (Good/Fair/Damaged/Lost), EquipmentStatus (Available/Borrowed/Maintenance/Retired), NotificationType, UserRole (Admin/Staff/User).

Entity se them (planned, phuc vu bonus): RefreshToken, AuditLog; BaseEntity them IsDeleted + DeletedAt (soft delete + global query filter).

5 business rules (dat o service layer):
1. Thiet bi khong Available -> khong them vao yeu cau moi.
2. User dang co yeu cau Overdue -> khong tao yeu cau moi.
3. Duyet/tu choi chi Staff/Admin, chi voi yeu cau Pending.
4. ExpectedReturnDate >= BorrowDate.
5. Khi tra: map tinh trang -> trang thai thiet bi (xem muc 1).

Workflow: Pending -> Approved -> Returned -> Completed ; + Rejected, Cancelled, Overdue.

## 8. CONG VIEC TIEP THEO (cac slice con lai)

Lam tuan tu theo vertical slice, moi slice build xanh + test Swagger truoc khi sang slice sau. Chi tiet day du o file plan: `.cursor/plans/complete_equipment_borrowing_system_c0493f75.plan.md`.

- Slice 1 - Walking skeleton: DONE.
- Slice 2 - Auth: register/login + JWT + 3 role + refresh token (them RefreshToken entity + migration) + `[Authorize]`. Them DTOs (Register/Login/AuthResponse/RefreshToken), `Constants/Roles.cs`, `IJwtTokenGenerator` + `ICurrentUser`, `Infrastructure/Security/JwtTokenGenerator.cs` + `CurrentUser.cs` (IHttpContextAccessor), `AuthService`, `AuthController`; cau hinh JWT bearer + Swagger bearer; them section `Jwt` trong appsettings. (TIEP THEO NGAY)
- Slice 3 - CRUD: Equipment + EquipmentCategory full CRUD + role authz + FluentValidation (Validators/).
- Slice 4 - Search/paging: Equipment search/filter/sort + `PagedResult`/`PaginationParams`.
- Slice 5 - Borrow workflow: BorrowRequest create/approve/reject/cancel/return + 5 rules + in-app Notification + user chi xem cua minh.
- Slice 6 - Reports: borrow-summary, overdue-requests, dashboard (Staff/Admin).
- Slice 7 - Cross-cutting bonus: audit log (SaveChanges interceptor) + soft delete (BaseEntity flags + global query filter) + migration.
- Slice 8 - OData (2 endpoint: Equipment, BorrowRequests) + XML content negotiation (AddXmlSerializerFormatters + [Produces]/[Consumes]). OData de JSON de tranh xung dot formatter.
- Slice 9 - gRPC: project moi `EquipmentBorrowingManagementSystem.Grpc` (notification.proto + NotificationGrpcService) + `Infrastructure/Grpc/NotificationClient` ; API goi khi approve/reject/return.
- Slice 10 - Client vanilla JS: `client/` (login + luu JWT localStorage, list equipment, create/update, gui borrow request, history, xu ly 401/403/404/400) + CORS.
- Slice 11 - Docker Compose (SQL Server + Api + Grpc) + Dockerfile.
- Slice 12 - Docs: hoan thien PROJECT_DOCUMENTATION (da co ban nhap), cap nhat ERD.dbml (them RefreshToken/AuditLog/soft delete), Postman collection.

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

Trang thai: Slice 1 (walking skeleton, Equipment read API) DA XONG va test chay duoc. Hay bat dau Slice 2 (Authentication: JWT + 3 role + refresh token) theo plan.

Luu y moi truong: Windows PowerShell (khong dung &&, dung working_directory), LocalDB instance MSSQLLocalDB co san, API chay o http://localhost:5171 (Swagger /swagger), tu dong migrate + seed luc khoi dong. Tai khoan mau: admin@ebms.local/Admin@123, staff@ebms.local/Staff@123, user@ebms.local/User@123.
---
