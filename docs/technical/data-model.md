# Respondr Data Design

## Purpose

This document describes the main data entities, relationships, lifecycle fields, and data rules for Respondr. It is intended to guide backend Entity Framework Core modeling, database schema design, API contracts, frontend state models, and tests.

## Database Direction

The first version may use SQL Server or PostgreSQL. The backend should use Entity Framework Core so that most data access code remains provider-neutral.

Provider-specific differences should be isolated in configuration, migrations, or infrastructure code.

## Core Data Concepts

Respondr stores operational data around five main concepts:

- Users
- Incidents
- Response Units
- Assignments
- Incident Updates / History

Supporting concepts may include roles, refresh tokens or sessions, lookup values, and audit metadata.

## Entity Relationship Summary

```text
User
  -> creates Incident
  -> updates Incident
  -> creates Assignment
  -> creates IncidentUpdate

Incident
  -> has many Assignments
  -> has many IncidentUpdates

ResponseUnit
  -> has many Assignments

Assignment
  -> belongs to Incident
  -> belongs to ResponseUnit
  -> created by User

IncidentUpdate
  -> belongs to Incident
  -> created by User
```

## Entity: User

### Purpose

Represents a person who can log in to Respondr and perform role-based actions.

### Suggested Fields

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| Id | Guid or long | Yes | Primary key. |
| Email | string | Yes | Unique login identifier. |
| PasswordHash | string | Yes | Required if using local auth. |
| DisplayName | string | Yes | Name shown in UI and history. |
| Role | enum/string | Yes | Dispatcher or OperationsLead. |
| IsActive | bool | Yes | Controls access without deleting user. |
| CreatedAtUtc | datetime | Yes | Audit field. |
| UpdatedAtUtc | datetime | Yes | Audit field. |

### Roles

| Role | Description |
| --- | --- |
| Dispatcher | Can create incidents and update incident details. |
| OperationsLead | Can assign units, update operational status, and close incidents. |

The backend should enforce role permissions. The frontend may hide actions for usability, but hidden UI is not security.

## Entity: Incident

### Purpose

Represents an emergency report or operational event that requires tracking and response.

### Suggested Fields

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| Id | Guid or long | Yes | Primary key. |
| IncidentNumber | string | Yes | Human-readable ID such as INC-2026-0001. |
| Title | string | Yes | Short label for lists and dashboards. |
| Type | enum/string | Yes | Flood, Fire, Medical, Utility, Security, Other, etc. |
| Description | string | No | Operational detail. |
| LocationText | string | Yes | Human-readable location. |
| Priority | enum/string | Yes | Low, Medium, High, Critical. |
| Status | enum/string | Yes | New, Assigned, InProgress, Resolved, Closed. |
| ReportedAtUtc | datetime | Yes | When the incident was reported. |
| CreatedByUserId | FK | Yes | User who created the record. |
| CreatedAtUtc | datetime | Yes | Audit field. |
| UpdatedAtUtc | datetime | Yes | Audit field. |
| ResolvedAtUtc | datetime | No | Set when status becomes Resolved. |
| ClosedAtUtc | datetime | No | Set when status becomes Closed. |

### Incident Status Values

| Status | Meaning |
| --- | --- |
| New | Created but no unit assigned. |
| Assigned | At least one response unit assigned. |
| InProgress | Response work is underway. |
| Resolved | Operational response completed, pending closure. |
| Closed | Incident record finalized. |

### Priority Values

| Priority | Meaning |
| --- | --- |
| Low | Can be handled after higher-priority incidents. |
| Medium | Requires attention but is not immediately critical. |
| High | Requires prompt attention. |
| Critical | Requires immediate attention and should be highlighted. |

## Entity: ResponseUnit

### Purpose

Represents a team, vehicle, or resource that can be assigned to an incident.

### Suggested Fields

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| Id | Guid or long | Yes | Primary key. |
| Name | string | Yes | Example: Rescue Team A. |
| UnitType | enum/string | Yes | Rescue, Medical, Fire, Relief, Utility, Security, Other. |
| Status | enum/string | Yes | Available, Assigned, EnRoute, OnScene, Unavailable. |
| Capabilities | string or related table | No | Searchable capabilities. |
| HomeBase | string | No | Optional base or staging area. |
| Notes | string | No | Operational notes. |
| IsActive | bool | Yes | Allows deactivation without deletion. |
| CreatedAtUtc | datetime | Yes | Audit field. |
| UpdatedAtUtc | datetime | Yes | Audit field. |

### Response Unit Status Values

