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

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

await app.Services.InitializeRespondrDatabaseAsync(applyMigrations);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapGet("/api/health", () => Results.Ok(new
{
    service = "Respondr.Incidents.Api",
    status = "Healthy",
    timestamp = DateTimeOffset.UtcNow
}));

app.MapGet("/api/version", () => Results.Ok(new
{
    name = "Respondr Incidents API",
    version = "v1",
    environment = app.Environment.EnvironmentName
}));

app.MapGet("/api/health/db", async (RespondrDbContext dbContext, CancellationToken cancellationToken) =>
{
    var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

    return canConnect
        ? Results.Ok(new
        {
            service = "Respondr.Incidents.Api",
            database = "RespondrDb",
            status = "Healthy",
            timestamp = DateTimeOffset.UtcNow
        })
        : Results.Problem(
            title: "Database unavailable",
            detail: "The API could not connect to RespondrDb.",
            statusCode: StatusCodes.Status503ServiceUnavailable);
});

app.MapControllers();

app.Run();
