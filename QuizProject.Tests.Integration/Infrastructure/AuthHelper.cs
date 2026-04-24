using System.Collections.Concurrent;
using System.Net.Http.Json;
using QuizProject.Api.Controllers;

namespace QuizProject.Tests.Integration.Infrastructure;

/// <summary>
/// Calls the real POST /api/auth/login endpoint and caches the AuthResponse
/// per (email, password) to avoid repeated roundtrips within the same factory lifetime.
/// </summary>
public sealed class AuthHelper
{
    private readonly HttpClient _client;
    private readonly ConcurrentDictionary<string, AuthResponse> _cache = new();

    public AuthHelper(HttpClient client)
    {
        _client = client;
    }

    public async Task<AuthResponse> LoginAsync(string email, string password)
    {
        var key = $"{email}:{password}";
        if (_cache.TryGetValue(key, out var cached))
            return cached;

        var response = await _client.PostAsJsonAsync("/api/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>()
            ?? throw new InvalidOperationException("Login returned null body.");

        _cache[key] = auth;
        return auth;
    }
}
