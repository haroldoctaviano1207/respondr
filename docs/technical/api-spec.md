# Respondr API Design

## Purpose

This document defines the planned backend API surface for the Angular frontend. It focuses on the first version of Respondr: authentication, dashboard data, incidents, response units, assignments, and live update support.

The API will be implemented as a .NET Web API using C# and Entity Framework Core.

## API Principles

- REST endpoints handle commands and queries.
- SignalR handles live update delivery.
- Backend validation is the source of truth.
- Responses should be predictable and typed.
- Errors should use consistent response formats.
- Endpoints should be role-protected where required.
- API routes should be versionable or easy to version later.

## Base Route

Recommended base path:

```text
/api/v1
```

## Authentication

The final auth mechanism can be selected during implementation. For planning, assume the frontend can log in and call protected endpoints using a backend-issued token or secure cookie.

### POST /api/v1/auth/login

Logs a user in.

Request:

```json
{
  "email": "dispatcher@example.com",
  "password": "password"
}
```

Response:

```json
{
  "user": {
    "id": "user-id",
    "email": "dispatcher@example.com",
    "displayName": "Dispatcher User",
    "role": "Dispatcher"
  },
  "accessToken": "token-if-token-auth-is-used"
}
```

### POST /api/v1/auth/logout

Logs the user out or invalidates the active session.

### GET /api/v1/auth/me

Returns the current authenticated user.

Response:

```json
{
  "id": "user-id",
  "email": "lead@example.com",
  "displayName": "Operations Lead",
  "role": "OperationsLead"
}
```

## Dashboard

### GET /api/v1/dashboard/summary

Returns dashboard metrics and recent operational activity.

Response:

```json
{
  "activeIncidents": 12,
  "criticalIncidents": 3,
  "availableUnits": 8,
  "assignedUnits": 5,
  "recentUpdates": [
    {
      "incidentId": "incident-id",
      "incidentNumber": "INC-2026-0001",
      "message": "Rescue Team A assigned",
      "createdAtUtc": "2026-06-28T01:00:00Z"
    }
  ]
}
```

### GET /api/v1/dashboard/active-work

Returns data needed to render the operational dashboard.

Suggested response includes:

- Active incident summaries.
- Critical incident summaries.
- Response unit capacity.
- Recent incident updates.
- Current live connection metadata if useful.

## Incidents

### GET /api/v1/incidents

Returns a paginated and filterable list of incidents.

Query parameters:

| Name | Description |
| --- | --- |
| status | Filter by incident status. |
| priority | Filter by priority. |
| type | Filter by incident type. |
| search | Search incident number, title, location, or description. |
| page | Page number. |
| pageSize | Page size. |
| sort | Sort field. |

Response:

```json
{
  "items": [
    {
      "id": "incident-id",
      "incidentNumber": "INC-2026-0001",
      "title": "Flooding near river road",
      "type": "Flood",
      "priority": "Critical",
      "status": "New",
      "locationText": "River Road, Tampa, FL",
      "reportedAtUtc": "2026-06-28T01:00:00Z",
      "assignedUnitCount": 0
    }
  ],
  "page": 1,
  "pageSize": 25,
  "totalCount": 1
}
```

### GET /api/v1/incidents/{id}

Returns incident detail.

Response should include:

- Incident core fields.
- Active assignments.
- Timeline/history.
- Created by user.
- Last updated information.

### POST /api/v1/incidents

Creates a new incident.

Request:

```json
{
  "title": "Flooding near river road",
  "type": "Flood",
  "description": "Water rising near residential street.",
  "locationText": "River Road, Tampa, FL",
  "priority": "Critical",
  "reportedAtUtc": "2026-06-28T01:00:00Z"
}
```

Response:

```json
{
  "id": "incident-id",
  "incidentNumber": "INC-2026-0001",
  "status": "New"
}
```

Expected side effects:

- Incident record is created.
- IncidentUpdate record is created.
- SignalR IncidentCreated event is published.

### PUT /api/v1/incidents/{id}

Updates editable incident details.

Editable fields may include:

- Title
- Type
- Description
- LocationText
- Priority

Status changes should use a dedicated endpoint.

### PATCH /api/v1/incidents/{id}/status

Updates incident status.

Request:

```json
{
  "status": "InProgress",
  "note": "Response unit arrived and started assessment."
}
```

Expected side effects:

