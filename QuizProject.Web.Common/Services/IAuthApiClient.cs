namespace QuizProject.Web.Common.Services;

/// <summary>Auth-only API operations shared by all Web frontends.</summary>
public interface IAuthApiClient
{
    Task<ApiResult<AuthTokens>> LoginAsync(string email, string password);
    Task<ApiResult<AuthTokens>> RegisterAsync(string email, string password, string displayName);
    Task RevokeTokenAsync(string refreshToken);
}
