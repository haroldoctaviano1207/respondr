namespace Respondr.Contracts.Realtime;

public sealed record DashboardUpdatedEvent(
    int ActiveIncidents,
    int CriticalIncidents,
    int AvailableResponders,
    int PendingResourceRequests,
    DateTimeOffset UpdatedAt);
