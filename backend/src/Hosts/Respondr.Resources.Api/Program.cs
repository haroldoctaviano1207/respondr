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
    service = "Respondr.Resources.Api",
    status = "Healthy",
    timestamp = DateTimeOffset.UtcNow
}));

app.MapGet("/api/version", () => Results.Ok(new
{
    name = "Respondr Resources API",
    version = "v1",
    environment = app.Environment.EnvironmentName
}));

app.MapControllers();

app.Run();
