using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using QuizProject.Api.Services;
using QuizProject.Domain.Data;
using QuizProject.Domain.Models.Domain;

namespace QuizProject.Api.Tests.Infrastructure;

public abstract class ApiTestBase : IDisposable
{
    private readonly SqliteConnection _connection;
    protected readonly ApplicationDbContext Db;
    protected readonly UserManager<ApplicationUser> UserManager;
    protected readonly TokenService TokenService;

    protected static readonly IOptions<JwtSettings> JwtOptions = Options.Create(new JwtSettings
    {
        Issuer = "test-issuer",
        Audience = "test-audience",
        SecretKey = "super-secret-key-for-testing-only-32chars!!",
        AccessTokenExpiryMinutes = 15,
        RefreshTokenExpiryDays = 7
    });

    protected ApiTestBase()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        Db = new ApplicationDbContext(options);
        Db.Database.EnsureCreated();

        var userStore = new UserStore<ApplicationUser>(Db);
        UserManager = new UserManager<ApplicationUser>(
            userStore,
            null,
            new PasswordHasher<ApplicationUser>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            new UpperInvariantLookupNormalizer(),
            new IdentityErrorDescriber(),
            null,
            NullLogger<UserManager<ApplicationUser>>.Instance);

        TokenService = new TokenService(JwtOptions, UserManager, Db, NullLogger<TokenService>.Instance);
    }

    protected async Task<ApplicationUser> CreateUserAsync(string email = "test@test.local", string role = "")
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            DisplayName = "Test User",
            SecurityStamp = Guid.NewGuid().ToString()
        };
        await UserManager.CreateAsync(user);
        if (!string.IsNullOrEmpty(role))
            await UserManager.AddToRoleAsync(user, role);
        return user;
    }

    public void Dispose()
    {
        UserManager.Dispose();
        Db.Dispose();
        _connection.Close();
        _connection.Dispose();
    }
}
