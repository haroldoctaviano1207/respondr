extern alias incidentsapi;

namespace Respondr.Api.Tests.Incidents;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Respondr.Api.Tests.Support;
using Respondr.Contracts.Incidents;
using Respondr.Shared.Common;

public sealed class IncidentsApiTests : IClassFixture<IncidentsApiFactory>
{
    private readonly HttpClient _client;

    public IncidentsApiTests(IncidentsApiFactory factory)
    {
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            TestAuthTokenFactory.CreateToken(Guid.NewGuid(), "dispatcher@respondr.test", "Dispatcher", "Dispatcher"));
    }

    [Fact]
    public async Task Incident_list_returns_paged_results()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/incidents", new CreateIncidentRequest(
            "Flood response",
            "Flood",
            "High",
            "River Road",
            "Water level is rising quickly.",
            DateTimeOffset.UtcNow));

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var response = await _client.GetAsync("/api/incidents?pageNumber=1&pageSize=10");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<PagedResult<IncidentResponse>>();
        Assert.NotNull(payload);
        Assert.NotEmpty(payload.Items);
        Assert.True(payload.TotalCount >= 1);
    }

    [Fact]
    public async Task Incident_create_defaults_to_new_status()
    {
        var response = await _client.PostAsJsonAsync("/api/incidents", new CreateIncidentRequest(
            "Warehouse fire",
            "Fire",
            "Critical",
            "Port district",
            "Smoke visible across two blocks.",
            DateTimeOffset.UtcNow));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<IncidentResponse>();
        Assert.NotNull(payload);
        Assert.Equal("New", payload.Status);
    }

    [Fact]
    public async Task Incident_create_returns_validation_errors_for_invalid_request()
    {
        var response = await _client.PostAsJsonAsync("/api/incidents", new CreateIncidentRequest(
            string.Empty,
            "Unknown",
            "Severe",
            string.Empty,
            string.Empty,
            default));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

public sealed class IncidentsApiFactory : WebApplicationFactory<incidentsapi::Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:RespondrDb"] = $"Server=(localdb)\\MSSQLLocalDB;Database=RespondrIncidentsApiTests_{Guid.NewGuid():N};Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True",
                ["Database:ApplyMigrations"] = "true",
                ["Jwt:Issuer"] = TestAuthTokenFactory.Issuer,
                ["Jwt:Audience"] = TestAuthTokenFactory.Audience,
                ["Jwt:Key"] = TestAuthTokenFactory.Key,
                ["Jwt:AccessTokenMinutes"] = "60",
                ["Jwt:RefreshTokenDays"] = "7",
                ["Realtime:Enabled"] = "false",
                ["Realtime:BaseUrl"] = "http://localhost:5108",
                ["Realtime:InternalApiKey"] = "respondr-local-internal-key"
            });
        });

        return base.CreateHost(builder);
    }
}
