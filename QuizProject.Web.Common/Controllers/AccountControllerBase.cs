using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using QuizProject.Web.Common.Models.ViewModels;
using QuizProject.Web.Common.Services;

namespace QuizProject.Web.Common.Controllers;

/// <summary>
/// Shared login/logout flow for all Web frontends.
/// Each site inherits and customizes redirects and available actions.
/// </summary>
public abstract class AccountControllerBase(
    IAuthApiClient apiClient,
    ITokenStorageService tokenStorage) : Controller
{
    /// <summary>Default controller to redirect to after login. Override per site.</summary>
    protected virtual string DefaultController => "Home";

    /// <summary>Default action to redirect to after login. Override per site.</summary>
    protected virtual string DefaultAction => "Index";

    [HttpGet]
    [AllowAnonymous]
    public virtual IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction(DefaultAction, DefaultController);

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("auth")]
    public virtual async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(model);

        var result = await apiClient.LoginAsync(model.Email, model.Password);
        if (!result.Succeeded || result.Data is null)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Invalid email or password.");
            return View(model);
        }

        await SignInFromTokensAsync(result.Data.AccessToken, result.Data.RefreshToken, result.Data.ExpiresIn);
        return RedirectToLocal(returnUrl);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public virtual async Task<IActionResult> Logout()
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
    public virtual IActionResult AccessDenied()
    {
        return View();
    }

    // Helpers

    protected async Task SignInFromTokensAsync(string accessToken, string refreshToken, int expiresIn)
    {
        // Read identity claims from the JWT without full validation
        // (the API already validated credentials and issued the token)
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(accessToken);

        var userId = jwt.Subject;
        var email = jwt.Claims.FirstOrDefault(c =>
            c.Type is JwtRegisteredClaimNames.Email or "email")?.Value ?? string.Empty;
        var displayName = jwt.Claims.FirstOrDefault(c => c.Type == "display_name")?.Value ?? email;

        // Extract role claims from the JWT so User.IsInRole() works in MVC
        var roleClaims = jwt.Claims
            .Where(c => c.Type is ClaimTypes.Role or "role" or
                "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
            .Select(c => new Claim(ClaimTypes.Role, c.Value));

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, displayName), // used by User.Identity.Name (navbar display)
            new(ClaimTypes.Email, email)
        };

        claims.AddRange(roleClaims);

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
            new AuthenticationProperties { IsPersistent = false });

        tokenStorage.StoreTokens(accessToken, refreshToken,
            DateTime.UtcNow.AddSeconds(expiresIn));
    }

    protected IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction(DefaultAction, DefaultController);
    }
}
