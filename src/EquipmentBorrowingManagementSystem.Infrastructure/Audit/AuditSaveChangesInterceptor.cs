using System.Security.Claims;
using System.Text.Json;
using EquipmentBorrowingManagementSystem.Domain.Entities;
using EquipmentBorrowingManagementSystem.Domain.Enums;
using EquipmentBorrowingManagementSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EquipmentBorrowingManagementSystem.Infrastructure.Audit;

public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private static readonly HashSet<string> ExcludedEntityNames =
    [
        nameof(AuditLog),
        nameof(RefreshToken)
    ];

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly List<PendingAuditEntry> _pendingEntries = [];

    public AuditSaveChangesInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        CapturePendingEntries(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        CapturePendingEntries(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        PersistAuditEntries(eventData.Context);
        return base.SavedChanges(eventData, result);
    }

    public override ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        PersistAuditEntries(eventData.Context);
        return base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private void CapturePendingEntries(DbContext? context)
    {
        _pendingEntries.Clear();

        if (context is not AppDbContext)
        {
            return;
        }

        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            var entityName = entry.Entity.GetType().Name;
            if (ExcludedEntityNames.Contains(entityName))
            {
                continue;
            }

            AuditAction? action = entry.State switch
            {
                EntityState.Added => AuditAction.Created,
                EntityState.Modified when IsSoftDelete(entry) => AuditAction.Deleted,
                EntityState.Modified => AuditAction.Updated,
                _ => null
            };

            if (action == null)
            {
                continue;
            }

            _pendingEntries.Add(new PendingAuditEntry
            {
                Entity = entry.Entity,
                EntityName = entityName,
                Action = action.Value,
                Changes = BuildChanges(entry, action.Value)
            });
        }
    }

    private void PersistAuditEntries(DbContext? context)
    {
        if (context is not AppDbContext dbContext || _pendingEntries.Count == 0)
        {
            _pendingEntries.Clear();
            return;
        }

        var principal = _httpContextAccessor.HttpContext?.User;
        int? userId = null;
        string? userEmail = null;

        if (principal?.Identity?.IsAuthenticated == true)
        {
            var idValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(idValue, out var parsedId))
            {
                userId = parsedId;
            }

            userEmail = principal.FindFirstValue(ClaimTypes.Email);
        }

        var now = DateTime.UtcNow;

        foreach (var pending in _pendingEntries)
        {
            dbContext.AuditLogs.Add(new AuditLog
            {
                UserId = userId,
                UserEmail = userEmail,
                EntityName = pending.EntityName,
                EntityId = pending.Entity.Id.ToString(),
                Action = pending.Action,
                Changes = pending.Changes,
                PerformedAt = now,
                CreatedAt = now
            });
        }

        _pendingEntries.Clear();
        dbContext.SaveChanges();
    }

    private static bool IsSoftDelete(EntityEntry<BaseEntity> entry)
    {
        var isDeletedProperty = entry.Property(nameof(BaseEntity.IsDeleted));
        return isDeletedProperty.IsModified &&
               isDeletedProperty.CurrentValue is true &&
               isDeletedProperty.OriginalValue is not true;
    }

    private static string? BuildChanges(EntityEntry entry, AuditAction action)
    {
        if (action == AuditAction.Created)
        {
            return null;
        }

        var changes = new Dictionary<string, object>();

        foreach (var property in entry.Properties.Where(p => p.IsModified))
        {
            if (property.Metadata.Name is nameof(BaseEntity.CreatedAt))
            {
                continue;
            }

            changes[property.Metadata.Name] = new
            {
                Old = property.OriginalValue,
                New = property.CurrentValue
            };
        }

        return changes.Count == 0
            ? null
            : JsonSerializer.Serialize(changes);
    }

    private sealed class PendingAuditEntry
    {
        public required BaseEntity Entity { get; init; }
        public required string EntityName { get; init; }
        public required AuditAction Action { get; init; }
        public string? Changes { get; init; }
    }
}
