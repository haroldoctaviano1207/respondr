extern alias resourcesapi;

namespace Respondr.Api.Tests.Resources;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Respondr.Api.Tests.Support;
using Respondr.Contracts.Resources;
using Respondr.Domain.Incidents;
using Respondr.Infrastructure.Persistence;
using Respondr.Shared.Common;

public sealed class ResourcesApiTests : IClassFixture<ResourcesApiFactory>
{
    private readonly ResourcesApiFactory _factory;
    private readonly HttpClient _client;

    public ResourcesApiTests(ResourcesApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            TestAuthTokenFactory.CreateToken(Guid.NewGuid(), "lead@respondr.test", "Lead", "OperationsLead"));
    }

    [Fact]
    public async Task Resource_request_create_defaults_to_pending()
    {
        var incidentId = await _factory.SeedIncidentAsync();

        var response = await _client.PostAsJsonAsync("/api/resource-requests", new CreateResourceRequest(
            incidentId,
            "Supply",
            2,
            "Need additional medical kits"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<ResourceRequestResponse>();
        Assert.NotNull(payload);
        Assert.Equal("Pending", payload.Status);
    }

    [Fact]
    public async Task Resource_request_list_returns_paged_results()
    {
        var incidentId = await _factory.SeedIncidentAsync();
        await _client.PostAsJsonAsync("/api/resource-requests", new CreateResourceRequest(
            incidentId,
            "Equipment",
            1,
            "Need a generator"));

        var response = await _client.GetAsync("/api/resource-requests");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<PagedResult<ResourceRequestResponse>>();
        Assert.NotNull(payload);
        Assert.NotEmpty(payload.Items);
    }
}

public sealed class ResourcesApiFactory : WebApplicationFactory<resourcesapi::Program>
{
    public async Task<Guid> SeedIncidentAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RespondrDbContext>();
        var incident = new Incident(
            Guid.NewGuid(),
            $"INC-{Guid.NewGuid():N}"[..20],
            "Resource incident",
            IncidentType.Flood,
            IncidentPriority.Medium,
            new IncidentLocation(Guid.NewGuid(), "Resource test location"),
            "Resource test summary",
            DateTimeOffset.UtcNow);

        dbContext.Incidents.Add(incident);
        await dbContext.SaveChangesAsync();
        return incident.Id;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:RespondrDb"] = $"Server=(localdb)\\MSSQLLocalDB;Database=RespondrResourcesApiTests_{Guid.NewGuid():N};Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True",
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
