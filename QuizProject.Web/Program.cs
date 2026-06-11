using QuizProject.Web.Common.Extensions;
using QuizProject.Web.Common.Middleware;
using QuizProject.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Shared web infrastructure (session, cookie auth, antiforgery, rate limiting, token storage)
builder.Services.AddWebCommonServices(
    builder.Configuration,
    builder.Environment,
    new WebCommonOptions
    {
        SessionCookieName = ".QuizProject.Session",
        AuthCookieName = ".QuizProject.Auth"
    });

// HTTP client
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"]
                 ?? throw new InvalidOperationException("ApiSettings:BaseUrl is not configured.");

builder.Services.AddHttpClient<IPublicApiClient, PublicApiClient>(
    client => { client.BaseAddress = new Uri(apiBaseUrl); });

// Register IAuthApiClient pointing to the same instance as IPublicApiClient
builder.Services.AddScoped<QuizProject.Web.Common.Services.IAuthApiClient>(
    sp => sp.GetRequiredService<IPublicApiClient>());

// MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Ensure SQL session cache table exists (production only)
await app.EnsureSessionCacheTableAsync();

// Security headers
app.UseSecurityHeaders();

// Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx => { ctx.Context.Response.Headers.CacheControl = "public, max-age=31536000"; }
});
app.UseRateLimiter();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Index}/{id?}");

app.Run();
