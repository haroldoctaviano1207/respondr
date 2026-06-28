namespace Respondr.Contracts.Notifications;

public sealed record NotificationResponse(
    Guid Id,
    string Type,
    string Title,
    string Message,
    bool IsRead,
    DateTimeOffset CreatedAt);
