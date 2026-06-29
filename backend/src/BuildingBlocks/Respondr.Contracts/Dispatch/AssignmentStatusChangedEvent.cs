namespace Respondr.Contracts.Dispatch;

public sealed record AssignmentStatusChangedEvent(
    Guid AssignmentId,
    Guid IncidentId,
    Guid ResponderId,
    string Status,
    DateTimeOffset ChangedAt);
