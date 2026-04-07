using Microsoft.AspNetCore.Mvc;
using QuizProject.Web.Services;

namespace QuizProject.Web.Controllers;

public class HomeController(IApiClient apiClient) : Controller
{
    public async Task<IActionResult> Index()
    {
        var model = await apiClient.GetLeaderboardAsync();
        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
    }
}