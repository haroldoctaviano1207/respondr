namespace Respondr.Contracts.Resources;

public sealed record AvailableResourceResponse(
    Guid Id,
    string Name,
    string ResourceType,
    bool IsAvailable,
    Guid? CurrentIncidentId);
