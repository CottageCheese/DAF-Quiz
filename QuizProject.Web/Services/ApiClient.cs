using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using QuizProject.Contracts;
using QuizProject.Web.Models.ViewModels;

namespace QuizProject.Web.Services;

public class ApiClient(
    HttpClient http,
    ITokenStorageService tokenStorage) : IApiClient
{
    // Prevents concurrent refresh calls from the same session racing each other
    private static readonly SemaphoreSlim RefreshLock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    // Auth

    public async Task<ApiResult<AuthTokens>> LoginAsync(string email, string password)
    {
        var response = await http.PostAsJsonAsync("api/auth/login",
            new { email, password });

        if (!response.IsSuccessStatusCode)
        {
            var error = await GetErrorMessage(response);
            return new ApiResult<AuthTokens>(null, false, error ?? "Invalid email or password.");
        }

        var tokens = await ReadAuthResponse(response);
        return new ApiResult<AuthTokens>(tokens, tokens is not null, tokens is null ? "Failed to read auth response." : null);
    }

    public async Task<ApiResult<AuthTokens>> RegisterAsync(string email, string password, string displayName)
    {
        var response = await http.PostAsJsonAsync("api/auth/register",
            new { email, password, displayName });

        if (!response.IsSuccessStatusCode)
        {
            var error = await GetErrorMessage(response);
            return new ApiResult<AuthTokens>(null, false, error ?? "Registration failed.");
        }

        var tokens = await ReadAuthResponse(response);
        return new ApiResult<AuthTokens>(tokens, tokens is not null, tokens is null ? "Failed to read auth response." : null);
    }

    public async Task RevokeTokenAsync(string refreshToken)
    {
        var accessToken = tokenStorage.GetAccessToken();
        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/revoke");
        request.Content = JsonContent.Create(new { refreshToken });
        if (accessToken is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        await http.SendAsync(request);
    }

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

    // Admin

    public async Task<List<AdminQuizListViewModel>> GetAdminQuizzesAsync()
    {
        var response = await SendWithAuthAsync(HttpMethod.Get, "api/admin/quizzes");
        if (response is null || !response.IsSuccessStatusCode) return [];
        return await response.Content.ReadFromJsonAsync<List<AdminQuizListViewModel>>(JsonOpts) ?? [];
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

    // Leaderboard

    public async Task<LeaderboardViewModel> GetLeaderboardAsync()
    {
        // Leaderboard is public — no auth needed
        var quizzesTask = http.GetFromJsonAsync<List<TopQuizViewModel>>(
            "api/leaderboard/top-quizzes", JsonOpts);
        var usersTask = http.GetFromJsonAsync<List<TopUserViewModel>>(
            "api/leaderboard/top-users", JsonOpts);

        await Task.WhenAll(quizzesTask, usersTask);

        return new LeaderboardViewModel
        {
            TopQuizzes = quizzesTask.Result ?? [],
            TopUsers = usersTask.Result ?? []
        };
    }

    // Core send with Bearer + 401 retry

    private async Task<HttpResponseMessage?> SendWithAuthAsync(
        HttpMethod method, string url, object? body = null)
    {
        // Proactive: refresh if token expires within 60 seconds
        await ProactiveRefreshIfNeededAsync();

        var response = await BuildAndSend(method, url, body);
        if (response.StatusCode != HttpStatusCode.Unauthorized)
            return response;

        // Reactive: refresh once and retry
        var refreshed = await TryRefreshAsync();
        if (!refreshed) return response; // caller will get 401

        return await BuildAndSend(method, url, body);
    }

    private async Task<HttpResponseMessage> BuildAndSend(HttpMethod method, string url, object? body)
    {
        var request = new HttpRequestMessage(method, url);
        if (body is not null)
            request.Content = JsonContent.Create(body);

        var token = tokenStorage.GetAccessToken();
        if (token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await http.SendAsync(request);
    }

    private async Task ProactiveRefreshIfNeededAsync()
    {
        var expiry = tokenStorage.GetAccessTokenExpiry();
        if (expiry is null || expiry.Value > DateTime.UtcNow.AddSeconds(60))
            return;

        await TryRefreshAsync();
    }

    private async Task<bool> TryRefreshAsync()
    {
        await RefreshLock.WaitAsync();
        try
        {
            // Re-check after acquiring lock — another thread may have already refreshed
            var expiry = tokenStorage.GetAccessTokenExpiry();
            if (expiry is not null && expiry.Value > DateTime.UtcNow.AddSeconds(60))
                return true;

            var refreshToken = tokenStorage.GetRefreshToken();
            if (string.IsNullOrEmpty(refreshToken)) return false;

            // Direct call — does NOT go through SendWithAuthAsync to avoid recursion
            var response = await http.PostAsJsonAsync("api/auth/refresh",
                new { refreshToken });

            if (!response.IsSuccessStatusCode) return false;

            var tokens = await ReadAuthResponse(response);
            if (tokens is null) return false;

            tokenStorage.StoreTokens(
                tokens.AccessToken,
                tokens.RefreshToken,
                DateTime.UtcNow.AddSeconds(tokens.ExpiresIn));

            return true;
        }
        finally
        {
            RefreshLock.Release();
        }
    }

    private static async Task<AuthTokens?> ReadAuthResponse(HttpResponseMessage response)
    {
        var doc = await JsonSerializer.DeserializeAsync<JsonElement>(
            await response.Content.ReadAsStreamAsync(), JsonOpts);

        var access = doc.GetProperty("accessToken").GetString();
        var refresh = doc.GetProperty("refreshToken").GetString();
        var expiresIn = doc.GetProperty("expiresIn").GetInt32();

        if (access is null || refresh is null) return null;
        return new AuthTokens(access, refresh, expiresIn);
    }

    private static async Task<string?> GetErrorMessage(HttpResponseMessage response)
    {
        try
        {
            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("message", out var messageProp))
                return messageProp.GetString();
            if (doc.RootElement.TryGetProperty("errors", out var errorsProp))
            {
                if (errorsProp.ValueKind == JsonValueKind.Array)
                    return string.Join(" ", errorsProp.EnumerateArray().Select(e => e.GetString()));
                return errorsProp.GetString();
            }
        }
        catch { /* ignored */ }
        return null;
    }
}
