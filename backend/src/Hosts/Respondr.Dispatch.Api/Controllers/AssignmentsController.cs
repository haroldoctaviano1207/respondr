namespace Respondr.Dispatch.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Respondr.Application.Realtime;
using Respondr.Contracts.Dispatch;
using Respondr.Contracts.Incidents;
using Respondr.Domain.Dispatch;
using Respondr.Domain.Incidents;
using Respondr.Infrastructure.Persistence;
using Respondr.Shared.Common;
using Respondr.Shared.Constants;
using Respondr.Shared.Security;

[ApiController]
[Authorize(Policy = PolicyNames.DispatcherOrOperationsLead)]
public sealed class AssignmentsController : ControllerBase
{
    private readonly RespondrDbContext _dbContext;
    private readonly IRealtimePublisher _realtimePublisher;

    public AssignmentsController(RespondrDbContext dbContext, IRealtimePublisher realtimePublisher)
    {
        _dbContext = dbContext;
        _realtimePublisher = realtimePublisher;
    }

    [HttpGet("api/assignments")]
    public async Task<ActionResult<PagedResult<DispatchAssignmentResponse>>> GetAssignments(
        [FromQuery] Guid? incidentId,
        [FromQuery] Guid? responderId,
        [FromQuery] string? status,
        [FromQuery] int pageNumber = PaginationRequest.DefaultPageNumber,
        [FromQuery] int pageSize = PaginationRequest.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var pagination = new PaginationRequest(pageNumber, pageSize);
        var assignments = _dbContext.DispatchAssignments.AsNoTracking().AsQueryable();

        if (incidentId.HasValue)
        {
            assignments = assignments.Where(item => item.IncidentId == incidentId.Value);
        }

        if (responderId.HasValue)
        {
            assignments = assignments.Where(item => item.ResponderProfileId == responderId.Value);
        }

        if (Enum.TryParse<AssignmentStatus>(status, true, out var assignmentStatus))
        {
            assignments = assignments.Where(item => item.Status == assignmentStatus);
        }

        var totalCount = await assignments.CountAsync(cancellationToken);
        var items = await assignments
            .OrderByDescending(item => item.AssignedAt)
            .Skip(pagination.Skip)
            .Take(pagination.PageSize)
            .Select(item => ToResponse(item))
            .ToListAsync(cancellationToken);

        return Ok(PagedResult<DispatchAssignmentResponse>.Create(items, pagination.PageNumber, pagination.PageSize, totalCount));
    }

    [HttpGet("api/assignments/{id:guid}")]
    public async Task<ActionResult<DispatchAssignmentResponse>> GetAssignment(Guid id, CancellationToken cancellationToken)
    {
        var assignment = await _dbContext.DispatchAssignments
            .AsNoTracking()
            .SingleOrDefaultAsync(current => current.Id == id, cancellationToken);

        return assignment is null ? NotFound() : Ok(ToResponse(assignment));
    }

    [HttpPost("api/incidents/{incidentId:guid}/assignments")]
    [Authorize(Policy = PolicyNames.OperationsLeadOnly)]
    public async Task<ActionResult<DispatchAssignmentResponse>> AssignResponder(
        Guid incidentId,
        [FromBody] AssignResponderRequest request,
        CancellationToken cancellationToken)
    {
        var incident = await _dbContext.Incidents.SingleOrDefaultAsync(current => current.Id == incidentId, cancellationToken);
        if (incident is null)
        {
            return NotFound(new { message = "Incident not found." });
        }

        var responder = await _dbContext.ResponderProfiles.SingleOrDefaultAsync(current => current.Id == request.ResponderId, cancellationToken);
        if (responder is null)
        {
            return NotFound(new { message = "Responder not found." });
        }

        var hasActiveAssignment = await _dbContext.DispatchAssignments.AnyAsync(
            assignment => assignment.ResponderProfileId == responder.Id &&
                assignment.Status != AssignmentStatus.Released &&
                assignment.Status != AssignmentStatus.Completed &&
                assignment.Status != AssignmentStatus.Cancelled,
            cancellationToken);

        if (!DispatchAssignmentRules.CanAssign(incident.Status, responder.Status, hasActiveAssignment))
        {
            return Conflict(new
            {
                code = "InvalidAssignment",
                message = "Responder cannot be assigned to this incident."
            });
        }

        var assignment = new DispatchAssignment(
            Guid.NewGuid(),
            incident.Id,
            responder.Id,
            User.GetUserId() ?? Guid.Empty);

        var changedAt = DateTimeOffset.UtcNow;
        responder.AssignToIncident(incident.Id, ResponderStatus.Assigned, changedAt);

        var previousIncidentStatus = incident.Status;
        if (incident.Status == IncidentStatus.New)
        {
            incident.UpdateStatus(IncidentStatus.Assigned, changedAt);
        }

        _dbContext.DispatchAssignments.Add(assignment);
        _dbContext.IncidentHistories.Add(new IncidentHistory(
            Guid.NewGuid(),
            incident.Id,
            "ResponderAssigned",
            $"{responder.DisplayName} was assigned to incident {incident.IncidentNumber}.",
            User.GetUserId()));

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _realtimePublisher.PublishDispatchAsync(
            "ResponderAssigned",
            new ResponderAssignedEvent(
                assignment.Id,
                incident.Id,
                responder.Id,
                assignment.Status.ToString(),
                assignment.AssignedAt),
            cancellationToken);

        if (previousIncidentStatus != incident.Status)
        {
            await _realtimePublisher.PublishIncidentsAsync(
                "IncidentStatusChanged",
                new IncidentStatusChangedEvent(
                    incident.Id,
                    incident.IncidentNumber,
                    previousIncidentStatus.ToString(),
                    incident.Status.ToString(),
                    changedAt),
                cancellationToken);
        }

        return CreatedAtAction(nameof(GetAssignment), new { id = assignment.Id }, ToResponse(assignment));
    }

