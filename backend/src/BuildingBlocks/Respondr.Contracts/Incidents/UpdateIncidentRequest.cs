namespace Respondr.Contracts.Incidents;

public sealed record UpdateIncidentRequest(
    string Title,
    string Type,
    string Location,
    string SituationSummary,
    DateTimeOffset ReportedAt);
