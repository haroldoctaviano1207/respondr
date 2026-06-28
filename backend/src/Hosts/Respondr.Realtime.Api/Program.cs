var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

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

app.Run();
