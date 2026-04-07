using Microsoft.AspNetCore.Identity;

namespace QuizProject.Api.Models.Domain;

public class RefreshToken
{
    public int Id { get; set; }

    /// <summary>SHA-256 hash (base64) of the raw token sent to the client.</summary>
    public string TokenHash { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;
    public IdentityUser User { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }

    /// <summary>Set when consumed via token rotation. Reuse after this is set = token theft.</summary>
    public DateTime? UsedAt { get; set; }

    /// <summary>Set on explicit revocation (logout / breach response).</summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>Hash of the new token that replaced this one (rotation audit trail).</summary>
    public string? ReplacedByTokenHash { get; set; }

    // Computed — not mapped to DB
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsUsed => UsedAt.HasValue;
    public bool IsActive => !IsExpired && !IsRevoked && !IsUsed;
}
