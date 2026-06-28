namespace Respondr.Contracts.Incidents;

public sealed record CreateIncidentRequest(
    string Title,
    string Type,
    string Priority,
    string Location,
    string SituationSummary,
    DateTimeOffset ReportedAt);
