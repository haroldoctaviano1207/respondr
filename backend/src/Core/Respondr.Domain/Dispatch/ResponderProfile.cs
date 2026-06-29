namespace Respondr.Domain.Dispatch;

using Respondr.Domain.Common;

public sealed class ResponderProfile : AuditableEntity
{
    public ResponderProfile(
        Guid id,
        Guid userId,
        string displayName,
        string responderType,
        ResponderStatus status = ResponderStatus.Available)
        : base(id)
    {
        UserId = userId;
        DisplayName = displayName;
        ResponderType = responderType;
        Status = status;
        MarkCreated(DateTimeOffset.UtcNow);
    }

    public Guid UserId { get; private set; }

    public string DisplayName { get; private set; }

    public string ResponderType { get; private set; }

    public ResponderStatus Status { get; private set; }

    public Guid? CurrentIncidentId { get; private set; }

    public void UpdateStatus(ResponderStatus status, DateTimeOffset updatedAt)
    {
        Status = status;

        if (status == ResponderStatus.Available)
        {
            CurrentIncidentId = null;
        }

        MarkUpdated(updatedAt);
    }

    public void AssignToIncident(Guid incidentId, ResponderStatus status, DateTimeOffset updatedAt)
    {
        CurrentIncidentId = incidentId;
        Status = status;
        MarkUpdated(updatedAt);
    }

    public void ReleaseFromIncident(DateTimeOffset updatedAt)
    {
        CurrentIncidentId = null;
        Status = ResponderStatus.Available;
        MarkUpdated(updatedAt);
    }

    private ResponderProfile()
        : base(Guid.Empty)
    {
        DisplayName = string.Empty;
        ResponderType = string.Empty;
    }
}
