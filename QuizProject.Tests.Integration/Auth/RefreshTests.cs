using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using QuizProject.Api.Controllers;
using QuizProject.Tests.Integration.Infrastructure;

namespace QuizProject.Tests.Integration.Auth;

[Collection("AuthTests")]
public class RefreshTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Refresh_ValidToken_Returns200WithNewTokens()
    {
        // Register a fresh user so we get a clean token pair
        var regResponse = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "refresh1@test.local",
            password = "Refresh@123!",
            displayName = "RefreshUser1"
        });
        regResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var initial = await regResponse.Content.ReadFromJsonAsync<AuthResponse>();

        var response = await Client.PostAsJsonAsync("/api/auth/refresh", new
        {
            refreshToken = initial!.RefreshToken
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var refreshed = await response.Content.ReadFromJsonAsync<AuthResponse>();
        refreshed!.AccessToken.Should().NotBeNullOrWhiteSpace();
        refreshed.RefreshToken.Should().NotBeNullOrWhiteSpace();
        // Tokens should rotate — new refresh token differs from old
        refreshed.RefreshToken.Should().NotBe(initial.RefreshToken);
    }

    [Fact]
    public async Task Refresh_TokenRotation_OldTokenInvalidated()
    {
        var regResponse = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "refresh2@test.local",
            password = "Refresh@123!",
            displayName = "RefreshUser2"
        });
        var initial = await regResponse.Content.ReadFromJsonAsync<AuthResponse>();

        // Rotate once
        await Client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = initial!.RefreshToken });

        // Use original token again — should fail
        var response = await Client.PostAsJsonAsync("/api/auth/refresh", new
        {
            refreshToken = initial.RefreshToken
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_InvalidToken_Returns401()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/refresh", new
        {
            refreshToken = "completely-invalid-token-value"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_EmptyToken_Returns400()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/refresh", new
        {
            refreshToken = ""
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
