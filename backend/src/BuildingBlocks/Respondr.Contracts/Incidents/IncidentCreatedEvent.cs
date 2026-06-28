namespace Respondr.Contracts.Incidents;

public sealed record IncidentCreatedEvent(
    Guid IncidentId,
    string IncidentNumber,
    string Title,
    string Priority,
    string Status,
    DateTimeOffset CreatedAt);
