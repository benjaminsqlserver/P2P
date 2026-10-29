using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using P2P.Infrastructure.Persistence;

namespace P2P.Application.Tests;

/// <summary>
/// An in-memory SQLite database with the real EF Core model applied.
/// Preferable to the InMemory provider, which is not a relational database
/// and will happily accept queries SQL Server rejects.
/// </summary>
public sealed class TestDatabase : IDisposable
{
    private readonly SqliteConnection _connection;

    public TestDatabase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new ApplicationDbContext(options);
        Context.Database.EnsureCreated();
    }

    public ApplicationDbContext Context { get; }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
