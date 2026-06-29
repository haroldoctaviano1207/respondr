namespace Respondr.Contracts.Incidents;

public sealed record IncidentHistoryResponse(
    Guid Id,
    string EventType,
    string Description,
    Guid? CreatedByUserId,
    DateTimeOffset CreatedAt);
