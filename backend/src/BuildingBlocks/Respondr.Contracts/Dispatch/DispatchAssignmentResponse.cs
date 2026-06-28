namespace Respondr.Contracts.Dispatch;

public sealed record DispatchAssignmentResponse(
    Guid Id,
    Guid IncidentId,
    Guid ResponderId,
    string Status,
    DateTimeOffset AssignedAt);
