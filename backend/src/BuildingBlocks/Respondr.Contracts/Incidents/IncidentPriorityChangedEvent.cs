namespace Respondr.Contracts.Incidents;

public sealed record IncidentPriorityChangedEvent(
    Guid IncidentId,
    string IncidentNumber,
    string PreviousPriority,
    string Priority,
    DateTimeOffset ChangedAt);
