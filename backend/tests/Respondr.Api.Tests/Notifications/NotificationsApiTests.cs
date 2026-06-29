extern alias notificationsapi;

namespace Respondr.Api.Tests.Notifications;

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Respondr.Api.Tests.Support;
using Respondr.Contracts.Notifications;
using Respondr.Domain.Notifications;
using Respondr.Infrastructure.Persistence;
using Respondr.Shared.Common;

public sealed class NotificationsApiTests : IClassFixture<NotificationsApiFactory>
{
    private readonly NotificationsApiFactory _factory;
    private readonly HttpClient _client;

    public NotificationsApiTests(NotificationsApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            TestAuthTokenFactory.CreateToken(NotificationsApiFactory.UserId, "dispatcher@respondr.test", "Dispatcher", "Dispatcher"));
    }

    [Fact]
    public async Task Notifications_list_returns_only_current_user_items()
    {
        await _factory.SeedNotificationsAsync();

        var response = await _client.GetAsync("/api/notifications");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<PagedResult<NotificationResponse>>();
        Assert.NotNull(payload);
        Assert.Single(payload.Items);
    }

    [Fact]
    public async Task Notifications_mark_read_updates_unread_count()
    {
        var notificationId = await _factory.SeedSingleNotificationAsync();

        var markReadResponse = await _client.PatchAsync($"/api/notifications/{notificationId}/read", JsonContent.Create(new { }));
        Assert.Equal(HttpStatusCode.OK, markReadResponse.StatusCode);

        var unreadResponse = await _client.GetAsync("/api/notifications/unread");
        var payload = await unreadResponse.Content.ReadFromJsonAsync<NotificationUnreadResponse>();

        Assert.NotNull(payload);
        Assert.Equal(0, payload.UnreadCount);
    }
}

public sealed class NotificationsApiFactory : WebApplicationFactory<notificationsapi::Program>
{
    public static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public async Task SeedNotificationsAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RespondrDbContext>();
        dbContext.Notifications.RemoveRange(dbContext.Notifications);
        await dbContext.SaveChangesAsync();
        dbContext.Notifications.Add(new Notification(Guid.NewGuid(), UserId, NotificationType.General, "Mine", "Current user notification"));
        dbContext.Notifications.Add(new Notification(Guid.NewGuid(), Guid.NewGuid(), NotificationType.General, "Other", "Other user notification"));
        await dbContext.SaveChangesAsync();
    }

    public async Task<Guid> SeedSingleNotificationAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RespondrDbContext>();
        dbContext.Notifications.RemoveRange(dbContext.Notifications);
        await dbContext.SaveChangesAsync();
        var notification = new Notification(Guid.NewGuid(), UserId, NotificationType.General, "Unread", "Unread notification");
        dbContext.Notifications.Add(notification);
        await dbContext.SaveChangesAsync();
        return notification.Id;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:RespondrDb"] = $"Server=(localdb)\\MSSQLLocalDB;Database=RespondrNotificationsApiTests_{Guid.NewGuid():N};Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True",
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
