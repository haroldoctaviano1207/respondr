extern alias dispatchapi;

namespace Respondr.Api.Tests.Dispatch;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Respondr.Api.Tests.Support;
using Respondr.Contracts.Dispatch;
using Respondr.Domain.Dispatch;
using Respondr.Domain.Incidents;
using Respondr.Infrastructure.Persistence;
using Respondr.Shared.Common;

public sealed class DispatchApiTests : IClassFixture<DispatchApiFactory>
{
    private readonly DispatchApiFactory _factory;
    private readonly HttpClient _client;

    public DispatchApiTests(DispatchApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            TestAuthTokenFactory.CreateToken(Guid.NewGuid(), "lead@respondr.test", "Lead", "OperationsLead"));
    }

    [Fact]
    public async Task Assignment_rejects_unavailable_responder()
    {
        var (incidentId, responderId) = await _factory.SeedIncidentAndResponderAsync(ResponderStatus.Unavailable);

        var response = await _client.PostAsJsonAsync($"/api/incidents/{incidentId}/assignments", new AssignResponderRequest(responderId));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Assignment_release_updates_statuses()
    {
        var (incidentId, responderId) = await _factory.SeedIncidentAndResponderAsync(ResponderStatus.Available);

        var createResponse = await _client.PostAsJsonAsync($"/api/incidents/{incidentId}/assignments", new AssignResponderRequest(responderId));
        var assignment = await createResponse.Content.ReadFromJsonAsync<DispatchAssignmentResponse>();

        Assert.NotNull(assignment);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var releaseResponse = await _client.PatchAsync($"/api/assignments/{assignment.Id}/release", JsonContent.Create(new { }));

        Assert.Equal(HttpStatusCode.OK, releaseResponse.StatusCode);

        var listResponse = await _client.GetAsync("/api/assignments");
        var payload = await listResponse.Content.ReadFromJsonAsync<PagedResult<DispatchAssignmentResponse>>();

        Assert.NotNull(payload);
        Assert.Contains(payload.Items, item => item.Id == assignment.Id && item.Status == "Released");
    }
}

public sealed class DispatchApiFactory : WebApplicationFactory<dispatchapi::Program>
{
    public async Task<(Guid IncidentId, Guid ResponderId)> SeedIncidentAndResponderAsync(ResponderStatus responderStatus)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RespondrDbContext>();

        var incident = new Incident(
            Guid.NewGuid(),
            $"INC-{Guid.NewGuid():N}"[..20],
            "Incident for dispatch",
            IncidentType.Flood,
            IncidentPriority.High,
            new IncidentLocation(Guid.NewGuid(), "Dispatch test location"),
            "Dispatch test summary",
            DateTimeOffset.UtcNow);

        var responder = new ResponderProfile(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Responder One",
            "Field Responder",
            responderStatus);

        dbContext.Incidents.Add(incident);
        dbContext.ResponderProfiles.Add(responder);
        await dbContext.SaveChangesAsync();

        return (incident.Id, responder.Id);
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:RespondrDb"] = $"Server=(localdb)\\MSSQLLocalDB;Database=RespondrDispatchApiTests_{Guid.NewGuid():N};Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True",
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
