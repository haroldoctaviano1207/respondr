namespace Respondr.Contracts.Dispatch;

public sealed record ResponderAssignedEvent(
    Guid AssignmentId,
    Guid IncidentId,
    Guid ResponderId,
    string AssignmentStatus,
    DateTimeOffset AssignedAt);