| Status | Meaning |
| --- | --- |
| Available | Unit can be assigned. |
| Assigned | Unit is assigned but may not be moving yet. |
| EnRoute | Unit is traveling to the incident. |
| OnScene | Unit has arrived. |
| Unavailable | Unit cannot be assigned. |

## Entity: Assignment

### Purpose

Links a response unit to an incident and tracks assignment lifecycle.

### Suggested Fields

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| Id | Guid or long | Yes | Primary key. |
| IncidentId | FK | Yes | Assigned incident. |
| ResponseUnitId | FK | Yes | Assigned unit. |
| AssignedByUserId | FK | Yes | User who made assignment. |
| Status | enum/string | Yes | Active, Completed, Cancelled. |
| AssignedAtUtc | datetime | Yes | Assignment timestamp. |
| ReleasedAtUtc | datetime | No | Set when assignment ends. |
| Notes | string | No | Optional assignment notes. |

### Assignment Rules

- Active assignments connect one incident to one response unit.
- A unit should not have more than one active assignment in the first version.
- A closed incident should not accept a new assignment.
- Assignment creation should update incident and unit status in the same transaction.

## Entity: IncidentUpdate

### Purpose

Stores the history of operational changes and notes for an incident.

### Suggested Fields

| Field | Type | Required | Notes |
| --- | --- | --- | --- |
| Id | Guid or long | Yes | Primary key. |
| IncidentId | FK | Yes | Related incident. |
| CreatedByUserId | FK | Yes | User who created update. |
| UpdateType | enum/string | Yes | Created, StatusChanged, PriorityChanged, Assigned, NoteAdded, Closed. |
| Message | string | Yes | Human-readable timeline entry. |
| OldValue | string | No | Optional previous value. |
| NewValue | string | No | Optional new value. |
| CreatedAtUtc | datetime | Yes | Timeline timestamp. |

### Update History Rules

Create a history entry when:

- An incident is created.
- An incident status changes.
- Incident priority changes.
- A response unit is assigned.
- A response unit is released.
- An operational note is added.
- An incident is resolved or closed.

## Suggested Lookup Enums

These may be implemented as C# enums, lookup tables, or constrained strings depending on implementation preference.

### IncidentType

- Flood
- Fire
- Medical
- Utility
- Security
- Traffic
- Other

### IncidentPriority

- Low
- Medium
- High
- Critical

### IncidentStatus

- New
- Assigned
- InProgress
- Resolved
- Closed

### ResponseUnitType

- Rescue
- Medical
- Fire
- Relief
- Utility
- Security
- Other

### ResponseUnitStatus

- Available
- Assigned
- EnRoute
- OnScene
- Unavailable

### AssignmentStatus

- Active
- Completed
- Cancelled

## Audit Fields

Most operational tables should include:

- CreatedAtUtc
- UpdatedAtUtc

Important user-generated records should include:

- CreatedByUserId
- UpdatedByUserId when useful

Use UTC timestamps in storage. The frontend can display local time.

## Data Integrity Requirements

### Incident Number

IncidentNumber should be unique. It should be readable by users and stable after creation.

### Assignment Consistency

Assignment creation must be transactional. If the assignment is created but the unit status fails to update, the transaction should roll back.

### Optimistic Concurrency

Consider adding a row version or concurrency token to Incident and ResponseUnit. This helps prevent lost updates when multiple users work at the same time.

### Soft Delete

Operational records should generally not be hard deleted. Prefer IsActive flags or status changes so history remains intact.

## Indexing Recommendations

Suggested indexes:

- User.Email unique index.
- Incident.IncidentNumber unique index.
- Incident.Status.
- Incident.Priority.
- Incident.CreatedAtUtc.
- ResponseUnit.Status.
- ResponseUnit.UnitType.
- Assignment.IncidentId.
- Assignment.ResponseUnitId.
- Assignment.Status.
- IncidentUpdate.IncidentId and CreatedAtUtc.

## Frontend Model Considerations

The Angular frontend will likely use models such as:

- UserDto
- LoginRequest
- LoginResponse
- IncidentSummaryDto
- IncidentDetailDto
- CreateIncidentRequest
- UpdateIncidentStatusRequest
- ResponseUnitDto
- AssignmentDto
- IncidentUpdateDto
- RealtimeEventDto

Summary DTOs should be optimized for tables and dashboard cards. Detail DTOs should include timeline, assignment, and responder information.

## Migration Strategy

When implementation begins:

1. Create EF Core entities.
2. Configure relationships and constraints.
3. Add initial migration.
4. Add development seed data for users, incidents, and response units.
5. Verify database creation in Docker Compose.
6. Add integration tests for critical persistence rules.
