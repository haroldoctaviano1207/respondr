namespace Respondr.Contracts.Resources;

public sealed record CreateResourceRequest(
    Guid IncidentId,
    string ResourceType,
    int Quantity,
    string Justification);
