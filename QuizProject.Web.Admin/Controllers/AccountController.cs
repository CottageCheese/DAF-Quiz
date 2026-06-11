using Microsoft.AspNetCore.Mvc;
using QuizProject.Web.Admin.Services;
using QuizProject.Web.Common.Controllers;
using QuizProject.Web.Common.Services;

namespace QuizProject.Web.Admin.Controllers;

/// <summary>Admin site login/logout — no self-registration (admins are seeded).</summary>
public class AccountController(IAdminApiClient apiClient, ITokenStorageService tokenStorage)
    : AccountControllerBase(apiClient, tokenStorage)
{
    protected override string DefaultController => "Admin";
    protected override string DefaultAction => "Index";
}
