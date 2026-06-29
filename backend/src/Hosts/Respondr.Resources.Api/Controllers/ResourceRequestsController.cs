namespace Respondr.Resources.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Respondr.Contracts.Resources;
using Respondr.Domain.Resources;
using Respondr.Infrastructure.Persistence;
using Respondr.Shared.Common;
using Respondr.Shared.Constants;

[ApiController]
[Authorize(Policy = PolicyNames.DispatcherOrOperationsLead)]
public sealed class ResourceRequestsController : ControllerBase
{
    private readonly RespondrDbContext _dbContext;

    public ResourceRequestsController(RespondrDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("api/resource-requests")]
    public async Task<ActionResult<PagedResult<ResourceRequestResponse>>> GetRequests(
        [FromQuery] Guid? incidentId,
        [FromQuery] string? status,
        [FromQuery] string? resourceType,
        [FromQuery] int pageNumber = PaginationRequest.DefaultPageNumber,
        [FromQuery] int pageSize = PaginationRequest.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var pagination = new PaginationRequest(pageNumber, pageSize);
        var requests = _dbContext.ResourceRequests.AsNoTracking().AsQueryable();

        if (incidentId.HasValue)
        {
            requests = requests.Where(item => item.IncidentId == incidentId.Value);
        }

        if (Enum.TryParse<ResourceRequestStatus>(status, true, out var requestStatus))
        {
            requests = requests.Where(item => item.Status == requestStatus);
        }

        if (Enum.TryParse<ResourceType>(resourceType, true, out var parsedResourceType))
        {
            requests = requests.Where(item => item.ResourceType == parsedResourceType);
        }

        var totalCount = await requests.CountAsync(cancellationToken);
        var items = await requests
            .OrderByDescending(item => item.RequestedAt)
            .Skip(pagination.Skip)
            .Take(pagination.PageSize)
            .Select(item => ToResponse(item))
            .ToListAsync(cancellationToken);

        return Ok(PagedResult<ResourceRequestResponse>.Create(items, pagination.PageNumber, pagination.PageSize, totalCount));
    }

    [HttpGet("api/resource-requests/{id:guid}")]
    public async Task<ActionResult<ResourceRequestResponse>> GetRequest(Guid id, CancellationToken cancellationToken)
    {
        var request = await _dbContext.ResourceRequests
            .AsNoTracking()
            .SingleOrDefaultAsync(current => current.Id == id, cancellationToken);

        return request is null ? NotFound() : Ok(ToResponse(request));
    }

