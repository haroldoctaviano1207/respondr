namespace Respondr.Contracts.Incidents;

public sealed record UpdateIncidentPriorityRequest(string Priority, string? Note = null);
