using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizProject.Contracts;
using QuizProject.Web.Services;

namespace QuizProject.Web.Controllers;

[Authorize]
public class QuizController(IApiClient apiClient) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var quizzes = await apiClient.GetQuizzesAsync();
        return View(quizzes);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(int quizId)
    {
        var model = await apiClient.StartAttemptAsync(quizId);
        if (model is null)
        {
            TempData["Error"] = "Quiz not found or is no longer active.";
            return RedirectToAction(nameof(Index));
        }

        return View("Take", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(SubmitQuizViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Please answer all questions before submitting.";
            return RedirectToAction(nameof(Index));
        }

        var result = await apiClient.SubmitAttemptAsync(model.AttemptId, model.Selections);
        if (result is null)
        {
            TempData["Error"] = "Unable to submit quiz. It may have already been completed.";
            return RedirectToAction(nameof(Index));
        }

        return View("Results", result);
    }

    [HttpGet]
    public async Task<IActionResult> Results(int attemptId)
    {
        var result = await apiClient.GetResultAsync(attemptId);
        if (result is null) return NotFound();

        return View(result);
    }
}