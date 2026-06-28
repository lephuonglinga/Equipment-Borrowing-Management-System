# Equipment Borrowing Management System

**Đề tài:** P1 — PRN232

## 1. Giới thiệu

Hệ thống quản lý mượn, trả và theo dõi thiết bị. Backend ASP.NET Core Web API, SQL Server, JWT, OData, gRPC, client .NET.

## 2. Vai trò

| Role | Quyền |
|------|-------|
| Admin | Quản lý user, equipment, duyệt yêu cầu, báo cáo |
| Staff | Quản lý equipment, duyệt/từ chối, ghi nhận trả, báo cáo |
| User | Gửi/hủy yêu cầu mượn, xem lịch sử của mình |

## 3. Use case chính

- UC-01: User gửi yêu cầu mượn thiết bị
- UC-02: Staff duyệt/từ chối yêu cầu
- UC-03: Staff ghi nhận trả thiết bị
- UC-04: Admin quản lý danh mục thiết bị
- UC-05: Admin/Staff xem báo cáo

## 4. ERD

Xem [`ERD.dbml`](ERD.dbml).

**Entity (7):** User, EquipmentCategory, Equipment, BorrowRequest, BorrowRequestItem, ReturnRecord, Notification

_Các mục còn lại (business rules, API list, security matrix, hướng dẫn chạy) bổ sung khi triển khai từng phần theo đề._
