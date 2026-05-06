using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QuizProject.Domain.Data;

namespace QuizProject.Tests.Integration.Infrastructure;

/// <summary>
/// Shared test host: SQL Server replaced with SQLite in-memory,
/// production SeedData bypassed, rate limiter disabled, test appsettings loaded.
/// </summary>
public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly SqliteConnection _connection =
        new($"Data Source=QuizTestDb_{Guid.NewGuid():N};Mode=Memory;Cache=Shared");

    public TestSeedContext Seed { get; private set; } = default!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Load test-specific appsettings (overrides production appsettings.json)
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddJsonFile("appsettings.Test.json", optional: false, reloadOnChange: false);
        });

        builder.ConfigureServices(services =>
        {
            // Replace SQL Server with SQLite
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<ApplicationDbContext>();

            services.AddSingleton(_connection);

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(_connection));
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        // Open connection — keeps the in-memory SQLite DB alive for the factory's lifetime
        await _connection.OpenAsync();

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // EnsureCreated derives schema from OnModelCreating (no SQL-Server-specific raw SQL)
        await db.Database.EnsureCreatedAsync();

        Seed = await TestDatabaseSeeder.SeedAsync(scope.ServiceProvider);
    }

    public new async Task DisposeAsync()
    {
        await _connection.CloseAsync();
        await _connection.DisposeAsync();
        await base.DisposeAsync();
    }
}