- Incident status is updated.
- Relevant timestamp is set when status becomes Resolved or Closed.
- IncidentUpdate record is created.
- SignalR IncidentStatusChanged event is published.

### PATCH /api/v1/incidents/{id}/priority

Updates incident priority.

Request:

```json
{
  "priority": "Critical",
  "note": "Escalated due to worsening conditions."
}
```

### POST /api/v1/incidents/{id}/notes

Adds an operational note to the incident timeline.

Request:

```json
{
  "message": "Caller reports water level has reached first floor."
}
```

## Response Units

### GET /api/v1/response-units

Returns response units with optional filters.

Query parameters:

| Name | Description |
| --- | --- |
| status | Available, Assigned, EnRoute, OnScene, Unavailable. |
| unitType | Rescue, Medical, Fire, Relief, Utility, Security, Other. |
| search | Search name, capability, home base. |

### GET /api/v1/response-units/{id}

Returns response unit detail, including active assignment if present.

### POST /api/v1/response-units

Creates a response unit.

This may be limited to administrative users in a later version.

### PUT /api/v1/response-units/{id}

Updates response unit metadata.

### PATCH /api/v1/response-units/{id}/status

Updates response unit status.

Request:

```json
{
  "status": "Unavailable",
  "note": "Vehicle maintenance."
}
```

Expected side effects:

- Unit status is updated.
- History entry may be created.
- SignalR ResponseUnitStatusChanged event is published.

## Assignments

### GET /api/v1/assignments

Returns assignments with optional filters.

Query parameters:

| Name | Description |
| --- | --- |
| incidentId | Filter by incident. |
| responseUnitId | Filter by response unit. |
| status | Active, Completed, Cancelled. |

### POST /api/v1/incidents/{incidentId}/assignments

Assigns a response unit to an incident.

Request:

```json
{
  "responseUnitId": "response-unit-id",
  "note": "Assign Rescue Team A to initial response."
}
```

Expected side effects:

- Assignment is created.
- Incident status becomes Assigned if it was New.
- Response unit status becomes Assigned or EnRoute based on implementation choice.
- IncidentUpdate record is created.
- SignalR UnitAssigned event is published.
- SignalR IncidentStatusChanged or ResponseUnitStatusChanged may be published.

### PATCH /api/v1/assignments/{id}/release

Releases a response unit from an incident.

Request:

```json
{
  "note": "Unit released after incident resolved."
}
```

Expected side effects:

- Assignment status changes to Completed.
- Response unit returns to Available unless another status is specified.
- IncidentUpdate record is created.
- SignalR AssignmentReleased event is published.

## Incident History

### GET /api/v1/incidents/{id}/updates

Returns timeline entries for an incident.

Response:

```json
{
  "items": [
    {
      "id": "update-id",
      "updateType": "Assigned",
      "message": "Rescue Team A assigned.",
      "createdByDisplayName": "Operations Lead",
      "createdAtUtc": "2026-06-28T01:10:00Z"
    }
  ]
}
```

## Error Response Format

Recommended error format:

```json
{
  "traceId": "trace-id",
  "code": "ValidationFailed",
  "message": "The request could not be processed.",
  "errors": {
    "priority": ["Priority is required."]
  }
}
```

Common codes:

- ValidationFailed
- Unauthorized
- Forbidden
- NotFound
- Conflict
- InvalidStatusTransition
- UnitUnavailable
- UnexpectedError

## Authorization Matrix

| Action | Dispatcher | Operations Lead |
| --- | --- | --- |
| View dashboard | Yes | Yes |
| View incidents | Yes | Yes |
| Create incident | Yes | Yes |
| Update incident details | Yes | Yes |
| Assign response unit | No or limited | Yes |
| Release response unit | No or limited | Yes |
| Update unit status | No or limited | Yes |
| Resolve incident | Limited | Yes |
| Close incident | No or limited | Yes |

Exact permissions can be refined during implementation, but the backend must enforce the selected rules.

## Pagination Standard

For list endpoints, use:

- page
- pageSize
- totalCount
- items

The frontend should avoid loading large unbounded lists.

## Date And Time Standard

- Store and return UTC timestamps.
- Use ISO 8601 strings.
- Let the frontend format timestamps for display.

## API Implementation Notes

- Use DTOs rather than exposing EF entities directly.
- Use validation classes or validators for command requests.
- Keep controllers thin.
- Put business rules in services or application layer handlers.
- Publish SignalR events only after successful database commits.
- Add integration tests for high-risk endpoints.
