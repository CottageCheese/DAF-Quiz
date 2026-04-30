using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using QuizProject.Contracts;
using QuizProject.Tests.Integration.Infrastructure;

namespace QuizProject.Tests.Integration.Auth;

[Collection("AuthTests")]
public class RegisterTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Register_ValidRequest_Returns200WithTokens()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "newuser@example.com",
            password = "ValidPass@1",
            displayName = "NewUser"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        body!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.RefreshToken.Should().NotBeNullOrWhiteSpace();
        body.ExpiresIn.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        // First register
        await Client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "dup@example.com",
            password = "ValidPass@1",
            displayName = "DupUser1"
        });

        // Second register with same email
        var response = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "dup@example.com",
            password = "ValidPass@1",
            displayName = "DupUser2"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("email");
    }

    [Fact]
    public async Task Register_DuplicateDisplayName_Returns409()
    {
        await Client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "dnuser1@example.com",
            password = "ValidPass@1",
            displayName = "SharedName"
        });

        var response = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "dnuser2@example.com",
            password = "ValidPass@1",
            displayName = "SharedName"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("display name");
    }

    [Fact]
    public async Task Register_WeakPassword_Returns400()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "weak@example.com",
            password = "abc",
            displayName = "WeakPassUser"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_MissingEmail_Returns400()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "",
            password = "ValidPass@1",
            displayName = "NoEmail"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_MissingDisplayName_Returns400()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            email = "nodisplay@example.com",
            password = "ValidPass@1",
            displayName = ""
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
