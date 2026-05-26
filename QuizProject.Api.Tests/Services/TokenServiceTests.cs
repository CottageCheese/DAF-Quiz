using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuizProject.Api.Tests.Infrastructure;
using QuizProject.Domain.Models.Domain;

namespace QuizProject.Api.Tests.Services;

public class TokenServiceTests : ApiTestBase
{
    // ---- GenerateAccessTokenAsync ----

    [Fact]
    public async Task GenerateAccessTokenAsync_ContainsExpectedClaims()
    {
        var user = await CreateUserAsync("alice@test.local");

        var token = await TokenService.GenerateAccessTokenAsync(user);

        var handler = new JwtSecurityTokenHandler();
        handler.InboundClaimTypeMap.Clear();
        var validationParams = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = JwtOptions.Value.Issuer,
            ValidAudience = JwtOptions.Value.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(JwtOptions.Value.SecretKey)),
            ClockSkew = TimeSpan.Zero
        };

        var principal = handler.ValidateToken(token, validationParams, out _);
        principal.FindFirst(JwtRegisteredClaimNames.Sub)!.Value.Should().Be(user.Id);
        principal.FindFirst(JwtRegisteredClaimNames.Email)!.Value.Should().Be(user.Email);
    }

    [Fact]
    public async Task GenerateAccessTokenAsync_TokenIsValid()
    {
        var user = await CreateUserAsync();

        var token = await TokenService.GenerateAccessTokenAsync(user);

        token.Should().NotBeNullOrEmpty();
        var handler = new JwtSecurityTokenHandler();
        handler.CanReadToken(token).Should().BeTrue();
    }

    // ---- CreateRefreshTokenAsync ----

    [Fact]
    public async Task CreateRefreshTokenAsync_PersistsHashedToken_RawNotStored()
    {
        var user = await CreateUserAsync();

        var rawToken = await TokenService.CreateRefreshTokenAsync(user);

        rawToken.Should().NotBeNullOrEmpty();
        var stored = await Db.RefreshTokens.SingleAsync(r => r.UserId == user.Id);
        stored.TokenHash.Should().NotBe(rawToken); // hash, not raw
        stored.UsedAt.Should().BeNull();
        stored.RevokedAt.Should().BeNull();
    }

    [Fact]
    public async Task CreateRefreshTokenAsync_ExpirySetCorrectly()
    {
        var user = await CreateUserAsync();
        var before = DateTime.UtcNow;

        await TokenService.CreateRefreshTokenAsync(user);

        var stored = await Db.RefreshTokens.SingleAsync(r => r.UserId == user.Id);
        stored.ExpiresAt.Should().BeAfter(before.AddDays(6));
        stored.ExpiresAt.Should().BeBefore(before.AddDays(8));
    }

    // ---- RotateRefreshTokenAsync ----

    [Fact]
    public async Task RotateRefreshTokenAsync_ValidToken_ReturnsNewTokenPair()
    {
        var user = await CreateUserAsync();
        var raw = await TokenService.CreateRefreshTokenAsync(user);

        var result = await TokenService.RotateRefreshTokenAsync(raw);

        result.Should().NotBeNull();
        result!.Value.AccessToken.Should().NotBeNullOrEmpty();
        result.Value.RefreshToken.Should().NotBeNullOrEmpty();
        result.Value.RefreshToken.Should().NotBe(raw);
    }

    [Fact]
    public async Task RotateRefreshTokenAsync_ValidToken_MarksOldTokenAsUsed()
    {
        var user = await CreateUserAsync();
        var raw = await TokenService.CreateRefreshTokenAsync(user);

        await TokenService.RotateRefreshTokenAsync(raw);

        var oldToken = await Db.RefreshTokens
            .Where(r => r.UserId == user.Id)
            .OrderBy(r => r.CreatedAt)
            .FirstAsync();
        oldToken.UsedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task RotateRefreshTokenAsync_UsedToken_RevokesAllAndReturnsNull()
    {
        var user = await CreateUserAsync();
        var raw = await TokenService.CreateRefreshTokenAsync(user);

        // First rotate (marks token as used, creates new one)
        await TokenService.RotateRefreshTokenAsync(raw);

        // Second rotate on same token — reuse detection
        var result = await TokenService.RotateRefreshTokenAsync(raw);

        result.Should().BeNull();
        var activeTokens = await Db.RefreshTokens
            .Where(r => r.UserId == user.Id && r.RevokedAt == null && r.UsedAt == null)
            .ToListAsync();
        activeTokens.Should().BeEmpty();
    }

    [Fact]
    public async Task RotateRefreshTokenAsync_ExpiredToken_ReturnsNull()
    {
        var user = await CreateUserAsync();
        var raw = await TokenService.CreateRefreshTokenAsync(user);

        // Manually expire the token
        var stored = await Db.RefreshTokens.SingleAsync(r => r.UserId == user.Id);
        stored.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);
        await Db.SaveChangesAsync();

        var result = await TokenService.RotateRefreshTokenAsync(raw);

        result.Should().BeNull();
    }

    [Fact]
    public async Task RotateRefreshTokenAsync_UnknownToken_ReturnsNull()
    {
        var result = await TokenService.RotateRefreshTokenAsync("totally-fake-token");

        result.Should().BeNull();
    }

    // ---- RevokeRefreshTokenAsync ----

    [Fact]
    public async Task RevokeRefreshTokenAsync_ActiveToken_SetsRevokedAt()
    {
        var user = await CreateUserAsync();
        var raw = await TokenService.CreateRefreshTokenAsync(user);

        var revoked = await TokenService.RevokeRefreshTokenAsync(raw);

        revoked.Should().BeTrue();
        var stored = await Db.RefreshTokens.SingleAsync(r => r.UserId == user.Id);
        stored.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_AlreadyRevoked_ReturnsFalse()
    {
        var user = await CreateUserAsync();
        var raw = await TokenService.CreateRefreshTokenAsync(user);
        await TokenService.RevokeRefreshTokenAsync(raw);

        var result = await TokenService.RevokeRefreshTokenAsync(raw);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task RevokeRefreshTokenAsync_UnknownToken_ReturnsFalse()
    {
        var result = await TokenService.RevokeRefreshTokenAsync("nonexistent-token");

        result.Should().BeFalse();
    }

    // ---- RevokeAllRefreshTokensForUserAsync ----

    [Fact]
    public async Task RevokeAllRefreshTokensForUserAsync_RevokesOnlyActiveTokens()
    {
        var user = await CreateUserAsync();

        // Create 3 tokens
        var raw1 = await TokenService.CreateRefreshTokenAsync(user);
        var raw2 = await TokenService.CreateRefreshTokenAsync(user);
        await TokenService.CreateRefreshTokenAsync(user);

        // Revoke one manually first
        await TokenService.RevokeRefreshTokenAsync(raw1);

        // Use one (rotate it)
        await TokenService.RotateRefreshTokenAsync(raw2);

        // Now revoke all remaining active
        await TokenService.RevokeAllRefreshTokensForUserAsync(user.Id);

        var activeTokens = await Db.RefreshTokens
            .Where(r => r.UserId == user.Id && r.RevokedAt == null && r.UsedAt == null)
            .ToListAsync();
        activeTokens.Should().BeEmpty();
    }
}
