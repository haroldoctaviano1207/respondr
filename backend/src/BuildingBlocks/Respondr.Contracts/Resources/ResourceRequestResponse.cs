namespace Respondr.Contracts.Resources;

public sealed record ResourceRequestResponse(
    Guid Id,
    Guid IncidentId,
    string ResourceType,
    int Quantity,
    string Status,
    DateTimeOffset RequestedAt,
    string Justification,
    string? DecisionNotes = null);
