namespace Respondr.Contracts.Identity;

public sealed record UserProfileResponse(
    Guid Id,
    string Email,
    string FullName,
    string Role);
