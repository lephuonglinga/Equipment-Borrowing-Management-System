using EquipmentBorrowingManagementSystem.Domain.Entities;
using EquipmentBorrowingManagementSystem.Domain.Enums;
using EquipmentBorrowingManagementSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowingManagementSystem.Infrastructure.Seeders;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await TruncateAllAsync(context);

        var now = DateTime.UtcNow;
        var today = now.Date;

        var admin = new User
        {
            Email = "admin@ebms.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            FullName = "System Admin",
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = today.AddDays(-30)
        };

        var staff = new User
        {
            Email = "staff@ebms.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff@123"),
            FullName = "Equipment Staff",
            Role = UserRole.Staff,
            IsActive = true,
            CreatedAt = today.AddDays(-30)
        };

        var user = new User
        {
            Email = "user@ebms.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
            FullName = "Regular User",
            Role = UserRole.User,
            IsActive = true,
            CreatedAt = today.AddDays(-30)
        };

        context.Users.AddRange(admin, staff, user);
        await context.SaveChangesAsync();

        var categories = new[]
        {
            new EquipmentCategory { Name = "Laptops", Description = "Portable computers", CreatedAt = today.AddDays(-30) },
            new EquipmentCategory { Name = "Cameras", Description = "Photo and video cameras", CreatedAt = today.AddDays(-30) },
            new EquipmentCategory { Name = "Audio", Description = "Microphones and speakers", CreatedAt = today.AddDays(-30) },
            new EquipmentCategory { Name = "Projectors", Description = "Presentation projectors", CreatedAt = today.AddDays(-30) },
            new EquipmentCategory { Name = "Tools", Description = "Hand tools and meters", CreatedAt = today.AddDays(-30) }
        };

        context.EquipmentCategories.AddRange(categories);
        await context.SaveChangesAsync();

        var equipments = new List<Equipment>
        {
            new() { Name = "Dell Latitude 5420", SerialNumber = "LAP-001", CategoryId = categories[0].Id, Status = EquipmentStatus.Available, Location = "Room A1", CreatedAt = today.AddDays(-30) },
            new() { Name = "MacBook Pro 14", SerialNumber = "LAP-002", CategoryId = categories[0].Id, Status = EquipmentStatus.Available, Location = "Room A1", CreatedAt = today.AddDays(-30) },
            new() { Name = "HP EliteBook", SerialNumber = "LAP-003", CategoryId = categories[0].Id, Status = EquipmentStatus.Reserved, Location = "Room A2", CreatedAt = today.AddDays(-30) },
            new() { Name = "Canon EOS R10", SerialNumber = "CAM-001", CategoryId = categories[1].Id, Status = EquipmentStatus.Available, Location = "Room B1", CreatedAt = today.AddDays(-30) },
            new() { Name = "Sony A7 III", SerialNumber = "CAM-002", CategoryId = categories[1].Id, Status = EquipmentStatus.Maintenance, Location = "Repair", CreatedAt = today.AddDays(-30) },
            new() { Name = "Shure SM58", SerialNumber = "AUD-001", CategoryId = categories[2].Id, Status = EquipmentStatus.Available, Location = "Room C1", CreatedAt = today.AddDays(-30) },
            new() { Name = "JBL Speaker", SerialNumber = "AUD-002", CategoryId = categories[2].Id, Status = EquipmentStatus.Damaged, Location = "Room C1", CreatedAt = today.AddDays(-30) },
            new() { Name = "Epson EB-X49", SerialNumber = "PRJ-001", CategoryId = categories[3].Id, Status = EquipmentStatus.Borrowed, Location = "Room D1", CreatedAt = today.AddDays(-30) },
            new() { Name = "BenQ MH535A", SerialNumber = "PRJ-002", CategoryId = categories[3].Id, Status = EquipmentStatus.Available, Location = "Room D2", CreatedAt = today.AddDays(-30) },
            new() { Name = "Digital Multimeter", SerialNumber = "TOL-001", CategoryId = categories[4].Id, Status = EquipmentStatus.Available, Location = "Lab 1", CreatedAt = today.AddDays(-30) },
            new() { Name = "Oscilloscope", SerialNumber = "TOL-002", CategoryId = categories[4].Id, Status = EquipmentStatus.Retired, Location = "Lab 1", CreatedAt = today.AddDays(-30) },
            new() { Name = "Lenovo ThinkPad", SerialNumber = "LAP-004", CategoryId = categories[0].Id, Status = EquipmentStatus.Available, Location = "Room A2", CreatedAt = today.AddDays(-30) }
        };

        context.Equipments.AddRange(equipments);
        await context.SaveChangesAsync();

        // Pending: borrow tomorrow, return in 4 days
        var pendingRequest = new BorrowRequest
        {
            UserId = user.Id,
            RequestDate = today.AddDays(-1),
            BorrowDate = today.AddDays(1),
            ExpectedReturnDate = today.AddDays(4),
            Status = BorrowRequestStatus.Pending,
            Purpose = "Project presentation rehearsal",
            CreatedAt = today.AddDays(-1)
        };

        // Approved / chờ bàn giao: borrow today, return in 7 days
        var approvedRequest = new BorrowRequest
        {
            UserId = user.Id,
            RequestDate = today.AddDays(-2),
            BorrowDate = today,
            ExpectedReturnDate = today.AddDays(7),
            Status = BorrowRequestStatus.Approved,
            Purpose = "Field recording session — chờ bàn giao",
            ApprovedById = staff.Id,
            ApprovedAt = today.AddDays(-2).AddHours(2),
            CreatedAt = today.AddDays(-2)
        };

        // Completed: borrowed last week, returned 2 days ago
        var completedRequest = new BorrowRequest
        {
            UserId = user.Id,
            RequestDate = today.AddDays(-10),
            BorrowDate = today.AddDays(-9),
            ExpectedReturnDate = today.AddDays(-3),
            ActualReturnDate = today.AddDays(-2),
            Status = BorrowRequestStatus.Completed,
            Purpose = "Lab workshop",
            ApprovedById = staff.Id,
            ApprovedAt = today.AddDays(-10).AddHours(1),
            CreatedAt = today.AddDays(-10)
        };

        // Rejected
        var rejectedRequest = new BorrowRequest
        {
            UserId = user.Id,
            RequestDate = today.AddDays(-5),
            BorrowDate = today.AddDays(-4),
            ExpectedReturnDate = today.AddDays(-1),
            Status = BorrowRequestStatus.Rejected,
            Purpose = "Personal use",
            RejectReason = "Insufficient business justification",
            ApprovedById = staff.Id,
            ApprovedAt = today.AddDays(-5).AddHours(3),
            CreatedAt = today.AddDays(-5)
        };

        // Overdue: borrowed 9 days ago, due 2 days ago, still out
        var overdueRequest = new BorrowRequest
        {
            UserId = user.Id,
            RequestDate = today.AddDays(-10),
            BorrowDate = today.AddDays(-9),
            ExpectedReturnDate = today.AddDays(-2),
            Status = BorrowRequestStatus.Overdue,
            Purpose = "Extended lab testing",
            ApprovedById = staff.Id,
            ApprovedAt = today.AddDays(-10).AddHours(1),
            CreatedAt = today.AddDays(-10)
        };

        context.BorrowRequests.AddRange(pendingRequest, approvedRequest, completedRequest, rejectedRequest, overdueRequest);
        await context.SaveChangesAsync();

        context.BorrowRequestItems.AddRange(
            new BorrowRequestItem
            {
                BorrowRequestId = pendingRequest.Id,
                EquipmentId = equipments[3].Id, // Canon — reserved while pending approval
                CreatedAt = today.AddDays(-1)
            },
            new BorrowRequestItem
            {
                BorrowRequestId = approvedRequest.Id,
                EquipmentId = equipments[2].Id, // HP Reserved
                CreatedAt = today.AddDays(-2)
            },
            new BorrowRequestItem
            {
                BorrowRequestId = completedRequest.Id,
                EquipmentId = equipments[5].Id, // Shure Available after return
                HandoverNote = "Đủ phụ kiện, còn hộp đựng",
                ReturnNote = "Trả đúng hạn, thiết bị hoạt động tốt",
                ReturnStatus = EquipmentStatus.Available,
                CreatedAt = today.AddDays(-10)
            },
            new BorrowRequestItem
            {
                BorrowRequestId = rejectedRequest.Id,
                EquipmentId = equipments[8].Id, // BenQ Available
                CreatedAt = today.AddDays(-5)
            },
            new BorrowRequestItem
            {
                BorrowRequestId = overdueRequest.Id,
                EquipmentId = equipments[7].Id, // Epson Borrowed
                HandoverNote = "Đủ phụ kiện",
                CreatedAt = today.AddDays(-10)
            }
        );

        // Pending request equipment should be Reserved
        equipments[3].Status = EquipmentStatus.Reserved;

        context.ReturnRecords.Add(new ReturnRecord
        {
            BorrowRequestId = completedRequest.Id,
            ReturnedAt = today.AddDays(-2),
            ReturnedById = staff.Id,
            StaffNote = "Returned in good condition",
            CreatedAt = today.AddDays(-2)
        });

        context.Notifications.AddRange(
            new Notification
            {
                UserId = user.Id,
                Title = "Request approved",
                Message = "Your borrow request has been approved.",
                Type = NotificationType.RequestApproved,
                IsRead = true,
                CreatedAt = today.AddDays(-2)
            },
            new Notification
            {
                UserId = user.Id,
                Title = "Request rejected",
                Message = "Your borrow request was rejected.",
                Type = NotificationType.RequestRejected,
                IsRead = false,
                CreatedAt = today.AddDays(-5)
            },
            new Notification
            {
                UserId = user.Id,
                Title = "Overdue reminder",
                Message = "Please return borrowed equipment.",
                Type = NotificationType.RequestOverdue,
                IsRead = false,
                CreatedAt = today.AddDays(-1)
            }
        );

        await context.SaveChangesAsync();
    }

    private static async Task TruncateAllAsync(AppDbContext context)
    {
        // Disable FK checks and wipe all business tables so seed always starts clean.
        await context.Database.ExecuteSqlRawAsync("""
            DELETE FROM Notifications;
            DELETE FROM ReturnRecords;
            DELETE FROM BorrowRequestItems;
            DELETE FROM BorrowRequests;
            DELETE FROM RefreshTokens;
            DELETE FROM Equipments;
            DELETE FROM EquipmentCategories;
            DELETE FROM Users;
            """);
    }
}
