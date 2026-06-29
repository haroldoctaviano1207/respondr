namespace Respondr.Incidents.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Respondr.Contracts.Common;
using Respondr.Application.Realtime;
using Respondr.Contracts.Incidents;
using Respondr.Domain.Incidents;
using Respondr.Infrastructure.Persistence;
using Respondr.Shared.Common;
using Respondr.Shared.Constants;
using Respondr.Shared.Security;

[ApiController]
[Route("api/incidents")]
[Authorize(Policy = PolicyNames.DispatcherOrOperationsLead)]
public sealed class IncidentsController : ControllerBase
{
    private readonly RespondrDbContext _dbContext;
    private readonly IRealtimePublisher _realtimePublisher;

    public IncidentsController(RespondrDbContext dbContext, IRealtimePublisher realtimePublisher)
    {
        _dbContext = dbContext;
        _realtimePublisher = realtimePublisher;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<IncidentResponse>>> GetIncidents(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] string? priority,
        [FromQuery] string? type,
        [FromQuery] int pageNumber = PaginationRequest.DefaultPageNumber,
        [FromQuery] int pageSize = PaginationRequest.DefaultPageSize,
        CancellationToken cancellationToken = default)
    {
        var pagination = new PaginationRequest(pageNumber, pageSize);

        var incidents = _dbContext.Incidents
            .Include(incident => incident.Location)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            incidents = incidents.Where(incident =>
                incident.IncidentNumber.Contains(normalizedSearch) ||
                incident.Title.Contains(normalizedSearch) ||
                incident.Location.Address.Contains(normalizedSearch) ||
                incident.SituationSummary.Contains(normalizedSearch));
        }

        if (TryParseEnum<IncidentStatus>(status, out var incidentStatus))
        {
            incidents = incidents.Where(incident => incident.Status == incidentStatus);
        }

        if (TryParseEnum<IncidentPriority>(priority, out var incidentPriority))
        {
            incidents = incidents.Where(incident => incident.Priority == incidentPriority);
        }

        if (TryParseEnum<IncidentType>(type, out var incidentType))
        {
            incidents = incidents.Where(incident => incident.Type == incidentType);
        }

        var totalCount = await incidents.CountAsync(cancellationToken);
        var items = await incidents
            .OrderByDescending(incident => incident.ReportedAt)
            .Skip(pagination.Skip)
            .Take(pagination.PageSize)
            .Select(incident => ToIncidentResponse(incident))
            .ToListAsync(cancellationToken);

