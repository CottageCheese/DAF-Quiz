using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using QuizProject.Api.Models.Domain;
using QuizProject.Api.Repositories;

namespace QuizProject.Api.Services;

public sealed class JwtSettings
{
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string SecretKey { get; init; } = string.Empty;
    public int AccessTokenExpiryMinutes { get; init; } = 15;
    public int RefreshTokenExpiryDays { get; init; } = 7;
}

public sealed class TokenService(
    IOptions<JwtSettings> jwtOptions,
    UserManager<IdentityUser> userManager,
    IRepository<RefreshToken> refreshTokenRepo) : ITokenService
{
    private readonly JwtSettings _jwt = jwtOptions.Value;

    public async Task<string> GenerateAccessTokenAsync(IdentityUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var roles = await userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
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

    public async Task<string> CreateRefreshTokenAsync(IdentityUser user)
    {
        var rawToken = GenerateRawToken();
        var entity = new RefreshToken
        {
            TokenHash = HashToken(rawToken),
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpiryDays)
        };

        await refreshTokenRepo.AddAsync(entity);
        await refreshTokenRepo.SaveChangesAsync();

        return rawToken;
    }

    public async Task<(string AccessToken, string RefreshToken)?> RotateRefreshTokenAsync(string rawRefreshToken)
    {
        var hash = HashToken(rawRefreshToken);
        var existing = (await refreshTokenRepo.FindAsync(r => r.TokenHash == hash)).FirstOrDefault();

        if (existing is null) return null;

        if (existing.IsUsed)
        {
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
        refreshTokenRepo.Update(existing);

        var newEntity = new RefreshToken
        {
            TokenHash = newHash,
            UserId = user.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpiryDays)
        };
        await refreshTokenRepo.AddAsync(newEntity);
        await refreshTokenRepo.SaveChangesAsync();

        return (await GenerateAccessTokenAsync(user), newRaw);
    }

    public async Task<bool> RevokeRefreshTokenAsync(string rawRefreshToken)
    {
        var hash = HashToken(rawRefreshToken);
        var existing = (await refreshTokenRepo.FindAsync(r => r.TokenHash == hash)).FirstOrDefault();

        if (existing is null || !existing.IsActive) return false;

        existing.RevokedAt = DateTime.UtcNow;
        refreshTokenRepo.Update(existing);
        await refreshTokenRepo.SaveChangesAsync();
        return true;
    }

    public async Task RevokeAllRefreshTokensForUserAsync(string userId)
    {
        var active = (await refreshTokenRepo.FindAsync(
            r => r.UserId == userId && r.RevokedAt == null && r.UsedAt == null)).ToList();

        foreach (var token in active)
        {
            token.RevokedAt = DateTime.UtcNow;
            refreshTokenRepo.Update(token);
        }

        if (active.Count > 0)
            await refreshTokenRepo.SaveChangesAsync();
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
