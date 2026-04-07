using QuizProject.Api.Models.Domain;

namespace QuizProject.Api.Services;

public interface ITokenService
{
    Task<string> GenerateAccessTokenAsync(ApplicationUser user);
    Task<string> CreateRefreshTokenAsync(ApplicationUser user);
    Task<(string AccessToken, string RefreshToken)?> RotateRefreshTokenAsync(string rawRefreshToken);
    Task<bool> RevokeRefreshTokenAsync(string rawRefreshToken);
    Task RevokeAllRefreshTokensForUserAsync(string userId);
}
