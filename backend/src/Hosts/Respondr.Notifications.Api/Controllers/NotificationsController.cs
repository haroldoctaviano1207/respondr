namespace Respondr.Notifications.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Respondr.Application.Realtime;
using Respondr.Contracts.Notifications;
using Respondr.Domain.Notifications;
using Respondr.Infrastructure.Persistence;
using Respondr.Shared.Common;
using Respondr.Shared.Constants;
using Respondr.Shared.Security;

[ApiController]
[Route("api/notifications")]
[Authorize(Policy = PolicyNames.DispatcherOrOperationsLead)]
public sealed class NotificationsController : ControllerBase
{
    private readonly RespondrDbContext _dbContext;
    private readonly IRealtimePublisher _realtimePublisher;

    public NotificationsController(RespondrDbContext dbContext, IRealtimePublisher realtimePublisher)
    {
        _dbContext = dbContext;
        _realtimePublisher = realtimePublisher;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<NotificationResponse>>> GetNotifications(
        [FromQuery] bool? unreadOnly,
        [FromQuery] int pageNumber = PaginationRequest.DefaultPageNumber,
        [FromQuery] int pageSize = PaginationRequest.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = User.GetUserId();
        if (!currentUserId.HasValue)
        {
            return Unauthorized();
        }

        var pagination = new PaginationRequest(pageNumber, pageSize);
        var notifications = _dbContext.Notifications
            .AsNoTracking()
            .Where(notification => notification.UserId == currentUserId.Value);

        if (unreadOnly == true)
        {
            notifications = notifications.Where(notification => !notification.IsRead);
        }

        var totalCount = await notifications.CountAsync(cancellationToken);
        var items = await notifications
            .OrderByDescending(notification => notification.CreatedAt)
            .Skip(pagination.Skip)
            .Take(pagination.PageSize)
            .Select(notification => ToResponse(notification))
            .ToListAsync(cancellationToken);

        return Ok(PagedResult<NotificationResponse>.Create(items, pagination.PageNumber, pagination.PageSize, totalCount));
    }

    [HttpGet("unread")]
    public async Task<ActionResult<NotificationUnreadResponse>> GetUnreadCount(CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        if (!currentUserId.HasValue)
        {
            return Unauthorized();
        }

        var unreadCount = await _dbContext.Notifications.CountAsync(
            notification => notification.UserId == currentUserId.Value && !notification.IsRead,
            cancellationToken);

        return Ok(new NotificationUnreadResponse(unreadCount));
    }

    [HttpPost]
    [Authorize(Policy = PolicyNames.OperationsLeadOnly)]
    public async Task<ActionResult<NotificationResponse>> CreateNotification(
        [FromBody] CreateNotificationRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Type) ||
            !Enum.TryParse<NotificationType>(request.Type, true, out var notificationType))
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["type"] = ["Type is invalid."]
            }));
        }

        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["request"] = ["Title and message are required."]
            }));
        }

        var notification = new Notification(
            Guid.NewGuid(),
            request.UserId,
            notificationType,
            request.Title.Trim(),
            request.Message.Trim(),
            request.RelatedIncidentId);

        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _realtimePublisher.PublishNotificationsAsync(
            "NotificationCreated",
            new NotificationCreatedEvent(
                notification.Id,
                notification.UserId,
                notification.Type.ToString(),
                notification.Title,
                notification.CreatedAt),
            cancellationToken);

        return CreatedAtAction(nameof(GetNotifications), new { }, ToResponse(notification));
    }

    [HttpPatch("{id:guid}/read")]
    public async Task<ActionResult<NotificationResponse>> MarkRead(Guid id, CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        if (!currentUserId.HasValue)
        {
            return Unauthorized();
        }

        var notification = await _dbContext.Notifications.SingleOrDefaultAsync(
            item => item.Id == id && item.UserId == currentUserId.Value,
            cancellationToken);

        if (notification is null)
        {
            return NotFound();
        }

        notification.MarkAsRead(DateTimeOffset.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(notification));
    }

    [HttpPatch("read-all")]
    public async Task<ActionResult<NotificationUnreadResponse>> MarkAllRead(CancellationToken cancellationToken)
    {
        var currentUserId = User.GetUserId();
        if (!currentUserId.HasValue)
        {
            return Unauthorized();
        }

        var notifications = await _dbContext.Notifications
            .Where(item => item.UserId == currentUserId.Value && !item.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var notification in notifications)
        {
            notification.MarkAsRead(DateTimeOffset.UtcNow);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(new NotificationUnreadResponse(0));
    }

    private static NotificationResponse ToResponse(Notification notification) =>
        new(
            notification.Id,
            notification.Type.ToString(),
            notification.Title,
            notification.Message,
            notification.IsRead,
            notification.CreatedAt);
}
