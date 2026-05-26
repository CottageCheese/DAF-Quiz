using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using QuizProject.Domain.Data;
using QuizProject.Domain.Models.Domain;

namespace QuizProject.Api.Services;

public sealed class TokenService(
    IOptions<JwtSettings> jwtOptions,
    UserManager<ApplicationUser> userManager,
    ApplicationDbContext db,
    ILogger<TokenService> logger) : ITokenService
{
    private readonly JwtSettings _jwt = jwtOptions.Value;

    public async Task<string> GenerateAccessTokenAsync(ApplicationUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var roles = await userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("display_name", user.DisplayName),
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.AccessTokenExpiryMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> CreateRefreshTokenAsync(ApplicationUser user)
    {
        var rawToken = GenerateRawToken();
        var entity = new RefreshToken
        {
            TokenHash = HashToken(rawToken),
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpiryDays)
        };

        db.Add(entity);
        await db.SaveChangesAsync();

        return rawToken;
    }

    public async Task<(string AccessToken, string RefreshToken)?> RotateRefreshTokenAsync(string rawRefreshToken)
    {
        var hash = HashToken(rawRefreshToken);
        var existing = await db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == hash);

        if (existing is null) return null;

        if (existing.IsUsed)
        {
            logger.LogWarning("Refresh token reuse detected for user {UserId} — revoking all tokens", existing.UserId);
            await RevokeAllRefreshTokensForUserAsync(existing.UserId);
            return null;
        }

        if (!existing.IsActive) return null;

        var user = await userManager.FindByIdAsync(existing.UserId);
        if (user is null) return null;

        var newRaw = GenerateRawToken();
        var newHash = HashToken(newRaw);

        existing.UsedAt = DateTime.UtcNow;
        existing.ReplacedByTokenHash = newHash;

        db.Add(new RefreshToken
        {
            TokenHash = newHash,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpiryDays)
        });
        await db.SaveChangesAsync();

        return (await GenerateAccessTokenAsync(user), newRaw);
    }

    public async Task<bool> RevokeRefreshTokenAsync(string rawRefreshToken)
    {
        var hash = HashToken(rawRefreshToken);
        var existing = await db.RefreshTokens.FirstOrDefaultAsync(r => r.TokenHash == hash);

        if (existing is null || !existing.IsActive) return false;

        existing.RevokedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        logger.LogInformation("Refresh token revoked for user {UserId}", existing.UserId);
        return true;
    }

    public async Task RevokeAllRefreshTokensForUserAsync(string userId)
    {
        var active = await db.RefreshTokens
            .Where(r => r.UserId == userId && r.RevokedAt == null && r.UsedAt == null)
            .ToListAsync();

        foreach (var token in active)
            token.RevokedAt = DateTime.UtcNow;

        if (active.Count > 0)
            await db.SaveChangesAsync();
    }

    private static string GenerateRawToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string HashToken(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToBase64String(bytes);
    }
}
