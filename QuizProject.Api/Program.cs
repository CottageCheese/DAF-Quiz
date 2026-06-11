using System.Text;
using System.Threading.RateLimiting;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using QuizProject.Api.Infrastructure;
using QuizProject.Api.Messaging;
using QuizProject.Api.Services;
using QuizProject.Contracts;
using QuizProject.Domain.Data;
using QuizProject.Domain.Models.Domain;
using QuizProject.Domain.Services;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        x => x.MigrationsAssembly("QuizProject.Domain")));

// Identity
builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = false;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddSignInManager();

// JWT Settings + Bearer
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var s = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = s.Issuer,
            ValidAudience = s.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(s.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };
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

// CORS (Web frontend only)
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                     ?? ["https://localhost:5001", "http://localhost:5000", "https://localhost:5003", "http://localhost:5002"];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
            .WithMethods("GET", "POST", "PUT", "DELETE")
            .WithHeaders("Content-Type", "Authorization"));
});

// Application services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<ILeaderboardService, LeaderboardService>();
builder.Services.AddScoped<IAdminQuizService, AdminQuizService>();

// Caching
var cacheMode = builder.Configuration["Cache:Mode"] ?? "None";
switch (cacheMode.ToLowerInvariant())
{
    case "redis":
        var redisConnection = builder.Configuration.GetConnectionString("Redis")
                              ?? throw new InvalidOperationException(
                                  "Cache:Mode=Redis requires ConnectionStrings:Redis");
        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnection;
            options.InstanceName = "QuizProject:";
        });
        break;
    case "memory":
        builder.Services.AddDistributedMemoryCache();
        break;
    default: // "None"
        builder.Services.AddSingleton<IDistributedCache, NullDistributedCache>();
        break;
}

// Service Bus
var serviceBusConnection = builder.Configuration.GetConnectionString("ServiceBus");
if (!string.IsNullOrWhiteSpace(serviceBusConnection))
{
    builder.Services.AddSingleton(new ServiceBusClient(serviceBusConnection));
    builder.Services.AddSingleton<IQuizEventPublisher, ServiceBusQuizEventPublisher>();
    builder.Services.AddHostedService<LeaderboardInvalidationConsumer>();
    builder.Services.AddHostedService<QuizResultNotificationConsumer>();
}
else
{
    builder.Services.AddSingleton<IQuizEventPublisher, NullQuizEventPublisher>();
}

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

// Problem Details (RFC 7807)
builder.Services.AddProblemDetails();

// Controllers
builder.Services.AddControllers(o => o.Filters.Add<DomainValidationExceptionFilter>());

var app = builder.Build();

// Fail fast if JWT secret is still a placeholder
if (!app.Environment.IsEnvironment("Testing"))
{
    var jwtKey = app.Configuration["JwtSettings:SecretKey"] ?? "";
    if (jwtKey.Contains("REPLACE", StringComparison.OrdinalIgnoreCase) || jwtKey.Length < 32)
        throw new InvalidOperationException(
            "JwtSettings:SecretKey must be a real secret (>=32 chars). "
            + "Set it via User Secrets or environment variable JwtSettings__SecretKey.");
}

// Database migration + seed
// Integration tests use a separate SQLite seed path in CustomWebApplicationFactory.
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    await SeedData.InitialiseAsync(scope.ServiceProvider);
}

// Security headers
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program
{
}