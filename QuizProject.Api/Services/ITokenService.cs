using Microsoft.AspNetCore.Identity;

namespace QuizProject.Api.Services;

public interface ITokenService
{
    Task<string> GenerateAccessTokenAsync(IdentityUser user);
    Task<string> CreateRefreshTokenAsync(IdentityUser user);
    Task<(string AccessToken, string RefreshToken)?> RotateRefreshTokenAsync(string rawRefreshToken);
    Task<bool> RevokeRefreshTokenAsync(string rawRefreshToken);
    Task RevokeAllRefreshTokensForUserAsync(string userId);
}
