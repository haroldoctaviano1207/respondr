namespace Respondr.Dispatch.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Respondr.Contracts.Dispatch;
using Respondr.Domain.Dispatch;
using Respondr.Infrastructure.Persistence;
using Respondr.Shared.Common;
using Respondr.Shared.Constants;

[ApiController]
[Route("api/responders")]
[Authorize(Policy = PolicyNames.DispatcherOrOperationsLead)]
public sealed class RespondersController : ControllerBase
{
    private readonly RespondrDbContext _dbContext;

    public RespondersController(RespondrDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ResponderProfileResponse>>> GetResponders(
        [FromQuery] string? status,
        [FromQuery] string? search,
        [FromQuery] int pageNumber = PaginationRequest.DefaultPageNumber,
        [FromQuery] int pageSize = PaginationRequest.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var pagination = new PaginationRequest(pageNumber, pageSize);
        var responders = _dbContext.ResponderProfiles.AsNoTracking().AsQueryable();

        if (Enum.TryParse<ResponderStatus>(status, true, out var responderStatus))
        {
            responders = responders.Where(responder => responder.Status == responderStatus);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            responders = responders.Where(responder =>
                responder.DisplayName.Contains(normalizedSearch) ||
                responder.ResponderType.Contains(normalizedSearch));
        }

        var totalCount = await responders.CountAsync(cancellationToken);
        var items = await responders
            .OrderBy(responder => responder.DisplayName)
            .Skip(pagination.Skip)
            .Take(pagination.PageSize)
            .Select(responder => ToResponse(responder))
            .ToListAsync(cancellationToken);

        return Ok(PagedResult<ResponderProfileResponse>.Create(items, pagination.PageNumber, pagination.PageSize, totalCount));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ResponderProfileResponse>> GetResponder(Guid id, CancellationToken cancellationToken)
    {
        var responder = await _dbContext.ResponderProfiles
            .AsNoTracking()
            .SingleOrDefaultAsync(current => current.Id == id, cancellationToken);

        return responder is null ? NotFound() : Ok(ToResponse(responder));
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = PolicyNames.OperationsLeadOnly)]
    public async Task<ActionResult<ResponderProfileResponse>> UpdateResponderStatus(
        Guid id,
        [FromBody] UpdateResponderStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ResponderStatus>(request.Status, true, out var nextStatus))
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["status"] = ["Status is invalid."]
            }));
        }

        var responder = await _dbContext.ResponderProfiles
            .SingleOrDefaultAsync(current => current.Id == id, cancellationToken);

        if (responder is null)
        {
            return NotFound();
        }

        var hasActiveAssignment = await _dbContext.DispatchAssignments.AnyAsync(
            assignment => assignment.ResponderProfileId == responder.Id &&
                assignment.Status != AssignmentStatus.Released &&
                assignment.Status != AssignmentStatus.Completed &&
                assignment.Status != AssignmentStatus.Cancelled,
            cancellationToken);

        if (hasActiveAssignment && nextStatus == ResponderStatus.Available)
        {
            return Conflict(new
            {
                code = "ResponderHasActiveAssignment",
                message = "Responder cannot be marked Available while an active assignment exists."
            });
        }

        responder.UpdateStatus(nextStatus, DateTimeOffset.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(responder));
    }

    private static ResponderProfileResponse ToResponse(ResponderProfile responder) =>
        new(
            responder.Id,
            responder.UserId,
            responder.DisplayName,
            responder.ResponderType,
            responder.Status.ToString(),
            responder.CurrentIncidentId);
}
