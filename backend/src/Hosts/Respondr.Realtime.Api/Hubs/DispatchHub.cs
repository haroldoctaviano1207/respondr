namespace Respondr.Realtime.Api.Hubs;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Respondr.Shared.Constants;

[Authorize(Policy = PolicyNames.DispatcherOrOperationsLead)]
public sealed class DispatchHub : Hub
{
    private readonly ILogger<DispatchHub> _logger;

    public DispatchHub(ILogger<DispatchHub> logger)
    {
        _logger = logger;
    }

    public override Task OnConnectedAsync()
    {
        _logger.LogInformation("Dispatch hub connected: {ConnectionId}", Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation(exception, "Dispatch hub disconnected: {ConnectionId}", Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }
}
