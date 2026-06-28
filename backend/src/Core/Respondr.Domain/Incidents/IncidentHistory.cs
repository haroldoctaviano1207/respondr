namespace Respondr.Domain.Incidents;

using Respondr.Domain.Common;

public sealed class IncidentHistory : AuditableEntity
{
    public IncidentHistory(
        Guid id,
        Guid incidentId,
        string eventType,
        string description,
        Guid? createdByUserId = null)
        : base(id)
    {
        IncidentId = incidentId;
        EventType = eventType;
        Description = description;
        CreatedByUserId = createdByUserId;
        MarkCreated(DateTimeOffset.UtcNow);
    }

    public Guid IncidentId { get; private set; }

    public string EventType { get; private set; }

    public string Description { get; private set; }

    public Guid? CreatedByUserId { get; private set; }

    private IncidentHistory()
        : base(Guid.Empty)
    {
        EventType = string.Empty;
        Description = string.Empty;
    }
}
