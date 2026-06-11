using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace QuizProject.Web.Common.Middleware;

public static class SecurityHeadersMiddleware
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
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
    }
}
