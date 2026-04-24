using System.Net.Http.Headers;

namespace QuizProject.Tests.Integration.Infrastructure;

/// <summary>
/// Base class for integration tests. Exposes a plain HttpClient (no auth)
/// and helpers to obtain authenticated clients using seeded credentials.
/// </summary>
public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;
    protected readonly TestSeedContext Seed;
    private readonly AuthHelper _authHelper;

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient();
        Seed = factory.Seed;
        _authHelper = new AuthHelper(Client);
    }

    protected async Task<HttpClient> CreateAdminClientAsync()
    {
        var auth = await _authHelper.LoginAsync(Seed.AdminEmail, Seed.AdminPassword);
        return CreateAuthenticatedClient(auth.AccessToken);
    }

    protected async Task<HttpClient> CreateUserClientAsync()
    {
        var auth = await _authHelper.LoginAsync(Seed.UserEmail, Seed.UserPassword);
        return CreateAuthenticatedClient(auth.AccessToken);
    }

    protected async Task<string> GetAdminTokenAsync()
    {
        var auth = await _authHelper.LoginAsync(Seed.AdminEmail, Seed.AdminPassword);
        return auth.AccessToken;
    }

    protected async Task<string> GetUserTokenAsync()
    {
        var auth = await _authHelper.LoginAsync(Seed.UserEmail, Seed.UserPassword);
        return auth.AccessToken;
    }

    protected HttpClient CreateAuthenticatedClient(string token)
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
