namespace Respondr.Domain.Resources;

using Respondr.Domain.Common;

public sealed class ResourceItem : AuditableEntity
{
    public ResourceItem(Guid id, string name, ResourceType type, bool isAvailable = true)
        : base(id)
    {
        Name = name;
        Type = type;
        IsAvailable = isAvailable;
        MarkCreated(DateTimeOffset.UtcNow);
    }

    public string Name { get; private set; }

    public ResourceType Type { get; private set; }

    public bool IsAvailable { get; private set; }

    public Guid? CurrentIncidentId { get; private set; }

    public void AssignToIncident(Guid incidentId, DateTimeOffset updatedAt)
    {
        CurrentIncidentId = incidentId;
        IsAvailable = false;
        MarkUpdated(updatedAt);
    }

    public void Release(DateTimeOffset updatedAt)
    {
        CurrentIncidentId = null;
        IsAvailable = true;
        MarkUpdated(updatedAt);
    }

    private ResourceItem()
        : base(Guid.Empty)
    {
        Name = string.Empty;
    }
}
