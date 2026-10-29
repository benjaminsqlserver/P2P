using Microsoft.EntityFrameworkCore;
using P2P.Application.Abstractions;
using P2P.Infrastructure.Persistence;

namespace P2P.Infrastructure.Services;

/// <summary>
/// Allocates gap-free document numbers using a single atomic UPDATE with
/// an UPDLOCK, which serialises concurrent callers on the row without a
/// long-held application-level lock.
/// </summary>
internal sealed class SqlNumberSequence(ApplicationDbContext context, IClock clock)
    : INumberSequence
{
    public async Task<string> NextAsync(
        string documentType, CancellationToken ct = default)
    {
        var year = clock.Today.Year;

        // ToListAsync, not SingleAsync: the SQL is not composable (DECLARE/MERGE),
        // and SingleAsync would make EF wrap it in an outer SELECT TOP(2).
        var values = await context.Database
            .SqlQuery<long>($"""
                DECLARE @next bigint;

                MERGE infrastructure.DocumentSequences WITH (HOLDLOCK) AS target
                USING (SELECT {documentType} AS DocumentType, {year} AS [Year]) AS source
                    ON  target.DocumentType = source.DocumentType
                    AND target.[Year]       = source.[Year]
                WHEN MATCHED THEN
                    UPDATE SET @next = target.NextValue,
                               target.NextValue = target.NextValue + 1
                WHEN NOT MATCHED THEN
                    INSERT (DocumentType, [Year], NextValue)
                    VALUES (source.DocumentType, source.[Year], 2);

                SELECT ISNULL(@next, 1) AS Value;
                """)
            .ToListAsync(ct);

        var next = values.Single();

        return $"{documentType}-{year}-{next:D6}";
    }
}
