namespace Respondr.Api.Tests.Identity;

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Respondr.Contracts.Identity;

public sealed class IdentityApiTests : IClassFixture<IdentityApiFactory>
{
    private readonly HttpClient _client;

    public IdentityApiTests(IdentityApiFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Login_returns_token_response_for_valid_credentials()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "dispatcher@respondr.test",
            "Dispatcher123!"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(payload.RefreshToken));
        Assert.Equal("dispatcher@respondr.test", payload.User.Email);
        Assert.Equal("Dispatcher", payload.User.Role);
    }

    [Fact]
    public async Task Login_returns_safe_error_for_invalid_credentials()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "dispatcher@respondr.test",
            "NotTheRightPassword"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_returns_current_user_profile_with_valid_token()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "operationslead@respondr.test",
            "OperationsLead123!"));

        var payload = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(payload);

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", payload.AccessToken);

        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var profile = await response.Content.ReadFromJsonAsync<UserProfileResponse>();

        Assert.NotNull(profile);
        Assert.Equal("operationslead@respondr.test", profile.Email);
        Assert.Equal("OperationsLead", profile.Role);
    }

    [Fact]
    public async Task Me_rejects_unauthenticated_requests()
    {
        var response = await _client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_returns_rotated_tokens_for_valid_refresh_token()
    {
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequest(
            "dispatcher@respondr.test",
            "Dispatcher123!"));

        var loginPayload = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(loginPayload);

        var refreshResponse = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(
            loginPayload.RefreshToken));

        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);

        var refreshPayload = await refreshResponse.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(refreshPayload);
        Assert.False(string.IsNullOrWhiteSpace(refreshPayload.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(refreshPayload.RefreshToken));
        Assert.NotEqual(loginPayload.RefreshToken, refreshPayload.RefreshToken);
        Assert.Equal("dispatcher@respondr.test", refreshPayload.User.Email);
    }

    [Fact]
    public async Task Refresh_returns_safe_error_for_invalid_token()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(
            "not-a-valid-refresh-token"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}

public sealed class IdentityApiFactory : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:RespondrDb"] = $"Server=(localdb)\\MSSQLLocalDB;Database=RespondrIdentityApiTests_{Guid.NewGuid():N};Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True",
                ["Database:ApplyMigrations"] = "true",
                ["Jwt:Issuer"] = "respondr.tests",
                ["Jwt:Audience"] = "respondr.clients",
                ["Jwt:Key"] = "Respondr_Test_Key_12345678901234567890",
                ["Jwt:AccessTokenMinutes"] = "60",
                ["Jwt:RefreshTokenDays"] = "7",
                ["Identity:SeedDevelopmentUsers"] = "true",
                ["Identity:AllowOpenRegistration"] = "false",
                ["Identity:DispatcherEmail"] = "dispatcher@respondr.test",
                ["Identity:DispatcherPassword"] = "Dispatcher123!",
                ["Identity:OperationsLeadEmail"] = "operationslead@respondr.test",
                ["Identity:OperationsLeadPassword"] = "OperationsLead123!"
            });
        });

        return base.CreateHost(builder);
    }
}
