using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Respondr.Application;
using Respondr.Contracts.Realtime;
using Respondr.Infrastructure;
using Respondr.Infrastructure.Authentication;
using Respondr.Infrastructure.Persistence;
using Respondr.Infrastructure.Realtime;
using Respondr.Realtime.Api.Hubs;
using Respondr.Realtime.Api.Services;
using Respondr.Shared.Constants;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("RespondrDb")
    ?? throw new InvalidOperationException("Connection string 'RespondrDb' was not found.");
var applyMigrations = builder.Configuration.GetValue("Database:ApplyMigrations", true);
var realtimeSection = builder.Configuration.GetSection(RealtimeApiOptions.SectionName);

builder.Services
    .AddRespondrApplication()
    .AddRespondrInfrastructure(
        connectionString,
        configureJwt: options => builder.Configuration.GetSection("Jwt").Bind(options),
        configureRealtime: options => realtimeSection.Bind(options));

builder.Services.AddRespondrApiAuthentication(builder.Configuration);
builder.Services.AddSignalR();
builder.Services.AddSingleton<RealtimeHubDispatcher>();

var app = builder.Build();

await app.Services.InitializeRespondrDatabaseAsync(applyMigrations);

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/health", () => Results.Ok(new
{
    service = "Respondr.Realtime.Api",
    status = "Healthy",
    timestamp = DateTimeOffset.UtcNow
}));

app.MapGet("/api/realtime/health", () => Results.Ok(new
{
    service = "Respondr.Realtime.Api",
    status = "Healthy",
    realtime = "Connected",
    timestamp = DateTimeOffset.UtcNow
}));

app.MapGet("/api/version", () => Results.Ok(new
{
    name = "Respondr Realtime API",
    version = "v1",
    environment = app.Environment.EnvironmentName
}));

app.MapGet("/api/health/db", async (RespondrDbContext dbContext, CancellationToken cancellationToken) =>
{
    var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

    return canConnect
        ? Results.Ok(new
        {
            service = "Respondr.Realtime.Api",
            database = "RespondrDb",
            status = "Healthy",
            timestamp = DateTimeOffset.UtcNow
        })
        : Results.Problem(
            title: "Database unavailable",
            detail: "The API could not connect to RespondrDb.",
            statusCode: StatusCodes.Status503ServiceUnavailable);
});

app.MapPost("/api/internal/realtime/publish", async (
    PublishRealtimeEventRequest request,
    HttpContext httpContext,
    RealtimeHubDispatcher dispatcher,
    IOptions<RealtimeApiOptions> options,
    CancellationToken cancellationToken) =>
{
    if (!httpContext.Request.Headers.TryGetValue(HeaderNames.InternalApiKey, out var providedApiKey) ||
        !string.Equals(providedApiKey, options.Value.InternalApiKey, StringComparison.Ordinal))
    {
        return Results.Unauthorized();
    }

    try
    {
        await dispatcher.DispatchAsync(request, cancellationToken);
        return Results.Accepted();
    }
    catch (InvalidOperationException exception)
    {
        return Results.BadRequest(new
        {
            message = exception.Message
        });
    }
})
    .AllowAnonymous();

app.MapHub<IncidentsHub>("/hubs/incidents")
    .RequireAuthorization(PolicyNames.DispatcherOrOperationsLead);

app.MapHub<DispatchHub>("/hubs/dispatch")
    .RequireAuthorization(PolicyNames.DispatcherOrOperationsLead);

app.MapHub<NotificationsHub>("/hubs/notifications")
    .RequireAuthorization(PolicyNames.DispatcherOrOperationsLead);

app.Run();

public partial class Program;
