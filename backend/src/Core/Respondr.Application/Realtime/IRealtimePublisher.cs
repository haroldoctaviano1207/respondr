namespace Respondr.Application.Realtime;

public interface IRealtimePublisher
{
    Task PublishIncidentsAsync<TPayload>(string eventType, TPayload payload, CancellationToken cancellationToken = default);

    Task PublishDispatchAsync<TPayload>(string eventType, TPayload payload, CancellationToken cancellationToken = default);

    Task PublishNotificationsAsync<TPayload>(string eventType, TPayload payload, CancellationToken cancellationToken = default);
}
