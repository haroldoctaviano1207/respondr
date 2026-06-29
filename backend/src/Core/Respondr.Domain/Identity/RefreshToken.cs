namespace Respondr.Domain.Identity;

using Respondr.Domain.Common;

public sealed class RefreshToken : AuditableEntity
{
    public RefreshToken(
        Guid id,
        Guid userId,
        string token,
        DateTimeOffset expiresAt)
        : base(id)
    {
        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
        MarkCreated(DateTimeOffset.UtcNow);
    }

    public Guid UserId { get; private set; }

    public string Token { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    public DateTimeOffset? RevokedAt { get; private set; }

    public bool IsRevoked => RevokedAt.HasValue;

    public void Revoke(DateTimeOffset revokedAt)
    {
        RevokedAt = revokedAt;
        MarkUpdated(revokedAt);
    }

    private RefreshToken()
        : base(Guid.Empty)
    {
        Token = string.Empty;
    }
}
