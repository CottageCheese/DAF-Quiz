namespace QuizProject.Web.Common.Services;

/// <summary>Tokens returned by the API auth endpoints.</summary>
public sealed record AuthTokens(string AccessToken, string RefreshToken, int ExpiresIn);
