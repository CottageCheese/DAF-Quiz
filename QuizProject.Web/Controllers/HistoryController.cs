using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizProject.Web.Services;

namespace QuizProject.Web.Controllers;

[Authorize]
public class HistoryController(IPublicApiClient apiClient) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? filter = null)
    {
        var history = await apiClient.GetMyHistoryAsync();
        var filtered = filter switch
        {
            "passed" => history.Where(a => a.Percentage >= 50).ToList(),
            "failed" => history.Where(a => a.Percentage < 50).ToList(),
            _ => history
        };
        ViewBag.Filter = filter ?? "all";
        ViewBag.TotalCount = history.Count;
        ViewBag.PassedCount = history.Count(a => a.Percentage >= 50);
        ViewBag.FailedCount = history.Count(a => a.Percentage < 50);
        return View(filtered);
    }
}