    [HttpPatch("api/assignments/{id:guid}/status")]
    [Authorize(Policy = PolicyNames.OperationsLeadOnly)]
    public async Task<ActionResult<DispatchAssignmentResponse>> UpdateAssignmentStatus(
        Guid id,
        [FromBody] UpdateAssignmentStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<AssignmentStatus>(request.Status, true, out var nextStatus))
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["status"] = ["Status is invalid."]
            }));
        }

        var assignment = await _dbContext.DispatchAssignments.SingleOrDefaultAsync(current => current.Id == id, cancellationToken);
        if (assignment is null)
        {
            return NotFound();
        }

        assignment.UpdateStatus(nextStatus, DateTimeOffset.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _realtimePublisher.PublishDispatchAsync(
            "AssignmentStatusChanged",
            new AssignmentStatusChangedEvent(
                assignment.Id,
                assignment.IncidentId,
                assignment.ResponderProfileId,
                assignment.Status.ToString(),
                DateTimeOffset.UtcNow),
            cancellationToken);

        return Ok(ToResponse(assignment));
    }

    [HttpPatch("api/assignments/{id:guid}/release")]
    [Authorize(Policy = PolicyNames.OperationsLeadOnly)]
    public async Task<ActionResult<DispatchAssignmentResponse>> ReleaseAssignment(Guid id, CancellationToken cancellationToken)
    {
        var assignment = await _dbContext.DispatchAssignments.SingleOrDefaultAsync(current => current.Id == id, cancellationToken);
        if (assignment is null)
        {
            return NotFound();
        }

        var responder = await _dbContext.ResponderProfiles.SingleOrDefaultAsync(current => current.Id == assignment.ResponderProfileId, cancellationToken);
        if (responder is null)
        {
            return NotFound(new { message = "Responder not found." });
        }

        var incident = await _dbContext.Incidents.SingleOrDefaultAsync(current => current.Id == assignment.IncidentId, cancellationToken);
        if (incident is null)
        {
            return NotFound(new { message = "Incident not found." });
        }

        var changedAt = DateTimeOffset.UtcNow;
        assignment.Release(changedAt);
        responder.ReleaseFromIncident(changedAt);

        var hasRemainingAssignments = await _dbContext.DispatchAssignments.AnyAsync(
            current => current.IncidentId == incident.Id &&
                current.Id != assignment.Id &&
                current.Status != AssignmentStatus.Released &&
                current.Status != AssignmentStatus.Completed &&
                current.Status != AssignmentStatus.Cancelled,
            cancellationToken);

        if (!hasRemainingAssignments && incident.Status == IncidentStatus.Assigned)
        {
            incident.UpdateStatus(IncidentStatus.New, changedAt);
        }

        _dbContext.IncidentHistories.Add(new IncidentHistory(
            Guid.NewGuid(),
            incident.Id,
            "AssignmentReleased",
            $"{responder.DisplayName} was released from incident {incident.IncidentNumber}.",
            User.GetUserId()));

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _realtimePublisher.PublishDispatchAsync(
            "AssignmentStatusChanged",
            new AssignmentStatusChangedEvent(
                assignment.Id,
                assignment.IncidentId,
                assignment.ResponderProfileId,
                assignment.Status.ToString(),
                changedAt),
            cancellationToken);

        return Ok(ToResponse(assignment));
    }

    private static DispatchAssignmentResponse ToResponse(DispatchAssignment assignment) =>
        new(
            assignment.Id,
            assignment.IncidentId,
            assignment.ResponderProfileId,
            assignment.Status.ToString(),
            assignment.AssignedAt);
}
