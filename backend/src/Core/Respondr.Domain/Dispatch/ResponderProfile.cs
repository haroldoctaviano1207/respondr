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

    private ResponderProfile()
        : base(Guid.Empty)
    {
        DisplayName = string.Empty;
        ResponderType = string.Empty;
    }
}