    [HttpPost("api/resource-requests")]
    public async Task<ActionResult<ResourceRequestResponse>> CreateRequest(
        [FromBody] CreateResourceRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ResourceType>(request.ResourceType, true, out var resourceType) ||
            resourceType == ResourceType.Unknown)
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["resourceType"] = ["ResourceType is invalid."]
            }));
        }

        if (request.Quantity <= 0 || string.IsNullOrWhiteSpace(request.Justification))
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["request"] = ["Quantity must be greater than zero and justification is required."]
            }));
        }

        var incidentExists = await _dbContext.Incidents.AnyAsync(item => item.Id == request.IncidentId, cancellationToken);
        if (!incidentExists)
        {
            return NotFound(new { message = "Incident not found." });
        }

        var resourceRequest = new ResourceRequest(
            Guid.NewGuid(),
            request.IncidentId,
            resourceType,
            request.Quantity,
            request.Justification.Trim());

        _dbContext.ResourceRequests.Add(resourceRequest);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetRequest), new { id = resourceRequest.Id }, ToResponse(resourceRequest));
    }

    [HttpPatch("api/resource-requests/{id:guid}/approve")]
    [Authorize(Policy = PolicyNames.OperationsLeadOnly)]
    public async Task<ActionResult<ResourceRequestResponse>> ApproveRequest(
        Guid id,
        [FromBody] UpdateResourceRequestDecision request,
        CancellationToken cancellationToken)
    {
        var resourceRequest = await _dbContext.ResourceRequests.SingleOrDefaultAsync(current => current.Id == id, cancellationToken);
        if (resourceRequest is null)
        {
            return NotFound();
        }

        if (!ResourceRequestStatusRules.CanTransition(resourceRequest.Status, ResourceRequestStatus.Approved) &&
            !ResourceRequestStatusRules.CanTransition(resourceRequest.Status, ResourceRequestStatus.Allocated))
        {
            return Conflict(new
            {
                code = "InvalidStatusTransition",
                message = $"Cannot approve request from status {resourceRequest.Status}."
            });
        }

        var availableItems = await _dbContext.ResourceItems
            .Where(item => item.Type == resourceRequest.ResourceType && item.IsAvailable)
            .OrderBy(item => item.Name)
            .Take(resourceRequest.Quantity)
            .ToListAsync(cancellationToken);

        var changedAt = DateTimeOffset.UtcNow;
        if (availableItems.Count == resourceRequest.Quantity)
        {
            foreach (var item in availableItems)
            {
                item.AssignToIncident(resourceRequest.IncidentId, changedAt);
                _dbContext.ResourceAllocations.Add(new ResourceAllocation(Guid.NewGuid(), resourceRequest.Id, item.Id));
            }

            resourceRequest.Allocate(changedAt, request.Reason?.Trim());
        }
        else
        {
            resourceRequest.Approve(changedAt, request.Reason?.Trim());
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Ok(ToResponse(resourceRequest));
    }

    [HttpPatch("api/resource-requests/{id:guid}/reject")]
    [Authorize(Policy = PolicyNames.OperationsLeadOnly)]
    public async Task<ActionResult<ResourceRequestResponse>> RejectRequest(
        Guid id,
        [FromBody] UpdateResourceRequestDecision request,
        CancellationToken cancellationToken)
    {
        var resourceRequest = await _dbContext.ResourceRequests.SingleOrDefaultAsync(current => current.Id == id, cancellationToken);
        if (resourceRequest is null)
        {
            return NotFound();
        }

        if (!ResourceRequestStatusRules.CanTransition(resourceRequest.Status, ResourceRequestStatus.Rejected))
        {
            return Conflict(new
            {
                code = "InvalidStatusTransition",
                message = $"Cannot reject request from status {resourceRequest.Status}."
            });
        }

        resourceRequest.Reject(DateTimeOffset.UtcNow, request.Reason?.Trim());
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(resourceRequest));
    }

    [HttpPatch("api/resource-requests/{id:guid}/cancel")]
    public async Task<ActionResult<ResourceRequestResponse>> CancelRequest(
        Guid id,
        [FromBody] UpdateResourceRequestDecision request,
        CancellationToken cancellationToken)
    {
        var resourceRequest = await _dbContext.ResourceRequests.SingleOrDefaultAsync(current => current.Id == id, cancellationToken);
        if (resourceRequest is null)
        {
            return NotFound();
        }

        if (!ResourceRequestStatusRules.CanTransition(resourceRequest.Status, ResourceRequestStatus.Cancelled))
        {
            return Conflict(new
            {
                code = "InvalidStatusTransition",
                message = $"Cannot cancel request from status {resourceRequest.Status}."
            });
        }

        resourceRequest.Cancel(DateTimeOffset.UtcNow, request.Reason?.Trim());
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToResponse(resourceRequest));
    }

    [HttpGet("api/resources/available")]
    public async Task<ActionResult<IReadOnlyList<AvailableResourceResponse>>> GetAvailableResources(CancellationToken cancellationToken)
    {
        var items = await _dbContext.ResourceItems
            .AsNoTracking()
            .Where(item => item.IsAvailable)
            .OrderBy(item => item.Name)
            .Select(item => new AvailableResourceResponse(
                item.Id,
                item.Name,
                item.Type.ToString(),
                item.IsAvailable,
                item.CurrentIncidentId))
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    private static ResourceRequestResponse ToResponse(ResourceRequest request) =>
        new(
            request.Id,
            request.IncidentId,
            request.ResourceType.ToString(),
            request.Quantity,
            request.Status.ToString(),
            request.RequestedAt,
            request.Justification,
            request.DecisionNotes);
}
