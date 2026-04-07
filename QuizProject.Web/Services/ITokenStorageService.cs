namespace QuizProject.Web.Services;

public interface ITokenStorageService
{
    void StoreTokens(string accessToken, string refreshToken, DateTime expiresAt);
    string? GetAccessToken();
    string? GetRefreshToken();
    DateTime? GetAccessTokenExpiry();
    void Clear();
}
