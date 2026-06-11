using QuizProject.Contracts;
using QuizProject.Web.Common.Services;

namespace QuizProject.Web.Admin.Services;

public class AdminApiClient(
    HttpClient http,
    ITokenStorageService tokenStorage) : ApiClientBase(http, tokenStorage), IAdminApiClient
{
    public async Task<PagedResult<AdminQuizListViewModel>> GetAdminQuizzesAsync(int page = 1, int pageSize = 20)
    {
        var response = await SendWithAuthAsync(HttpMethod.Get, $"api/admin/quizzes?page={page}&pageSize={pageSize}");
        if (response is null || !response.IsSuccessStatusCode) return new PagedResult<AdminQuizListViewModel>();
        return await response.Content.ReadFromJsonAsync<PagedResult<AdminQuizListViewModel>>(JsonOpts)
               ?? new PagedResult<AdminQuizListViewModel>();
    }

    public async Task<AdminQuizDetailViewModel?> GetAdminQuizAsync(int id)
    {
        var response = await SendWithAuthAsync(HttpMethod.Get, $"api/admin/quizzes/{id}");
        if (response is null || !response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<AdminQuizDetailViewModel>(JsonOpts);
    }

    public async Task<AdminQuizDetailViewModel?> CreateQuizAsync(string title, string? description)
    {
        var response = await SendWithAuthAsync(HttpMethod.Post, "api/admin/quizzes",
            new { title, description });
        if (response is null || !response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<AdminQuizDetailViewModel>(JsonOpts);
    }

    public async Task<AdminQuizDetailViewModel?> UpdateQuizAsync(
        int id, string title, string? description, DateTime? publishedAt)
    {
        var response = await SendWithAuthAsync(HttpMethod.Put, $"api/admin/quizzes/{id}",
            new { title, description, publishedAt });
        if (response is null || !response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<AdminQuizDetailViewModel>(JsonOpts);
    }

    public async Task<bool> DeleteQuizAsync(int id)
    {
        var response = await SendWithAuthAsync(HttpMethod.Delete, $"api/admin/quizzes/{id}");
        return response?.IsSuccessStatusCode == true;
    }

    public async Task<AdminQuestionViewModel?> AddQuestionAsync(
        int quizId, string text, int displayOrder, List<(string Text, bool IsCorrect)> answers)
    {
        var response = await SendWithAuthAsync(HttpMethod.Post, $"api/admin/quizzes/{quizId}/questions",
            new
            {
                text,
                displayOrder,
                answers = answers.Select(a => new { text = a.Text, isCorrect = a.IsCorrect }).ToList()
            });
        if (response is null || !response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<AdminQuestionViewModel>(JsonOpts);
    }

    public async Task<AdminQuestionViewModel?> UpdateQuestionAsync(
        int quizId, int questionId, string text, int displayOrder, List<(string Text, bool IsCorrect)> answers)
    {
        var response = await SendWithAuthAsync(HttpMethod.Put,
            $"api/admin/quizzes/{quizId}/questions/{questionId}",
            new
            {
                text,
                displayOrder,
                answers = answers.Select(a => new { text = a.Text, isCorrect = a.IsCorrect }).ToList()
            });
        if (response is null || !response.IsSuccessStatusCode) return null;
        return await response.Content.ReadFromJsonAsync<AdminQuestionViewModel>(JsonOpts);
    }

    public async Task<bool> DeleteQuestionAsync(int quizId, int questionId)
    {
        var response = await SendWithAuthAsync(HttpMethod.Delete,
            $"api/admin/quizzes/{quizId}/questions/{questionId}");
        return response?.IsSuccessStatusCode == true;
    }
}
