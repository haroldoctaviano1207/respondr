# Respondr Realtime Design

## Purpose

This document explains how Respondr will deliver live updates when incidents, response units, assignments, or operational status changes.

Respondr will use SignalR for real-time communication between the .NET backend and the Angular frontend.

## Goals

The real-time layer should:

- Notify users when an incident is created.
- Notify users when incident details, priority, or status change.
- Notify users when a response unit is assigned or released.
- Notify users when response unit status changes.
- Keep dashboard metrics and lists current without manual refresh.
- Provide connection status so users know whether live updates are active.

## Non-Goals

The real-time layer should not:

- Replace REST API commands.
- Become the primary data persistence mechanism.
- Carry large full-dashboard payloads for every event.
- Allow unauthenticated clients.
- Bypass backend authorization.

## SignalR Hub

Recommended hub route:

```text
/hubs/operations
```

Suggested hub name in backend code:

```text
OperationsHub
```

## Client Connection Flow

1. User logs in.
2. Angular stores or receives authentication context.
3. Angular loads initial dashboard data through REST API.
4. Angular connects to `/hubs/operations`.
5. Backend authenticates the SignalR connection.
6. Client joins relevant groups if group support is used.
7. Client listens for operational events.
8. Client updates state or refetches data after events arrive.

## Event Publishing Rule

SignalR events should be published after the database transaction has committed successfully.

Reason:

- Clients should never receive an event for a change that failed to persist.
- Clients can safely refetch the changed entity after receiving the event.

## Recommended Events

### IncidentCreated

Sent when a new incident is created.

Payload:

```json
{
  "eventType": "IncidentCreated",
  "incidentId": "incident-id",
  "incidentNumber": "INC-2026-0001",
  "title": "Flooding near river road",
  "priority": "Critical",
  "status": "New",
  "createdAtUtc": "2026-06-28T01:00:00Z",
  "createdByDisplayName": "Dispatcher User"
}
```

Frontend behavior:

- Add or refresh the incident in active lists.
- Refresh dashboard metrics.
- Show notification if priority is High or Critical.

### IncidentUpdated

Sent when incident details change.

Payload:

```json
{
  "eventType": "IncidentUpdated",
  "incidentId": "incident-id",
  "incidentNumber": "INC-2026-0001",
  "changedFields": ["locationText", "description"],
  "updatedAtUtc": "2026-06-28T01:05:00Z"
}
```

Frontend behavior:

- Refresh incident detail if currently open.
- Refresh visible row if incident is in a table.

### IncidentStatusChanged

Sent when incident status changes.

Payload:

```json
{
  "eventType": "IncidentStatusChanged",
  "incidentId": "incident-id",
  "incidentNumber": "INC-2026-0001",
  "oldStatus": "Assigned",
  "newStatus": "InProgress",
  "changedAtUtc": "2026-06-28T01:10:00Z",
  "changedByDisplayName": "Operations Lead"
}
```

Frontend behavior:

- Update incident status badge.
- Refresh dashboard metrics.
- Refresh incident timeline if detail view is open.

### IncidentPriorityChanged

Sent when incident priority changes.

Payload:

```json
{
  "eventType": "IncidentPriorityChanged",
  "incidentId": "incident-id",
  "incidentNumber": "INC-2026-0001",
  "oldPriority": "High",
  "newPriority": "Critical",
  "changedAtUtc": "2026-06-28T01:12:00Z"
}
```

Frontend behavior:

- Update priority badge.
- Highlight critical changes.
- Refresh dashboard critical count.

### UnitAssigned

Sent when a response unit is assigned to an incident.

Payload:

```json
{
  "eventType": "UnitAssigned",
  "incidentId": "incident-id",
  "incidentNumber": "INC-2026-0001",
  "assignmentId": "assignment-id",
  "responseUnitId": "unit-id",
  "responseUnitName": "Rescue Team A",
  "assignedAtUtc": "2026-06-28T01:15:00Z",
  "assignedByDisplayName": "Operations Lead"
}
```

Frontend behavior:

- Refresh incident assignments.
- Refresh response unit availability.
- Refresh dashboard capacity.
- Add timeline update.

### AssignmentReleased

Sent when a response unit is released from an incident.

Payload:

```json
{
  "eventType": "AssignmentReleased",
  "assignmentId": "assignment-id",
  "incidentId": "incident-id",
  "responseUnitId": "unit-id",
  "releasedAtUtc": "2026-06-28T02:00:00Z"
}
```

Frontend behavior:

- Refresh incident assignments.
- Refresh response unit status.
- Refresh dashboard capacity.

### ResponseUnitStatusChanged

Sent when a response unit status changes.

Payload:

```json
{
  "eventType": "ResponseUnitStatusChanged",
  "responseUnitId": "unit-id",
  "responseUnitName": "Rescue Team A",
  "oldStatus": "Assigned",
  "newStatus": "EnRoute",
  "changedAtUtc": "2026-06-28T01:20:00Z"
}
```

Frontend behavior:

- Update unit status badge.
- Refresh available unit counts.
- Refresh assignment options if the assignment panel is open.

## Event Envelope

To keep the client implementation consistent, events can use a shared envelope.

```json
{
  "eventId": "event-id",
  "eventType": "IncidentCreated",
  "occurredAtUtc": "2026-06-28T01:00:00Z",
  "payload": {}
}
```

Benefits:

- Easier logging.
- Easier client event routing.
- Easier future event versioning.

## Client State Strategy

For the first version, prefer a safe hybrid approach:

- Use event payloads for simple badge/count/list updates.
- Refetch detail data when the currently open record changes.
- Refetch dashboard summary after high-impact events.

This avoids overly complex client-side synchronization while still giving users a live experience.

## Groups And Targeting

The first version can broadcast operational updates to all authenticated users because there are only two roles and a small operational scope.

Future grouping options:

- By role: Dispatchers, OperationsLeads.
- By incident: users viewing a specific incident.
- By region or sector if multi-region support is added.

## Reconnection Behavior

The Angular frontend should:

- Show live connection status.
- Attempt automatic reconnect.
- Disable only features that truly require live updates.
- Continue allowing REST API commands while disconnected.
- Refresh dashboard data after reconnect.

Recommended client states:

- Connected
- Reconnecting
- Disconnected

## Security

SignalR must use the same authentication model as the API.

Requirements:

- Reject unauthenticated connections.
- Do not publish sensitive data to clients that should not see it.
- Validate commands through REST endpoints, not SignalR client messages.
- Avoid putting secrets in event payloads.

## Operational Logging

The backend should log:

- Hub connection started.
- Hub connection ended.
- Publish failures.
- Event type and record IDs.
- Reconnect patterns if observable.

The frontend should log or surface:

- Connection failed.
- Reconnecting.
- Reconnected.
- Live update received but refresh failed.

## Testing Realtime Behavior

Backend tests should cover:

- Event publisher is called after incident creation.
- Event publisher is called after assignment.
- Event publisher is called after status changes.
- Invalid commands do not publish events.

Frontend smoke tests should cover:

- Dashboard shows live connection status.
- Simulated event updates visible incident data.
- Reconnect state is displayed.

## Future Considerations

If Respondr scales beyond a small local deployment, consider:

- Azure SignalR Service.
- Event versioning.
- Per-tenant or per-region SignalR groups.
- Background event processing.
- Outbox pattern for reliable event publication.
