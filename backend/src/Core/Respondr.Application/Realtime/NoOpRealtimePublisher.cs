namespace Respondr.Application.Realtime;

public sealed class NoOpRealtimePublisher : IRealtimePublisher
{
    public Task PublishIncidentsAsync<TPayload>(string eventType, TPayload payload, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task PublishDispatchAsync<TPayload>(string eventType, TPayload payload, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task PublishNotificationsAsync<TPayload>(string eventType, TPayload payload, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
