using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QuizProject.Domain.Data;

namespace QuizProject.Domain.Tests.Infrastructure;

public abstract class DomainTestBase : IDisposable
{
    private readonly SqliteConnection _connection;
    protected readonly ApplicationDbContext Db;
    protected readonly IMemoryCache Cache;

    protected DomainTestBase()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        Db = new ApplicationDbContext(options);
        Db.Database.EnsureCreated();

        Cache = new MemoryCache(new MemoryCacheOptions());
    }

    public void Dispose()
    {
        Db.Dispose();
        Cache.Dispose();
        _connection.Close();
        _connection.Dispose();
    }
}
