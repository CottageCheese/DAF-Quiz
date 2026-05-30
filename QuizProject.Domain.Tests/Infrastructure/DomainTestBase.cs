using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using QuizProject.Domain.Data;

namespace QuizProject.Domain.Tests.Infrastructure;

public abstract class DomainTestBase : IDisposable
{
    private readonly SqliteConnection _connection;
    protected readonly IDistributedCache Cache;
    protected readonly ApplicationDbContext Db;

    protected DomainTestBase()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        Db = new ApplicationDbContext(options);
        Db.Database.EnsureCreated();

        Cache = new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions()));
    }

    public void Dispose()
    {
        Db.Dispose();
        _connection.Close();
        _connection.Dispose();
    }
}