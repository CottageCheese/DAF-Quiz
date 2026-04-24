using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using QuizProject.Api.Controllers;
using QuizProject.Tests.Integration.Infrastructure;

namespace QuizProject.Tests.Integration.Auth;

[Collection("AuthTests")]
public class RevokeTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Revoke_ValidToken_Returns204()
    {
        var regResponse = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "revoke1@test.local",
            password = "Revoke@123!",
            displayName = "RevokeUser1"
        });
        var auth = await regResponse.Content.ReadFromJsonAsync<AuthResponse>();

        var client = CreateAuthenticatedClient(auth!.AccessToken);
        var response = await client.PostAsJsonAsync("/api/auth/revoke", new
        {
            refreshToken = auth.RefreshToken
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Revoke_AfterRevoke_RefreshReturns401()
    {
        var regResponse = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "revoke2@test.local",
            password = "Revoke@123!",
            displayName = "RevokeUser2"
        });
        var auth = await regResponse.Content.ReadFromJsonAsync<AuthResponse>();

        // Revoke
        var authedClient = CreateAuthenticatedClient(auth!.AccessToken);
        await authedClient.PostAsJsonAsync("/api/auth/revoke", new { refreshToken = auth.RefreshToken });

        // Attempt refresh with revoked token
        var response = await Client.PostAsJsonAsync("/api/auth/refresh", new
        {
            refreshToken = auth.RefreshToken
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Revoke_NoAuth_Returns401()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/revoke", new
        {
            refreshToken = "some-token"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Revoke_EmptyToken_Returns400()
    {
        var token = await GetUserTokenAsync();
        var client = CreateAuthenticatedClient(token);
        var response = await client.PostAsJsonAsync("/api/auth/revoke", new { refreshToken = "" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
