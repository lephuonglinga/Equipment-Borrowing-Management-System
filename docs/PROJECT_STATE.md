# PROJECT STATE - Equipment Borrowing Management System (P1 / PRN232)

## 1. Current confirmed model

- Workflow da chuyen sang **status-only** (da bo toan bo Condition o Domain/Application/Infrastructure/Client/Docs).
- REST endpoint da refactor:
  - `PATCH /api/borrow-requests/{id}` cho toan bo transition cua don muon
  - `PATCH /api/users/{id}` cho bat/tat `isActive`
  - `PUT /api/equipment/{id}` cho cap nhat thiet bi + status
- Reserve-on-create da bat:
  - Tao don (`Pending`) => thiet bi `Reserved` ngay
  - Reject/Cancel/Auto-expire => `Reserved -> Available`
  - Handover => `Reserved -> Borrowed`
  - Return Completed => `Borrowed -> Available`
- Trang thai thiet bi hien tai:
  - `Available`, `Borrowed`, `Maintenance`, `Retired`, `Reserved`, `Lost`, `Compensated`
- Tat ca status duoc hien thi trong list/filter (bao gom `Lost`, `Compensated`).

## 2. Migration / Snapshot

- Da tao migration moi: `20260707100222_RemoveConditionWorkflow` de drop 4 cot legacy cua model cu.
- Da tao migration: `20260707101932_RemoveBorrowRequestReturnedStatus` — xoa `BorrowRequestStatus.Returned`, renumber `Completed=6`, `Overdue=7`, migrate du lieu DB.
- Snapshot `AppDbContextModelSnapshot.cs` da dong bo theo schema moi.
- Da xoa migration cu lien quan Condition workflow de codebase sach.

## 3. Documentation / ERD

- `docs/ERD.dbml` da bo toan bo cot Condition.
- `docs/PROJECT_DOCUMENTATION.md` da cap nhat theo workflow status-only + state chart moi.

## 4. Client status filter

- `client/manage.html`:
  - Da co option filter `Compensated`
  - Dam bao chon duoc `Lost` va `Compensated`
- `client/js/manage.js`:
  - Query filter dung `status=<value>`
  - Bang list hien thi theo moi status nhan tu API.

## 5. Next check

- Chay migration len DB va smoke test nhanh:
  - `GET /api/equipment?status=Lost`
  - `GET /api/equipment?status=Compensated`
  - UI `manage.html` filter Lost/Compensated.
