using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using QuizProject.Web.Common.Controllers;
using QuizProject.Web.Common.Services;
using QuizProject.Web.Models.ViewModels;
using QuizProject.Web.Services;

namespace QuizProject.Web.Controllers;

public class AccountController(IPublicApiClient apiClient, ITokenStorageService tokenStorage)
    : AccountControllerBase(apiClient, tokenStorage)
{
    protected override string DefaultController => "Home";
    protected override string DefaultAction => "Index";

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

        var result = await apiClient.RegisterAsync(model.Email, model.Password, model.DisplayName);
        if (!result.Succeeded || result.Data is null)
        {
            ModelState.AddModelError(string.Empty,
                result.ErrorMessage ??
                "Registration failed. The email may already be in use or the password does not meet requirements.");
            return View(model);
        }

        await SignInFromTokensAsync(result.Data.AccessToken, result.Data.RefreshToken, result.Data.ExpiresIn);
        return RedirectToLocal(returnUrl);
    }
}
