namespace Respondr.Contracts.Notifications;

public sealed record CreateNotificationRequest(
    Guid UserId,
    string Type,
    string Title,
    string Message,
    Guid? RelatedIncidentId);
