namespace Respondr.Contracts.Incidents;

public sealed record IncidentStatusChangedEvent(
    Guid IncidentId,
    string IncidentNumber,
    string PreviousStatus,
    string Status,
    DateTimeOffset ChangedAt);
