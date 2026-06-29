namespace Respondr.Domain.Dispatch;

using Respondr.Domain.Common;

public sealed class DispatchAssignment : AuditableEntity
{
    public DispatchAssignment(
        Guid id,
        Guid incidentId,
        Guid responderProfileId,
        Guid assignedByUserId)
        : base(id)
    {
        IncidentId = incidentId;
        ResponderProfileId = responderProfileId;
        AssignedByUserId = assignedByUserId;
        Status = AssignmentStatus.Active;
        AssignedAt = DateTimeOffset.UtcNow;
        MarkCreated(DateTimeOffset.UtcNow);
    }

    public Guid IncidentId { get; private set; }

    public Guid ResponderProfileId { get; private set; }

    public Guid AssignedByUserId { get; private set; }

    public AssignmentStatus Status { get; private set; }

    public DateTimeOffset AssignedAt { get; private set; }

    public DateTimeOffset? ReleasedAt { get; private set; }

    public void UpdateStatus(AssignmentStatus status, DateTimeOffset updatedAt)
    {
        Status = status;

        if (status is AssignmentStatus.Released or AssignmentStatus.Completed or AssignmentStatus.Cancelled)
        {
            ReleasedAt = updatedAt;
        }

        MarkUpdated(updatedAt);
    }

    public void Release(DateTimeOffset releasedAt)
    {
        Status = AssignmentStatus.Released;
        ReleasedAt = releasedAt;
        MarkUpdated(releasedAt);
    }

    private DispatchAssignment()
        : base(Guid.Empty)
    {
    }
}
