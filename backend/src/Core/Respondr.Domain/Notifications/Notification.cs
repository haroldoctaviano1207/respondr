namespace Respondr.Domain.Notifications;

using Respondr.Domain.Common;

public sealed class Notification : AuditableEntity
{
    public Notification(
        Guid id,
        Guid userId,
        NotificationType type,
        string title,
        string message,
        Guid? relatedIncidentId = null)
        : base(id)
    {
        UserId = userId;
        Type = type;
        Title = title;
        Message = message;
        RelatedIncidentId = relatedIncidentId;
        Status = NotificationStatus.Unread;
        IsRead = false;
        MarkCreated(DateTimeOffset.UtcNow);
    }

    public Guid UserId { get; private set; }

    public NotificationType Type { get; private set; }

    public string Title { get; private set; }

    public string Message { get; private set; }

    public NotificationStatus Status { get; private set; }

    public bool IsRead { get; private set; }

    public Guid? RelatedIncidentId { get; private set; }

    public DateTimeOffset? ReadAt { get; private set; }

    public void MarkAsRead(DateTimeOffset readAt)
    {
        Status = NotificationStatus.Read;
        IsRead = true;
        ReadAt = readAt;
        MarkUpdated(readAt);
    }

    private Notification()
        : base(Guid.Empty)
    {
        Title = string.Empty;
        Message = string.Empty;
    }
}