        return Ok(PagedResult<IncidentResponse>.Create(items, pagination.PageNumber, pagination.PageSize, totalCount));
    }

    [HttpGet("lookups")]
    public ActionResult<IncidentLookupsResponse> GetLookups()
    {
        return Ok(new IncidentLookupsResponse(
            Enum.GetValues<IncidentType>()
                .Where(value => value != IncidentType.Unknown)
                .Select(value => new LookupOptionResponse(value.ToString(), value.ToString()))
                .ToArray(),
            Enum.GetValues<IncidentPriority>()
                .Select(value => new LookupOptionResponse(value.ToString(), value.ToString()))
                .ToArray(),
            Enum.GetValues<IncidentStatus>()
                .Select(value => new LookupOptionResponse(value.ToString(), value.ToString()))
                .ToArray()));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<IncidentDetailResponse>> GetIncident(Guid id, CancellationToken cancellationToken)
    {
        var incident = await _dbContext.Incidents
            .Include(current => current.Location)
            .AsNoTracking()
            .SingleOrDefaultAsync(current => current.Id == id, cancellationToken);

        if (incident is null)
        {
            return NotFound();
        }

        var history = await _dbContext.IncidentHistories
            .AsNoTracking()
            .Where(item => item.IncidentId == id)
            .OrderByDescending(item => item.CreatedAt)
            .Select(item => new IncidentHistoryResponse(
                item.Id,
                item.EventType,
                item.Description,
                item.CreatedByUserId,
                item.CreatedAt))
            .ToListAsync(cancellationToken);

        return Ok(new IncidentDetailResponse(
            incident.Id,
            incident.IncidentNumber,
            incident.Title,
            incident.Type.ToString(),
            incident.Priority.ToString(),
            incident.Status.ToString(),
            incident.Location.Address,
            incident.SituationSummary,
            incident.ReportedAt,
            history));
    }

    [HttpPost]
    public async Task<ActionResult<IncidentResponse>> CreateIncident(
        [FromBody] CreateIncidentRequest request,
        CancellationToken cancellationToken)
    {
        var validationErrors = ValidateIncidentRequest(request.Title, request.Type, request.Priority, request.Location, request.SituationSummary, request.ReportedAt);
        if (validationErrors.Count > 0)
        {
            return BadRequest(new ValidationProblemDetails(validationErrors));
        }

        var location = new IncidentLocation(Guid.NewGuid(), request.Location.Trim());
        var incident = new Incident(
            Guid.NewGuid(),
            GenerateIncidentNumber(),
            request.Title.Trim(),
            Enum.Parse<IncidentType>(request.Type.Trim(), true),
            Enum.Parse<IncidentPriority>(request.Priority.Trim(), true),
            location,
            request.SituationSummary.Trim(),
            request.ReportedAt);

        var createdByUserId = User.GetUserId();
        var history = new IncidentHistory(
            Guid.NewGuid(),
            incident.Id,
            "Created",
            $"Incident {incident.IncidentNumber} was created.",
            createdByUserId);

        _dbContext.Incidents.Add(incident);
        _dbContext.IncidentHistories.Add(history);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _realtimePublisher.PublishIncidentsAsync(
            "IncidentCreated",
            new IncidentCreatedEvent(
                incident.Id,
                incident.IncidentNumber,
                incident.Title,
                incident.Priority.ToString(),
                incident.Status.ToString(),
                incident.CreatedAt),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetIncident),
            new { id = incident.Id },
            ToIncidentResponse(incident));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<IncidentResponse>> UpdateIncident(
        Guid id,
        [FromBody] UpdateIncidentRequest request,
        CancellationToken cancellationToken)
    {
        var validationErrors = ValidateIncidentRequest(request.Title, request.Type, null, request.Location, request.SituationSummary, request.ReportedAt);
        if (validationErrors.Count > 0)
        {
            return BadRequest(new ValidationProblemDetails(validationErrors));
        }

        var incident = await _dbContext.Incidents
            .Include(current => current.Location)
            .SingleOrDefaultAsync(current => current.Id == id, cancellationToken);

        if (incident is null)
        {
            return NotFound();
        }

        if (!TryParseEnum<IncidentType>(request.Type, out var incidentType))
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["type"] = ["Type is invalid."]
            }));
        }

        var updatedAt = DateTimeOffset.UtcNow;
        incident.Location.UpdateAddress(request.Location.Trim());
        incident.UpdateDetails(
            request.Title.Trim(),
            incidentType,
            incident.Location,
            request.SituationSummary.Trim(),
            request.ReportedAt,
            updatedAt);

        _dbContext.IncidentHistories.Add(new IncidentHistory(
            Guid.NewGuid(),
            incident.Id,
            "Updated",
            "Incident details were updated.",
            User.GetUserId()));

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(ToIncidentResponse(incident));
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = PolicyNames.OperationsLeadOnly)]
    public async Task<ActionResult<IncidentResponse>> UpdateStatus(
        Guid id,
        [FromBody] UpdateIncidentStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryParseEnum<IncidentStatus>(request.Status, out var nextStatus))
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["status"] = ["Status is invalid."]
            }));
        }

        var incident = await _dbContext.Incidents
            .Include(current => current.Location)
            .SingleOrDefaultAsync(current => current.Id == id, cancellationToken);

        if (incident is null)
        {
            return NotFound();
        }

        if (!IncidentStatusTransitionRules.CanTransition(incident.Status, nextStatus))
        {
            return Conflict(new
            {
                code = "InvalidStatusTransition",
                message = $"Cannot transition incident from {incident.Status} to {nextStatus}."
            });
        }

        var previousStatus = incident.Status;
        var updatedAt = DateTimeOffset.UtcNow;
        incident.UpdateStatus(nextStatus, updatedAt);

        _dbContext.IncidentHistories.Add(new IncidentHistory(
            Guid.NewGuid(),
            incident.Id,
            "StatusChanged",
            string.IsNullOrWhiteSpace(request.Note)
                ? $"Incident status changed from {previousStatus} to {nextStatus}."
                : $"Incident status changed from {previousStatus} to {nextStatus}. Note: {request.Note.Trim()}",
            User.GetUserId()));

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _realtimePublisher.PublishIncidentsAsync(
            "IncidentStatusChanged",
            new IncidentStatusChangedEvent(
                incident.Id,
                incident.IncidentNumber,
                previousStatus.ToString(),
                incident.Status.ToString(),
                updatedAt),
            cancellationToken);

        return Ok(ToIncidentResponse(incident));
    }

    [HttpPatch("{id:guid}/priority")]
    [Authorize(Policy = PolicyNames.OperationsLeadOnly)]
    public async Task<ActionResult<IncidentResponse>> UpdatePriority(
        Guid id,
        [FromBody] UpdateIncidentPriorityRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryParseEnum<IncidentPriority>(request.Priority, out var nextPriority))
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["priority"] = ["Priority is invalid."]
            }));
        }

        var incident = await _dbContext.Incidents
            .Include(current => current.Location)
            .SingleOrDefaultAsync(current => current.Id == id, cancellationToken);

        if (incident is null)
        {
            return NotFound();
        }

        var previousPriority = incident.Priority;
        var updatedAt = DateTimeOffset.UtcNow;
        incident.UpdatePriority(nextPriority, updatedAt);

        _dbContext.IncidentHistories.Add(new IncidentHistory(
            Guid.NewGuid(),
            incident.Id,
            "PriorityChanged",
            string.IsNullOrWhiteSpace(request.Note)
                ? $"Incident priority changed from {previousPriority} to {nextPriority}."
                : $"Incident priority changed from {previousPriority} to {nextPriority}. Note: {request.Note.Trim()}",
            User.GetUserId()));

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _realtimePublisher.PublishIncidentsAsync(
            "IncidentPriorityChanged",
            new IncidentPriorityChangedEvent(
                incident.Id,
                incident.IncidentNumber,
                previousPriority.ToString(),
                incident.Priority.ToString(),
                updatedAt),
            cancellationToken);

        return Ok(ToIncidentResponse(incident));
    }

    private static IncidentResponse ToIncidentResponse(Incident incident) =>
        new(
            incident.Id,
            incident.IncidentNumber,
            incident.Title,
            incident.Type.ToString(),
            incident.Priority.ToString(),
            incident.Status.ToString(),
            incident.Location.Address,
            incident.SituationSummary,
            incident.ReportedAt);

    private static Dictionary<string, string[]> ValidateIncidentRequest(
        string title,
        string type,
        string? priority,
        string location,
        string situationSummary,
        DateTimeOffset reportedAt)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(title))
        {
            errors["title"] = ["Title is required."];
        }

        if (!TryParseEnum<IncidentType>(type, out var parsedType) || parsedType == IncidentType.Unknown)
        {
            errors["type"] = ["Type is invalid."];
        }

        if (priority is not null &&
            !TryParseEnum<IncidentPriority>(priority, out _))
        {
            errors["priority"] = ["Priority is invalid."];
        }

        if (string.IsNullOrWhiteSpace(location))
        {
            errors["location"] = ["Location is required."];
        }

        if (string.IsNullOrWhiteSpace(situationSummary))
        {
            errors["situationSummary"] = ["Situation summary is required."];
        }

        if (reportedAt == default)
        {
            errors["reportedAt"] = ["ReportedAt is required."];
        }

        return errors;
    }

    private static bool TryParseEnum<TEnum>(string? value, out TEnum parsedValue)
        where TEnum : struct, Enum =>
        Enum.TryParse(value?.Trim(), true, out parsedValue);

    private static string GenerateIncidentNumber() =>
        $"INC-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..28];
}
