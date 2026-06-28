var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapGet("/api/health", () => Results.Ok(new
{
    service = "Respondr.Notifications.Api",
    status = "Healthy",
    timestamp = DateTimeOffset.UtcNow
}));

app.MapGet("/api/version", () => Results.Ok(new
{
    name = "Respondr Notifications API",
    version = "v1",
    environment = app.Environment.EnvironmentName
}));

app.MapControllers();

app.Run();
