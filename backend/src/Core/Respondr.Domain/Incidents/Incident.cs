namespace Respondr.Domain.Incidents;

using Respondr.Domain.Common;

public sealed class Incident : AuditableEntity
{
    public Incident(
        Guid id,
        string incidentNumber,
        string title,
        IncidentType type,
        IncidentPriority priority,
        IncidentLocation location,
        string situationSummary,
        DateTimeOffset reportedAt)
        : base(id)
    {
        IncidentNumber = incidentNumber;
        Title = title;
        Type = type;
        Priority = priority;
        Location = location;
        IncidentLocationId = location.Id;
        SituationSummary = situationSummary;
        ReportedAt = reportedAt;
        Status = IncidentStatus.New;
        MarkCreated(DateTimeOffset.UtcNow);
    }

    public string IncidentNumber { get; private set; }

    public string Title { get; private set; }

    public IncidentType Type { get; private set; }

    public IncidentPriority Priority { get; private set; }

    public IncidentStatus Status { get; private set; }

    public IncidentLocation Location { get; private set; }

    public Guid IncidentLocationId { get; private set; }

    public string SituationSummary { get; private set; }

    public DateTimeOffset ReportedAt { get; private set; }

    public DateTimeOffset? ResolvedAt { get; private set; }

    public DateTimeOffset? ClosedAt { get; private set; }

    public void UpdateStatus(IncidentStatus status, DateTimeOffset updatedAt)
    {
        Status = status;

        if (status == IncidentStatus.Resolved)
        {
            ResolvedAt = updatedAt;
        }

        if (status == IncidentStatus.Closed)
        {
            ClosedAt = updatedAt;
        }

        MarkUpdated(updatedAt);
    }

    private Incident()
        : base(Guid.Empty)
    {
        IncidentNumber = string.Empty;
        Title = string.Empty;
        Location = new IncidentLocation(Guid.Empty, string.Empty);
        IncidentLocationId = Guid.Empty;
        SituationSummary = string.Empty;
    }
}
