using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuizProject.Web.Common.Services;

/// <summary>
/// Base HTTP client with Bearer token management and transparent refresh.
/// Each Web frontend derives from this and adds its own domain-specific methods.
/// </summary>
public abstract class ApiClientBase : IAuthApiClient
{
    protected readonly HttpClient Http;
    private readonly ITokenStorageService _tokenStorage;

    // Prevents concurrent refresh calls from the same session racing each other
    private static readonly SemaphoreSlim RefreshLock = new(1, 1);

    protected static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    protected ApiClientBase(HttpClient http, ITokenStorageService tokenStorage)
    {
        Http = http;
        _tokenStorage = tokenStorage;
    }

    // Auth

    public async Task<ApiResult<AuthTokens>> LoginAsync(string email, string password)
    {
        var response = await Http.PostAsJsonAsync("api/auth/login",
            new { email, password });

        if (!response.IsSuccessStatusCode)
        {
            var error = await GetErrorMessage(response);
            return new ApiResult<AuthTokens>(null, false, error ?? "Invalid email or password.");
        }

        var tokens = await ReadAuthResponse(response);
        return new ApiResult<AuthTokens>(tokens, tokens is not null,
            tokens is null ? "Failed to read auth response." : null);
    }

    public async Task<ApiResult<AuthTokens>> RegisterAsync(string email, string password, string displayName)
    {
        var response = await Http.PostAsJsonAsync("api/auth/register",
            new { email, password, displayName });

        if (!response.IsSuccessStatusCode)
        {
            var error = await GetErrorMessage(response);
            return new ApiResult<AuthTokens>(null, false, error ?? "Registration failed.");
        }

        var tokens = await ReadAuthResponse(response);
        return new ApiResult<AuthTokens>(tokens, tokens is not null,
            tokens is null ? "Failed to read auth response." : null);
    }

    public async Task RevokeTokenAsync(string refreshToken)
    {
        var accessToken = _tokenStorage.GetAccessToken();
        var request = new HttpRequestMessage(HttpMethod.Post, "api/auth/revoke");
        request.Content = JsonContent.Create(new { refreshToken });
        if (accessToken is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        await Http.SendAsync(request);
    }

    // Core send with Bearer + 401 retry

    protected async Task<HttpResponseMessage?> SendWithAuthAsync(
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

        var token = _tokenStorage.GetAccessToken();
        if (token is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await Http.SendAsync(request);
    }

    private async Task ProactiveRefreshIfNeededAsync()
    {
        var expiry = _tokenStorage.GetAccessTokenExpiry();
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
            var expiry = _tokenStorage.GetAccessTokenExpiry();
            if (expiry is not null && expiry.Value > DateTime.UtcNow.AddSeconds(60))
                return true;

            var refreshToken = _tokenStorage.GetRefreshToken();
            if (string.IsNullOrEmpty(refreshToken)) return false;

            // Direct call — does NOT go through SendWithAuthAsync to avoid recursion
            var response = await Http.PostAsJsonAsync("api/auth/refresh",
                new { refreshToken });

            if (!response.IsSuccessStatusCode) return false;

            var tokens = await ReadAuthResponse(response);
            if (tokens is null) return false;

            _tokenStorage.StoreTokens(
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

    protected static async Task<AuthTokens?> ReadAuthResponse(HttpResponseMessage response)
    {
        var doc = await JsonSerializer.DeserializeAsync<JsonElement>(
            await response.Content.ReadAsStreamAsync(), JsonOpts);

        var access = doc.GetProperty("accessToken").GetString();
        var refresh = doc.GetProperty("refreshToken").GetString();
        var expiresIn = doc.GetProperty("expiresIn").GetInt32();

        if (access is null || refresh is null) return null;
        return new AuthTokens(access, refresh, expiresIn);
    }

    protected static async Task<string?> GetErrorMessage(HttpResponseMessage response)
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
        catch
        {
            /* ignored */
        }

        return null;
    }
}
