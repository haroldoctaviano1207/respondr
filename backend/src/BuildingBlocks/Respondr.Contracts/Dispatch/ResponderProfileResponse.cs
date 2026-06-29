namespace Respondr.Contracts.Dispatch;

public sealed record ResponderProfileResponse(
    Guid Id,
    Guid UserId,
    string DisplayName,
    string ResponderType,
    string Status,
    Guid? CurrentIncidentId);
