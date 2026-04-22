using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.RateLimiting;
using QuizProject.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Session (server-side JWT token storage)
var sessionConnection = builder.Configuration.GetConnectionString("SessionConnection")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (builder.Environment.IsDevelopment() || sessionConnection is null)
{
    builder.Services.AddDistributedMemoryCache();
}
else
{
    builder.Services.AddDistributedSqlServerCache(options =>
    {
        options.ConnectionString = sessionConnection;
        options.SchemaName = "dbo";
        options.TableName = "SessionCache";
    });
}
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.Name = ".QuizProject.Session";
    options.IdleTimeout = TimeSpan.FromHours(2);
});

// Cookie authentication (no Identity/EF — principal comes from JWT claims)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.Name = ".QuizProject.Auth";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";

        // If the session no longer has tokens the cookie principal is stale — sign out
        options.Events.OnValidatePrincipal = async ctx =>
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
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.HttpOnly = true;
});

// Rate limiting
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("auth", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10;
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// HTTP client + services
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
    ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");

builder.Services.AddHttpClient<IApiClient, ApiClient>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

builder.Services.AddScoped<ITokenStorageService, TokenStorageService>();
builder.Services.AddHttpContextAccessor();

// MVC
builder.Services.AddControllersWithViews();

// Build
var app = builder.Build();

// Ensure SQL session cache table exists (production only)
if (!app.Environment.IsDevelopment() && sessionConnection is not null)
{
    using var conn = new Microsoft.Data.SqlClient.SqlConnection(sessionConnection);
    await conn.OpenAsync();
    await new Microsoft.Data.SqlClient.SqlCommand("""
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

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
    context.Response.Headers["Content-Security-Policy"] =
        "default-src 'self'; " +
        "script-src 'self' https://cdn.jsdelivr.net; " +
        "style-src 'self' https://cdn.jsdelivr.net; " +
        "font-src 'self' https://cdn.jsdelivr.net; " +
        "img-src 'self' data:; " +
        "frame-ancestors 'none';";
    await next();
});

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRateLimiter();
app.UseRouting();
app.UseSession(); // Must be before UseAuthentication
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Index}/{id?}");

app.Run();
