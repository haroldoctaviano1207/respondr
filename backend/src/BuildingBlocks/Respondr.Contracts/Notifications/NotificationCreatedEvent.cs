namespace Respondr.Contracts.Notifications;

public sealed record NotificationCreatedEvent(
    Guid NotificationId,
    Guid UserId,
    string Type,
    string Title,
    DateTimeOffset CreatedAt);
