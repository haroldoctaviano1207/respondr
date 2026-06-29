namespace Respondr.Infrastructure.Realtime;

using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Respondr.Application.Realtime;
using Respondr.Contracts.Realtime;
using Respondr.Shared.Constants;

public sealed class HttpRealtimePublisher : IRealtimePublisher
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HttpRealtimePublisher> _logger;
    private readonly RealtimeApiOptions _options;

    public HttpRealtimePublisher(
        HttpClient httpClient,
        IOptions<RealtimeApiOptions> options,
        ILogger<HttpRealtimePublisher> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
    }

    public Task PublishIncidentsAsync<TPayload>(string eventType, TPayload payload, CancellationToken cancellationToken = default) =>
        PublishAsync("incidents", eventType, payload, cancellationToken);

    public Task PublishDispatchAsync<TPayload>(string eventType, TPayload payload, CancellationToken cancellationToken = default) =>
        PublishAsync("dispatch", eventType, payload, cancellationToken);

    public Task PublishNotificationsAsync<TPayload>(string eventType, TPayload payload, CancellationToken cancellationToken = default) =>
        PublishAsync("notifications", eventType, payload, cancellationToken);

    private async Task PublishAsync<TPayload>(string channel, string eventType, TPayload payload, CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            return;
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/internal/realtime/publish")
            {
                Content = JsonContent.Create(new PublishRealtimeEventRequest(
                    channel,
                    eventType,
                    JsonSerializer.SerializeToElement(payload),
                    DateTimeOffset.UtcNow))
            };

            request.Headers.Add(HeaderNames.InternalApiKey, _options.InternalApiKey);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Realtime publish failed with status code {StatusCode} for channel {Channel} and event type {EventType}.",
                    response.StatusCode,
                    channel,
                    eventType);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Realtime publish failed for channel {Channel} and event type {EventType}.",
                channel,
                eventType);
        }
    }
}
