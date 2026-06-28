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

    private ResourceItem()
        : base(Guid.Empty)
    {
        Name = string.Empty;
    }
}
