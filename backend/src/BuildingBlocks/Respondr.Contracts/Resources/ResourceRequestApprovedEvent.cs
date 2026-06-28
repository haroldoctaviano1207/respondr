namespace Respondr.Contracts.Resources;

public sealed record ResourceRequestApprovedEvent(
    Guid ResourceRequestId,
    Guid IncidentId,
    DateTimeOffset ApprovedAt);
