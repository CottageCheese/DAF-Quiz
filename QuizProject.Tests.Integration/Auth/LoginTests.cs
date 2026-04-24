using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using QuizProject.Api.Controllers;
using QuizProject.Tests.Integration.Infrastructure;

namespace QuizProject.Tests.Integration.Auth;

[Collection("AuthTests")]
public class LoginTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Login_ValidCredentials_Returns200WithTokens()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            email = Seed.UserEmail,
            password = Seed.UserPassword
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.RefreshToken.Should().NotBeNullOrWhiteSpace();
        body.ExpiresIn.Should().Be(900); // 15 minutes * 60
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            email = Seed.UserEmail,
            password = "WrongPassword@1"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_NonExistentEmail_Returns401()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "nobody@example.com",
            password = "ValidPass@1"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_EmptyBody_Returns400()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "",
            password = ""
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_AccountLockout_Returns429After5Failures()
    {
        const string lockoutEmail = "lockout@test.local";
        const string lockoutPassword = "LockoutUser@1";

        // Register the user first
        await Client.PostAsJsonAsync("/api/auth/register", new
        {
            email = lockoutEmail,
            password = lockoutPassword,
            displayName = "LockoutUser"
        });

        // Send 5 wrong password attempts
        for (var i = 0; i < 5; i++)
        {
            await Client.PostAsJsonAsync("/api/auth/login", new
            {
                email = lockoutEmail,
                password = "WrongPass@99"
            });
        }

        // 6th attempt — account should be locked
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            email = lockoutEmail,
            password = "WrongPass@99"
        });

        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
    }
}
