extern alias realtimeapi;

namespace Respondr.Api.Tests.Realtime;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Respondr.Api.Tests.Support;
using Respondr.Contracts.Realtime;
using Respondr.Shared.Constants;

public sealed class RealtimeApiTests : IClassFixture<RealtimeApiFactory>
{
    private readonly RealtimeApiFactory _factory;
    private readonly HttpClient _client;

    public RealtimeApiTests(RealtimeApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_endpoints_return_healthy_status()
    {
        var healthResponse = await _client.GetAsync("/api/health");
        var realtimeHealthResponse = await _client.GetAsync("/api/realtime/health");
        var databaseHealthResponse = await _client.GetAsync("/api/health/db");

        Assert.Equal(HttpStatusCode.OK, healthResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, realtimeHealthResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, databaseHealthResponse.StatusCode);
    }

    [Fact]
    public async Task Unauthenticated_client_is_rejected_from_hub()
    {
        var connection = CreateConnection(accessToken: null);

        await Assert.ThrowsAnyAsync<Exception>(() => connection.StartAsync());
    }

    [Fact]
    public async Task Published_incident_event_is_received_by_connected_client()
    {
        var token = TestAuthTokenFactory.CreateToken(Guid.NewGuid(), "dispatcher@respondr.test", "Dispatcher", "Dispatcher");
        var connection = CreateConnection(token);
        var received = new TaskCompletionSource<RealtimeMessage<JsonElement>>(TaskCreationOptions.RunContinuationsAsynchronously);

        connection.On<RealtimeMessage<JsonElement>>("Receive", message => received.TrySetResult(message));
        await connection.StartAsync();

        var request = new PublishRealtimeEventRequest(
            "incidents",
            "IncidentCreated",
            JsonSerializer.SerializeToElement(new { incidentId = Guid.NewGuid(), title = "Flooding" }),
            DateTimeOffset.UtcNow);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/internal/realtime/publish")
        {
            Content = JsonContent.Create(request)
        };
        httpRequest.Headers.Add(HeaderNames.InternalApiKey, "respondr-local-internal-key");

        var response = await _client.SendAsync(httpRequest);

        Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

        var message = await received.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal("IncidentCreated", message.EventType);
    }

    private HubConnection CreateConnection(string? accessToken)
    {
        return new HubConnectionBuilder()
            .WithUrl(new Uri(_client.BaseAddress!, "/hubs/incidents"), options =>
            {
                options.Transports = HttpTransportType.LongPolling;
                options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
                options.AccessTokenProvider = () => Task.FromResult(accessToken);
            })
            .Build();
    }
}

public sealed class RealtimeApiFactory : WebApplicationFactory<realtimeapi::Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:RespondrDb"] = $"Server=(localdb)\\MSSQLLocalDB;Database=RespondrRealtimeApiTests_{Guid.NewGuid():N};Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True",
                ["Database:ApplyMigrations"] = "true",
                ["Jwt:Issuer"] = TestAuthTokenFactory.Issuer,
                ["Jwt:Audience"] = TestAuthTokenFactory.Audience,
                ["Jwt:Key"] = TestAuthTokenFactory.Key,
                ["Jwt:AccessTokenMinutes"] = "60",
                ["Jwt:RefreshTokenDays"] = "7",
                ["Realtime:Enabled"] = "true",
                ["Realtime:BaseUrl"] = "http://localhost:5108",
                ["Realtime:InternalApiKey"] = "respondr-local-internal-key"
            });
        });

        return base.CreateHost(builder);
    }
}
