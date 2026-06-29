namespace Respondr.Contracts.Incidents;

public sealed record UpdateIncidentStatusRequest(string Status, string? Note = null);
