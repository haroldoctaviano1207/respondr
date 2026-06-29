namespace Respondr.Realtime.Api.Services;

using Microsoft.AspNetCore.SignalR;
using Respondr.Contracts.Realtime;
using Respondr.Realtime.Api.Hubs;

public sealed class RealtimeHubDispatcher
{
    private readonly IHubContext<IncidentsHub> _incidentsHubContext;
    private readonly IHubContext<DispatchHub> _dispatchHubContext;
    private readonly IHubContext<NotificationsHub> _notificationsHubContext;
    private readonly ILogger<RealtimeHubDispatcher> _logger;

    public RealtimeHubDispatcher(
        IHubContext<IncidentsHub> incidentsHubContext,
        IHubContext<DispatchHub> dispatchHubContext,
        IHubContext<NotificationsHub> notificationsHubContext,
        ILogger<RealtimeHubDispatcher> logger)
    {
        _incidentsHubContext = incidentsHubContext;
        _dispatchHubContext = dispatchHubContext;
        _notificationsHubContext = notificationsHubContext;
        _logger = logger;
    }

    public async Task DispatchAsync(PublishRealtimeEventRequest request, CancellationToken cancellationToken = default)
    {
        var message = new RealtimeMessage<System.Text.Json.JsonElement>(
            request.EventType,
            request.Payload,
            request.OccurredAt,
            request.CorrelationId);

        try
        {
            switch (request.Channel.Trim().ToLowerInvariant())
            {
                case "incidents":
                    await _incidentsHubContext.Clients.All.SendAsync("Receive", message, cancellationToken);
                    break;
                case "dispatch":
                    await _dispatchHubContext.Clients.All.SendAsync("Receive", message, cancellationToken);
                    break;
                case "notifications":
                    await _notificationsHubContext.Clients.All.SendAsync("Receive", message, cancellationToken);
                    break;
                default:
                    throw new InvalidOperationException($"Unknown realtime channel '{request.Channel}'.");
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to dispatch realtime event {EventType} to channel {Channel}.",
                request.EventType,
                request.Channel);
            throw;
        }
    }
}
