using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuizProject.Web.Common.Services;

namespace QuizProject.Web.Common.Extensions;

public static class WebCommonServiceExtensions
{
    /// <summary>
    /// Registers session, cookie auth, antiforgery, rate limiting, and token storage
    /// shared by all Web frontends.
    /// </summary>
    public static IServiceCollection AddWebCommonServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment,
        WebCommonOptions? options = null)
    {
        options ??= new WebCommonOptions();

        // Session (server-side JWT token storage)
        var sessionConnection = configuration.GetConnectionString("SessionConnection")
                                ?? configuration.GetConnectionString("DefaultConnection");

        if (environment.IsDevelopment() || sessionConnection is null)
            services.AddDistributedMemoryCache();
        else
            services.AddDistributedSqlServerCache(o =>
            {
                o.ConnectionString = sessionConnection;
                o.SchemaName = "dbo";
                o.TableName = "SessionCache";
            });

        services.AddSession(o =>
        {
            o.Cookie.HttpOnly = true;
            o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            o.Cookie.SameSite = SameSiteMode.Strict;
            o.Cookie.Name = options.SessionCookieName;
            o.IdleTimeout = TimeSpan.FromHours(2);
        });

        // Cookie authentication (no Identity/EF — principal comes from JWT claims)
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(o =>
            {
                o.Cookie.HttpOnly = true;
                o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                o.Cookie.SameSite = SameSiteMode.Strict;
                o.Cookie.Name = options.AuthCookieName;
                o.ExpireTimeSpan = TimeSpan.FromHours(2);
                o.SlidingExpiration = true;
                o.LoginPath = "/Account/Login";
                o.LogoutPath = "/Account/Logout";
                o.AccessDeniedPath = "/Account/AccessDenied";

                // If the session no longer has tokens the cookie principal is stale — sign out
                o.Events.OnValidatePrincipal = async ctx =>
                {
                    var storage = ctx.HttpContext.RequestServices
                        .GetRequiredService<ITokenStorageService>();

                    if (storage.GetAccessToken() is null)
                    {
                        ctx.RejectPrincipal();
                        await ctx.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    }
                };
            });

        // Anti-forgery
        services.AddAntiforgery(o =>
        {
            o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            o.Cookie.SameSite = SameSiteMode.Strict;
            o.Cookie.HttpOnly = true;
        });

        // Rate limiting
        services.AddRateLimiter(o =>
        {
            o.AddFixedWindowLimiter("auth", limiterOptions =>
            {
                limiterOptions.PermitLimit = 10;
                limiterOptions.Window = TimeSpan.FromMinutes(1);
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                limiterOptions.QueueLimit = 0;
            });
            o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        // Token storage + HTTP context
        services.AddScoped<ITokenStorageService, TokenStorageService>();
        services.AddHttpContextAccessor();

        return services;
    }

    /// <summary>
    /// Ensures the SQL Server session cache table exists (production only).
    /// Call after building the app but before running.
    /// </summary>
    public static async Task EnsureSessionCacheTableAsync(
        this WebApplication app)
    {
        if (app.Environment.IsDevelopment()) return;

        var sessionConnection = app.Configuration.GetConnectionString("SessionConnection")
                                ?? app.Configuration.GetConnectionString("DefaultConnection");

        if (sessionConnection is null) return;

        using var conn = new SqlConnection(sessionConnection);
        await conn.OpenAsync();
        await new SqlCommand("""
                             IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'SessionCache' AND schema_id = SCHEMA_ID('dbo'))
                             BEGIN
                                 CREATE TABLE [dbo].[SessionCache] (
                                     [Id]                         NVARCHAR(449)       NOT NULL,
                                     [Value]                      VARBINARY(MAX)      NOT NULL,
                                     [ExpiresAtTime]              DATETIMEOFFSET(7)   NOT NULL,
                                     [SlidingExpirationInSeconds] BIGINT              NULL,
                                     [AbsoluteExpiration]         DATETIMEOFFSET(7)   NULL,
                                     CONSTRAINT [pk_Id] PRIMARY KEY ([Id] ASC)
                                 );
                                 CREATE NONCLUSTERED INDEX [Index_ExpiresAtTime] ON [dbo].[SessionCache] ([ExpiresAtTime] ASC);
                             END
                             """, conn).ExecuteNonQueryAsync();
    }
}
