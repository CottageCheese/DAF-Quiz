using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using QuizProject.Web.Models.ViewModels;
using QuizProject.Web.Services;

namespace QuizProject.Web.Controllers;

public class AccountController(IApiClient apiClient, ITokenStorageService tokenStorage) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(model);

        var tokens = await apiClient.RegisterAsync(model.Email, model.Password);
        if (tokens is null)
        {
            ModelState.AddModelError(string.Empty,
                "Registration failed. The email may already be in use or the password does not meet requirements.");
            return View(model);
        }

        await SignInFromTokensAsync(tokens.AccessToken, tokens.RefreshToken, tokens.ExpiresIn);
        return RedirectToLocal(returnUrl);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(model);

        var tokens = await apiClient.LoginAsync(model.Email, model.Password);
        if (tokens is null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        await SignInFromTokensAsync(tokens.AccessToken, tokens.RefreshToken, tokens.ExpiresIn);
        return RedirectToLocal(returnUrl);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var refreshToken = tokenStorage.GetRefreshToken();
        if (refreshToken is not null)
            await apiClient.RevokeTokenAsync(refreshToken);

        tokenStorage.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Account");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied() => View();

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task SignInFromTokensAsync(string accessToken, string refreshToken, int expiresIn)
    {
        // Read identity claims from the JWT without full validation
        // (the API already validated credentials and issued the token)
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(accessToken);

        var userId = jwt.Subject;
        var email = jwt.Claims.FirstOrDefault(c =>
            c.Type is JwtRegisteredClaimNames.Email or "email")?.Value ?? string.Empty;

        // Extract role claims from the JWT so User.IsInRole() works in MVC
        var roleClaims = jwt.Claims
            .Where(c => c.Type is ClaimTypes.Role or "role" or
                "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
            .Select(c => new Claim(ClaimTypes.Role, c.Value));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, email),
            new(ClaimTypes.Email, email),
        };

        claims.AddRange(roleClaims);

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = false });

        tokenStorage.StoreTokens(accessToken, refreshToken,
            DateTime.UtcNow.AddSeconds(expiresIn));
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }
}
