namespace Respondr.Domain.Tests;

using Respondr.Domain.Dispatch;
using Respondr.Domain.Incidents;
using Respondr.Domain.Notifications;
using Respondr.Domain.Resources;
using Respondr.Shared.Constants;

public sealed class DomainDefaultsTests
{
    [Fact]
    public void Incident_defaults_to_new_status()
    {
        var incident = new Incident(
            Guid.NewGuid(),
            "INC-2026-0001",
            "Flood in district 4",
            IncidentType.Flood,
            IncidentPriority.High,
            new IncidentLocation(Guid.NewGuid(), "123 Main St"),
            "Water has reached the first floor.",
            DateTimeOffset.UtcNow);

        Assert.Equal(IncidentStatus.New, incident.Status);
    }

    [Fact]
    public void Notification_defaults_to_unread_state()
    {
        var notification = new Notification(
            Guid.NewGuid(),
            Guid.NewGuid(),
            NotificationType.Incident,
            "Assignment received",
            "You were assigned to INC-2026-0001");

        Assert.Equal(NotificationStatus.Unread, notification.Status);
        Assert.False(notification.IsRead);
    }

    [Fact]
    public void Responder_profile_defaults_to_available()
    {
        var profile = new ResponderProfile(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Alex Morgan",
            "Field Responder");

        Assert.Equal(ResponderStatus.Available, profile.Status);
    }

    [Fact]
    public void Resource_request_defaults_to_pending()
    {
        var request = new ResourceRequest(
            Guid.NewGuid(),
            Guid.NewGuid(),
            ResourceType.Supply,
            3,
            "Need triage kits and bandages");

        Assert.Equal(ResourceRequestStatus.Pending, request.Status);
    }

    [Fact]
    public void Role_names_remain_stable()
    {
        Assert.Equal("Dispatcher", RoleNames.Dispatcher);
        Assert.Equal("OperationsLead", RoleNames.OperationsLead);
    }
}
