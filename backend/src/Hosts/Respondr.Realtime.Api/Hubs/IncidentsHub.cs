namespace Respondr.Realtime.Api.Hubs;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Respondr.Shared.Constants;

[Authorize(Policy = PolicyNames.DispatcherOrOperationsLead)]
public sealed class IncidentsHub : Hub
{
    private readonly ILogger<IncidentsHub> _logger;

    public IncidentsHub(ILogger<IncidentsHub> logger)
    {
        _logger = logger;
    }

    public override Task OnConnectedAsync()
    {
        _logger.LogInformation("Incidents hub connected: {ConnectionId}", Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation(exception, "Incidents hub disconnected: {ConnectionId}", Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}
