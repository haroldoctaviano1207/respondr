namespace Respondr.Contracts.Realtime;

using System.Text.Json;

public sealed record PublishRealtimeEventRequest(
    string Channel,
    string EventType,
    JsonElement Payload,
    DateTimeOffset OccurredAt,
    string? CorrelationId = null);
