namespace Respondr.Domain.Resources;

using Respondr.Domain.Common;

public sealed class ResourceAllocation : AuditableEntity
{
    public ResourceAllocation(Guid id, Guid resourceRequestId, Guid resourceItemId)
        : base(id)
    {
        ResourceRequestId = resourceRequestId;
        ResourceItemId = resourceItemId;
        AllocatedAt = DateTimeOffset.UtcNow;
        MarkCreated(DateTimeOffset.UtcNow);
    }

    public Guid ResourceRequestId { get; private set; }

    public Guid ResourceItemId { get; private set; }

    public DateTimeOffset AllocatedAt { get; private set; }

    public DateTimeOffset? FulfilledAt { get; private set; }

    private ResourceAllocation()
        : base(Guid.Empty)
    {
    }
}
