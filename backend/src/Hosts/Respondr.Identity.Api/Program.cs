using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Respondr.Application;
using Respondr.Application.Identity;
using Respondr.Contracts.Identity;
using Respondr.Domain.Identity;
using Respondr.Infrastructure;
using Respondr.Infrastructure.Identity;
using Respondr.Infrastructure.Persistence;
using Respondr.Shared.Constants;
using Respondr.Shared.Time;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("RespondrDb")
    ?? throw new InvalidOperationException("Connection string 'RespondrDb' was not found.");
var applyMigrations = builder.Configuration.GetValue("Database:ApplyMigrations", true);
var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var identitySection = builder.Configuration.GetSection(IdentitySeedOptions.SectionName);

builder.Services
    .AddRespondrApplication()
    .AddRespondrInfrastructure(
        connectionString,
        configureJwt: options => jwtSection.Bind(options),
        configureIdentitySeed: options => identitySection.Bind(options));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services
    .AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IConfiguration>((options, configuration) =>
    {
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PolicyNames.DispatcherOnly, policy => policy.RequireRole(RoleNames.Dispatcher));
    options.AddPolicy(PolicyNames.OperationsLeadOnly, policy => policy.RequireRole(RoleNames.OperationsLead));
    options.AddPolicy(
        PolicyNames.DispatcherOrOperationsLead,
        policy => policy.RequireRole(RoleNames.Dispatcher, RoleNames.OperationsLead));
});

var app = builder.Build();

await app.Services.InitializeRespondrDatabaseAsync(applyMigrations);

if (app.Environment.IsDevelopment())
{
    await app.Services.SeedDevelopmentIdentityUsersAsync();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/health", () => Results.Ok(new
{
    service = "Respondr.Identity.Api",
    status = "Healthy",
    timestamp = DateTimeOffset.UtcNow
}));

app.MapGet("/api/version", () => Results.Ok(new
{
    name = "Respondr Identity API",
    version = "v1",
    environment = app.Environment.EnvironmentName
}));

app.MapGet("/api/health/db", async (RespondrDbContext dbContext, CancellationToken cancellationToken) =>
{
    var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

    return canConnect
        ? Results.Ok(new
        {
            service = "Respondr.Identity.Api",
            database = "RespondrDb",
            status = "Healthy",
            timestamp = DateTimeOffset.UtcNow
        })
        : Results.Problem(
            title: "Database unavailable",
            detail: "The API could not connect to RespondrDb.",
            statusCode: StatusCodes.Status503ServiceUnavailable);
});

app.MapPost("/api/auth/register", async (
    RegisterUserRequest request,
    HttpContext httpContext,
    RespondrDbContext dbContext,
    IPasswordHasherService passwordHasher,
    IConfiguration configuration,
    CancellationToken cancellationToken) =>
{
    var email = request.Email.Trim();
    var password = request.Password.Trim();
    var fullName = request.FullName.Trim();
    var roleName = request.Role.Trim();

    if (string.IsNullOrWhiteSpace(email) ||
        string.IsNullOrWhiteSpace(password) ||
        string.IsNullOrWhiteSpace(fullName) ||
        string.IsNullOrWhiteSpace(roleName))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["request"] = ["Email, password, full name, and role are required."]
        });
    }

    if (password.Length < 8)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["password"] = ["Password must be at least 8 characters long."]
        });
    }

    if (roleName != RoleNames.Dispatcher && roleName != RoleNames.OperationsLead)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["role"] = ["Role must be Dispatcher or OperationsLead."]
        });
    }

    var hasExistingUsers = await dbContext.Users.AnyAsync(cancellationToken);
    var allowOpenRegistration = configuration.GetValue("Identity:AllowOpenRegistration", false);
    var isOperationsLead = httpContext.User.IsInRole(RoleNames.OperationsLead);

    if (hasExistingUsers && !allowOpenRegistration && !isOperationsLead)
    {
        return Results.Forbid();
    }

    var existingUser = await dbContext.Users.SingleOrDefaultAsync(user => user.Email == email, cancellationToken);
    if (existingUser is not null)
    {
        return Results.Conflict(new
        {
            message = "A user with that email already exists."
        });
    }

    var role = await dbContext.Roles.SingleOrDefaultAsync(r => r.Name == roleName, cancellationToken);
    if (role is null)
    {
        role = new Role(Guid.NewGuid(), roleName);
        dbContext.Roles.Add(role);
    }

    var user = new User(
        Guid.NewGuid(),
        email,
        passwordHasher.HashPassword(password),
        fullName,
        role.Id);

    dbContext.Users.Add(user);
    await dbContext.SaveChangesAsync(cancellationToken);

    return Results.Created($"/api/auth/users/{user.Id}", new UserProfileResponse(
        user.Id,
        user.Email,
        user.FullName,
        role.Name));
});

