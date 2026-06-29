namespace Respondr.Contracts.Incidents;

public sealed record IncidentDetailResponse(
    Guid Id,
    string IncidentNumber,
    string Title,
    string Type,
    string Priority,
    string Status,
    string Location,
    string SituationSummary,
    DateTimeOffset ReportedAt,
    IReadOnlyList<IncidentHistoryResponse> History);
