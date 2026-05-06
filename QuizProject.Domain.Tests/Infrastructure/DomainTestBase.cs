using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QuizProject.Domain.Data;
using QuizProject.Domain.Models.Domain;
using QuizProject.Domain.Repositories;

namespace QuizProject.Domain.Tests.Infrastructure;

public abstract class DomainTestBase : IDisposable
{
    private readonly SqliteConnection _connection;
    protected readonly ApplicationDbContext Db;
    protected readonly IRepository<Quiz> QuizRepo;
    protected readonly IRepository<Question> QuestionRepo;
    protected readonly IRepository<Answer> AnswerRepo;
    protected readonly IRepository<QuizAttempt> AttemptRepo;
    protected readonly IRepository<QuizAttemptAnswer> AttemptAnswerRepo;
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

        QuizRepo = new Repository<Quiz>(Db);
        QuestionRepo = new Repository<Question>(Db);
        AnswerRepo = new Repository<Answer>(Db);
        AttemptRepo = new Repository<QuizAttempt>(Db);
        AttemptAnswerRepo = new Repository<QuizAttemptAnswer>(Db);
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
