namespace Respondr.Contracts.Incidents;

using Respondr.Contracts.Common;

public sealed record IncidentLookupsResponse(
    IReadOnlyList<LookupOptionResponse> Types,
    IReadOnlyList<LookupOptionResponse> Priorities,
    IReadOnlyList<LookupOptionResponse> Statuses);
