using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using P2P.Application.Abstractions;

namespace P2P.Infrastructure.Persistence.Interceptors;

public sealed class AuditInterceptor(ICurrentUser currentUser, IClock clock)
    : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            WriteAuditEntries(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void WriteAuditEntries(DbContext context)
    {
        context.ChangeTracker.DetectChanges();

        var auditable = context.ChangeTracker.Entries()
            .Where(e => e.Entity is not AuditEntry
                        && e.State is EntityState.Added
                                   or EntityState.Modified
                                   or EntityState.Deleted)
            .ToArray();

        foreach (var entry in auditable)
        {
            context.Set<AuditEntry>().Add(new AuditEntry
            {
                TableName = entry.Metadata.GetTableName() ?? entry.Metadata.Name,
                EntityId = PrimaryKeyOf(entry),
                Action = entry.State.ToString(),
                UserId = currentUser.UserId,
                OccurredAtUtc = clock.UtcNow,
                OldValues = entry.State == EntityState.Added
                    ? null
                    : Serialise(entry, original: true),
                NewValues = entry.State == EntityState.Deleted
                    ? null
                    : Serialise(entry, original: false),
                ChangedColumns = entry.State == EntityState.Modified
                    ? string.Join(",", entry.Properties
                        .Where(p => p.IsModified)
                        .Select(p => p.Metadata.Name))
                    : null
            });
        }
    }

    private static string PrimaryKeyOf(EntityEntry entry) =>
        string.Join("|", entry.Properties
            .Where(p => p.Metadata.IsPrimaryKey())
            .Select(p => p.CurrentValue?.ToString() ?? string.Empty));

    private static string Serialise(EntityEntry entry, bool original)
    {
        var values = entry.Properties
            .Where(p => !p.Metadata.IsPrimaryKey())
            .ToDictionary(
                p => p.Metadata.Name,
                p => original ? p.OriginalValue : p.CurrentValue);

        return JsonSerializer.Serialize(values);
    }
}