app.MapPost("/api/auth/login", async (
    LoginRequest request,
    RespondrDbContext dbContext,
    IPasswordHasherService passwordHasher,
    IJwtTokenService jwtTokenService,
    IDateTimeProvider dateTimeProvider,
    IConfiguration configuration,
    CancellationToken cancellationToken) =>
{
    var user = await dbContext.Users
        .Include(currentUser => currentUser.Role)
        .SingleOrDefaultAsync(currentUser => currentUser.Email == request.Email.Trim(), cancellationToken);

    if (user is null ||
        !user.IsActive ||
        !passwordHasher.VerifyPassword(user.PasswordHash, request.Password))
    {
        return Results.Json(
            new { message = "Invalid email or password." },
            statusCode: StatusCodes.Status401Unauthorized);
    }

    var roleName = user.Role?.Name ?? RoleNames.Dispatcher;
    var tokenResult = jwtTokenService.CreateAccessToken(user, roleName);
    var refreshTokenLifetime = configuration.GetValue("Jwt:RefreshTokenDays", 7);
    var refreshTokenValue = jwtTokenService.CreateRefreshToken();

    var refreshToken = new RefreshToken(
        Guid.NewGuid(),
        user.Id,
        refreshTokenValue,
        dateTimeProvider.UtcNow.AddDays(refreshTokenLifetime));

    dbContext.RefreshTokens.Add(refreshToken);
    await dbContext.SaveChangesAsync(cancellationToken);

    return Results.Ok(new LoginResponse(
        tokenResult.AccessToken,
        refreshToken.Token,
        tokenResult.ExpiresAt,
        new UserProfileResponse(user.Id, user.Email, user.FullName, roleName)));
});

app.MapPost("/api/auth/refresh", async (
    RefreshTokenRequest request,
    RespondrDbContext dbContext,
    IJwtTokenService jwtTokenService,
    IDateTimeProvider dateTimeProvider,
    IConfiguration configuration,
    CancellationToken cancellationToken) =>
{
    var refreshToken = await dbContext.RefreshTokens
        .SingleOrDefaultAsync(token => token.Token == request.RefreshToken.Trim(), cancellationToken);

    if (refreshToken is null ||
        refreshToken.IsRevoked ||
        refreshToken.ExpiresAt <= dateTimeProvider.UtcNow)
    {
        return Results.Json(
            new { message = "Invalid or expired refresh token." },
            statusCode: StatusCodes.Status401Unauthorized);
    }

    var user = await dbContext.Users
        .Include(currentUser => currentUser.Role)
        .SingleOrDefaultAsync(currentUser => currentUser.Id == refreshToken.UserId, cancellationToken);

    if (user is null || !user.IsActive)
    {
        return Results.Json(
            new { message = "Invalid or expired refresh token." },
            statusCode: StatusCodes.Status401Unauthorized);
    }

    refreshToken.Revoke(dateTimeProvider.UtcNow);

    var roleName = user.Role?.Name ?? RoleNames.Dispatcher;
    var tokenResult = jwtTokenService.CreateAccessToken(user, roleName);
    var refreshTokenLifetime = configuration.GetValue("Jwt:RefreshTokenDays", 7);
    var nextRefreshToken = new RefreshToken(
        Guid.NewGuid(),
        user.Id,
        jwtTokenService.CreateRefreshToken(),
        dateTimeProvider.UtcNow.AddDays(refreshTokenLifetime));

    dbContext.RefreshTokens.Add(nextRefreshToken);
    await dbContext.SaveChangesAsync(cancellationToken);

    return Results.Ok(new LoginResponse(
        tokenResult.AccessToken,
        nextRefreshToken.Token,
        tokenResult.ExpiresAt,
        new UserProfileResponse(user.Id, user.Email, user.FullName, roleName)));
});

app.MapGet("/api/auth/me", async (
    ClaimsPrincipal principal,
    RespondrDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var userIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);
    if (!Guid.TryParse(userIdValue, out var userId))
    {
        return Results.Unauthorized();
    }

    var user = await dbContext.Users
        .Include(currentUser => currentUser.Role)
        .SingleOrDefaultAsync(currentUser => currentUser.Id == userId, cancellationToken);

    if (user is null || !user.IsActive)
    {
        return Results.Unauthorized();
    }

    return Results.Ok(new UserProfileResponse(
        user.Id,
        user.Email,
        user.FullName,
        user.Role?.Name ?? string.Empty));
})
    .RequireAuthorization(PolicyNames.DispatcherOrOperationsLead);

app.Run();

public partial class Program;
