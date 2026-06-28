using EquipmentBorrowingManagementSystem.Domain.Entities;
using EquipmentBorrowingManagementSystem.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EquipmentBorrowingManagementSystem.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context)
    {
        if (await context.Users.AnyAsync())
        {
            return;
        }

        var seedTime = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc);

        var admin = new User
        {
            Email = "admin@ebms.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            FullName = "System Admin",
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = seedTime
        };

        var staff = new User
        {
            Email = "staff@ebms.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff@123"),
            FullName = "Equipment Staff",
            Role = UserRole.Staff,
            IsActive = true,
            CreatedAt = seedTime
        };

        var user = new User
        {
            Email = "user@ebms.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
            FullName = "Regular User",
            Role = UserRole.User,
            IsActive = true,
            CreatedAt = seedTime
        };

        context.Users.AddRange(admin, staff, user);
        await context.SaveChangesAsync();

        var categories = new[]
        {
            new EquipmentCategory { Name = "Laptops", Description = "Portable computers", CreatedAt = seedTime },
            new EquipmentCategory { Name = "Cameras", Description = "Photo and video cameras", CreatedAt = seedTime },
            new EquipmentCategory { Name = "Audio", Description = "Microphones and speakers", CreatedAt = seedTime },
            new EquipmentCategory { Name = "Projectors", Description = "Presentation projectors", CreatedAt = seedTime },
            new EquipmentCategory { Name = "Tools", Description = "Hand tools and meters", CreatedAt = seedTime }
        };

        context.EquipmentCategories.AddRange(categories);
        await context.SaveChangesAsync();

        var equipments = new List<Equipment>
        {
            new() { Name = "Dell Latitude 5420", SerialNumber = "LAP-001", CategoryId = categories[0].Id, Status = EquipmentStatus.Available, Location = "Room A1", CreatedAt = seedTime },
            new() { Name = "MacBook Pro 14", SerialNumber = "LAP-002", CategoryId = categories[0].Id, Status = EquipmentStatus.Borrowed, Location = "Room A1", CreatedAt = seedTime },
            new() { Name = "HP EliteBook", SerialNumber = "LAP-003", CategoryId = categories[0].Id, Status = EquipmentStatus.Available, Location = "Room A2", CreatedAt = seedTime },
            new() { Name = "Canon EOS R10", SerialNumber = "CAM-001", CategoryId = categories[1].Id, Status = EquipmentStatus.Available, Location = "Room B1", CreatedAt = seedTime },
            new() { Name = "Sony A7 III", SerialNumber = "CAM-002", CategoryId = categories[1].Id, Status = EquipmentStatus.Maintenance, Location = "Repair", CreatedAt = seedTime },
            new() { Name = "Shure SM58", SerialNumber = "AUD-001", CategoryId = categories[2].Id, Status = EquipmentStatus.Available, Location = "Room C1", CreatedAt = seedTime },
            new() { Name = "JBL Speaker", SerialNumber = "AUD-002", CategoryId = categories[2].Id, Status = EquipmentStatus.Available, Location = "Room C1", CreatedAt = seedTime },
            new() { Name = "Epson EB-X49", SerialNumber = "PRJ-001", CategoryId = categories[3].Id, Status = EquipmentStatus.Borrowed, Location = "Room D1", CreatedAt = seedTime },
            new() { Name = "BenQ MH535A", SerialNumber = "PRJ-002", CategoryId = categories[3].Id, Status = EquipmentStatus.Available, Location = "Room D2", CreatedAt = seedTime },
            new() { Name = "Digital Multimeter", SerialNumber = "TOL-001", CategoryId = categories[4].Id, Status = EquipmentStatus.Available, Location = "Lab 1", CreatedAt = seedTime },
            new() { Name = "Oscilloscope", SerialNumber = "TOL-002", CategoryId = categories[4].Id, Status = EquipmentStatus.Available, Location = "Lab 1", CreatedAt = seedTime },
            new() { Name = "Lenovo ThinkPad", SerialNumber = "LAP-004", CategoryId = categories[0].Id, Status = EquipmentStatus.Available, Location = "Room A2", CreatedAt = seedTime }
        };

        context.Equipments.AddRange(equipments);
        await context.SaveChangesAsync();

        var laptopBorrowed = equipments[1];
        var projectorBorrowed = equipments[7];

        var pendingRequest = new BorrowRequest
        {
            UserId = user.Id,
            RequestDate = seedTime.AddDays(10),
            BorrowDate = seedTime.AddDays(12),
            ExpectedReturnDate = seedTime.AddDays(15),
            Status = BorrowRequestStatus.Pending,
            Purpose = "Project presentation rehearsal",
            CreatedAt = seedTime.AddDays(10)
        };

        var approvedRequest = new BorrowRequest
        {
            UserId = user.Id,
            RequestDate = seedTime.AddDays(5),
            BorrowDate = seedTime.AddDays(6),
            ExpectedReturnDate = seedTime.AddDays(20),
            Status = BorrowRequestStatus.Approved,
            Purpose = "Field recording session",
            ApprovedById = staff.Id,
            ApprovedAt = seedTime.AddDays(5).AddHours(2),
            CreatedAt = seedTime.AddDays(5)
        };

        var completedRequest = new BorrowRequest
        {
            UserId = user.Id,
            RequestDate = seedTime.AddDays(1),
            BorrowDate = seedTime.AddDays(2),
            ExpectedReturnDate = seedTime.AddDays(8),
            Status = BorrowRequestStatus.Completed,
            Purpose = "Lab workshop",
            ApprovedById = staff.Id,
            ApprovedAt = seedTime.AddDays(1).AddHours(1),
            CreatedAt = seedTime.AddDays(1)
        };

        var rejectedRequest = new BorrowRequest
        {
            UserId = user.Id,
            RequestDate = seedTime.AddDays(7),
            BorrowDate = seedTime.AddDays(8),
            ExpectedReturnDate = seedTime.AddDays(9),
            Status = BorrowRequestStatus.Rejected,
            Purpose = "Personal use",
            RejectReason = "Insufficient business justification",
            ApprovedById = staff.Id,
            ApprovedAt = seedTime.AddDays(7).AddHours(3),
            CreatedAt = seedTime.AddDays(7)
        };

        var overdueRequest = new BorrowRequest
        {
            UserId = user.Id,
            RequestDate = seedTime.AddDays(-10),
            BorrowDate = seedTime.AddDays(-9),
            ExpectedReturnDate = seedTime.AddDays(-2),
            Status = BorrowRequestStatus.Overdue,
            Purpose = "Extended lab testing",
            ApprovedById = staff.Id,
            ApprovedAt = seedTime.AddDays(-10).AddHours(1),
            CreatedAt = seedTime.AddDays(-10)
        };

        context.BorrowRequests.AddRange(pendingRequest, approvedRequest, completedRequest, rejectedRequest, overdueRequest);
        await context.SaveChangesAsync();

        context.BorrowRequestItems.AddRange(
            new BorrowRequestItem
            {
                BorrowRequestId = pendingRequest.Id,
                EquipmentId = equipments[3].Id,
                Quantity = 1,
                ConditionAtBorrow = EquipmentCondition.Good,
                CreatedAt = seedTime.AddDays(10)
            },
            new BorrowRequestItem
            {
                BorrowRequestId = approvedRequest.Id,
                EquipmentId = laptopBorrowed.Id,
                Quantity = 1,
                ConditionAtBorrow = EquipmentCondition.Good,
                CreatedAt = seedTime.AddDays(5)
            },
            new BorrowRequestItem
            {
                BorrowRequestId = completedRequest.Id,
                EquipmentId = equipments[5].Id,
                Quantity = 1,
                ConditionAtBorrow = EquipmentCondition.Good,
                ConditionAtReturn = EquipmentCondition.Good,
                CreatedAt = seedTime.AddDays(1)
            },
            new BorrowRequestItem
            {
                BorrowRequestId = rejectedRequest.Id,
                EquipmentId = equipments[8].Id,
                Quantity = 1,
                CreatedAt = seedTime.AddDays(7)
            },
            new BorrowRequestItem
            {
                BorrowRequestId = overdueRequest.Id,
                EquipmentId = projectorBorrowed.Id,
                Quantity = 1,
                ConditionAtBorrow = EquipmentCondition.Good,
                CreatedAt = seedTime.AddDays(-10)
            }
        );

        context.ReturnRecords.Add(new ReturnRecord
        {
            BorrowRequestId = completedRequest.Id,
            ReturnedAt = seedTime.AddDays(7),
            ReturnedById = staff.Id,
            StaffNote = "Returned in good condition",
            OverallCondition = EquipmentCondition.Good,
            CreatedAt = seedTime.AddDays(7)
        });

        context.Notifications.AddRange(
            new Notification
            {
                UserId = user.Id,
                Title = "Request approved",
                Message = "Your borrow request has been approved.",
                Type = NotificationType.RequestApproved,
                IsRead = true,
                CreatedAt = seedTime.AddDays(5)
            },
            new Notification
            {
                UserId = user.Id,
                Title = "Request rejected",
                Message = "Your borrow request was rejected.",
                Type = NotificationType.RequestRejected,
                IsRead = false,
                CreatedAt = seedTime.AddDays(7)
            },
            new Notification
            {
                UserId = user.Id,
                Title = "Overdue reminder",
                Message = "Please return borrowed equipment.",
                Type = NotificationType.RequestOverdue,
                IsRead = false,
                CreatedAt = seedTime.AddDays(-1)
            }
        );

        await context.SaveChangesAsync();
    }
}
