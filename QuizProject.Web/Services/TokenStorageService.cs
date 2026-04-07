namespace QuizProject.Web.Services;

public class TokenStorageService(IHttpContextAccessor httpContextAccessor) : ITokenStorageService
{
    private const string AccessTokenKey = "tokens:access";
    private const string RefreshTokenKey = "tokens:refresh";
    private const string ExpiryKey = "tokens:expires_at";

    private ISession Session => httpContextAccessor.HttpContext!.Session;

    public void StoreTokens(string accessToken, string refreshToken, DateTime expiresAt)
    {
        Session.SetString(AccessTokenKey, accessToken);
        Session.SetString(RefreshTokenKey, refreshToken);
        Session.SetString(ExpiryKey, expiresAt.Ticks.ToString());
    }

    public string? GetAccessToken() => Session.GetString(AccessTokenKey);

    public string? GetRefreshToken() => Session.GetString(RefreshTokenKey);

    public DateTime? GetAccessTokenExpiry()
    {
        var raw = Session.GetString(ExpiryKey);
        return raw is not null && long.TryParse(raw, out var ticks)
            ? new DateTime(ticks, DateTimeKind.Utc)
            : null;
    }

    public void Clear() => Session.Clear();
}
