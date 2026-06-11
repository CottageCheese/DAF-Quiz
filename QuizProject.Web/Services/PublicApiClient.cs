using QuizProject.Contracts;
using QuizProject.Web.Common.Services;
using QuizProject.Web.Models.ViewModels;

namespace QuizProject.Web.Services;

public class PublicApiClient(
    HttpClient http,
    ITokenStorageService tokenStorage) : ApiClientBase(http, tokenStorage), IPublicApiClient
{
    // Quizzes

    public async Task<List<QuizListViewModel>> GetQuizzesAsync()
    {
        var response = await SendWithAuthAsync(HttpMethod.Get, "api/quizzes");
        if (response is null || !response.IsSuccessStatusCode) return [];
        return await response.Content.ReadFromJsonAsync<List<QuizListViewModel>>(JsonOpts) ?? [];
    }

    public async Task<TakeQuizViewModel?> StartAttemptAsync(int quizId)
    {
        var response = await SendWithAuthAsync(HttpMethod.Post, $"api/quizzes/{quizId}/start");
        if (response is null || !response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<TakeQuizViewModel>(JsonOpts);
    }

    public async Task<QuizResultViewModel?> SubmitAttemptAsync(int attemptId, List<QuestionAnswerSelection> selections)
    {
        var response = await SendWithAuthAsync(HttpMethod.Post,
            $"api/quizzes/attempts/{attemptId}/submit", selections);
        if (response is null || !response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<QuizResultViewModel>(JsonOpts);
    }

    public async Task<QuizResultViewModel?> GetResultAsync(int attemptId)
    {
        var response = await SendWithAuthAsync(HttpMethod.Get,
            $"api/quizzes/attempts/{attemptId}/result");
        if (response is null || !response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<QuizResultViewModel>(JsonOpts);
    }

    // Leaderboard (public — no auth needed)

    public async Task<LeaderboardViewModel> GetLeaderboardAsync()
    {
        var quizzesTask = Http.GetFromJsonAsync<List<TopQuizViewModel>>(
            "api/leaderboard/top-quizzes", JsonOpts);
        var usersTask = Http.GetFromJsonAsync<List<TopUserViewModel>>(
            "api/leaderboard/top-users", JsonOpts);

        await Task.WhenAll(quizzesTask, usersTask);

        return new LeaderboardViewModel
        {
            TopQuizzes = quizzesTask.Result ?? [],
            TopUsers = usersTask.Result ?? []
        };
    }
}
