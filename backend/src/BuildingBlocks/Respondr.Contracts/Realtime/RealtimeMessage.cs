namespace Respondr.Contracts.Realtime;

public sealed record RealtimeMessage<TPayload>(
    string EventType,
    TPayload Payload,
    DateTimeOffset OccurredAt,
    string? CorrelationId = null);
