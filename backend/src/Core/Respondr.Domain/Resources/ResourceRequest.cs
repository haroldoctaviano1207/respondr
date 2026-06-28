namespace Respondr.Domain.Resources;

using Respondr.Domain.Common;

public sealed class ResourceRequest : AuditableEntity
{
    public ResourceRequest(
        Guid id,
        Guid incidentId,
        ResourceType resourceType,
        int quantity,
        string justification)
        : base(id)
    {
        IncidentId = incidentId;
        ResourceType = resourceType;
        Quantity = quantity;
        Justification = justification;
        Status = ResourceRequestStatus.Pending;
        RequestedAt = DateTimeOffset.UtcNow;
        MarkCreated(DateTimeOffset.UtcNow);
    }

    public Guid IncidentId { get; private set; }

    public ResourceType ResourceType { get; private set; }

    public int Quantity { get; private set; }

    public string Justification { get; private set; }

    public ResourceRequestStatus Status { get; private set; }

    public DateTimeOffset RequestedAt { get; private set; }

    public DateTimeOffset? ApprovedAt { get; private set; }

    public DateTimeOffset? RejectedAt { get; private set; }

    private ResourceRequest()
        : base(Guid.Empty)
    {
        Justification = string.Empty;
    }
}
