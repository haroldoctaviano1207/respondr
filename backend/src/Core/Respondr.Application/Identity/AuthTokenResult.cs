namespace Respondr.Application.Identity;

public sealed record AuthTokenResult(string AccessToken, DateTimeOffset ExpiresAt);
