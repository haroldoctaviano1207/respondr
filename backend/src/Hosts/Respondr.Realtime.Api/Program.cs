using Respondr.Application;
using Respondr.Infrastructure;
using Respondr.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("RespondrDb")
    ?? throw new InvalidOperationException("Connection string 'RespondrDb' was not found.");
var applyMigrations = builder.Configuration.GetValue("Database:ApplyMigrations", true);

builder.Services
    .AddRespondrApplication()
    .AddRespondrInfrastructure(connectionString);

var app = builder.Build();

await app.Services.InitializeRespondrDatabaseAsync(applyMigrations);

app.MapGet("/api/health", () => Results.Ok(new
{
    service = "Respondr.Realtime.Api",
    status = "Healthy",
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

app.Run();
