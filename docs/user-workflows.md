# Respondr User Workflows

## Purpose

This document describes how the primary users move through Respondr from login to incident closure. It is written for product planning, UI design, frontend development, backend development, and test planning.

The workflows assume the first version of Respondr has two operational roles:

- Dispatcher
- Operations Lead

## Workflow Principles

Respondr should feel like an operations console. The user should always understand:

- What incidents are active.
- Which incidents are critical.
- Which response units are available.
- Which unit is assigned to which incident.
- What changed recently.
- Whether the system is connected to live updates.

The product should reduce repeated manual communication by keeping the shared operational state current for every logged-in user.

## Roles

### Dispatcher

The Dispatcher is responsible for receiving emergency reports and creating accurate incident records. The Dispatcher may also update incident details as more information arrives.

Primary responsibilities:

- Log in to Respondr.
- View the dashboard.
- Create new incidents.
- Update incident details, priority, and status when appropriate.
- Monitor live updates from the Operations Lead.
- Confirm that incident records remain complete and understandable.

### Operations Lead

The Operations Lead is responsible for coordinating response units and moving incidents toward resolution.

Primary responsibilities:

- Log in to Respondr.
- Review active and critical incidents.
- Check response unit availability.
- Assign units to incidents.
- Monitor incident status and response unit status.
- Update incident state as response work progresses.
- Close incidents when work is complete.

## Workflow 1: Login And Session Start

### Goal

Allow a Dispatcher or Operations Lead to securely access the system and land on the operational dashboard.

### Main Flow

1. User opens Respondr in the browser.
2. User sees the login screen.
3. User enters email and password.
4. Frontend sends login request to the backend.
5. Backend validates credentials.
6. Backend returns authentication result and user profile.
7. Frontend stores the session using the selected authentication approach.
8. Frontend routes the user to the dashboard.
9. Frontend establishes the SignalR connection.
10. User sees current dashboard data and live connection status.

### Expected UI Behavior

- Login errors should be clear and professional.
- Loading states should prevent duplicate login attempts.
- The dashboard should not display protected data before authentication succeeds.
- If SignalR fails to connect, the user should still be able to use the REST API and see a clear degraded-live-status indicator.

### Backend Requirements

- Validate login credentials.
- Return authenticated user identity and role.
- Support authorization checks for protected endpoints.
- Provide enough user profile information for the frontend to display role-specific behavior.

### Test Coverage

- Successful login.
- Invalid credentials.
- Protected route access without a session.
- Role is loaded correctly after login.
- Dashboard initializes after login.

## Workflow 2: Dispatcher Creates A New Incident

### Goal

Allow the Dispatcher to create a new incident quickly and accurately while preserving enough detail for response coordination.

### Main Flow

1. Dispatcher opens the dashboard or incident page.
2. Dispatcher selects Create Incident.
3. Frontend displays the incident creation form.
4. Dispatcher enters required incident information.
5. Dispatcher selects incident priority or severity.
6. Dispatcher submits the form.
7. Frontend validates required fields.
8. Frontend sends the create request to the backend.
9. Backend validates the request.
10. Backend creates the incident with initial status New.
11. Backend records an incident history entry.
12. Backend sends a SignalR event that an incident was created.
13. Frontend updates the incident list and dashboard metrics.
14. Operations Lead receives the live update.

### Required Fields

- Incident type
- Title or short description
- Location description
- Priority or severity
- Reported date/time

### Recommended Optional Fields

- Detailed description
- Reporter name or source
- Contact number
- Notes
- Initial hazards

### Expected UI Behavior

- Required fields should be visually clear.
- The form should avoid unnecessary complexity.
- The user should receive confirmation after creation.
- Newly created critical incidents should stand out in the dashboard and incident list.
- The incident detail view should be available immediately after creation.

### Backend Requirements

- Create a unique incident ID.
- Assign default status New.
- Store who created the incident.
- Store timestamps.
- Persist incident history.
- Publish an IncidentCreated real-time event.

### Test Coverage

- Create valid incident.
- Reject invalid incident.
- Verify initial status is New.
- Verify history is created.
- Verify live update is published.

## Workflow 3: Operations Lead Reviews And Assigns A Response Unit

### Goal

Allow the Operations Lead to choose an available response unit and assign it to an incident.

### Main Flow

1. Operations Lead receives a new incident update or opens the dashboard.
2. Operations Lead opens the incident detail view.
3. Operations Lead reviews priority, location, status, and notes.
4. Operations Lead opens available response units.
5. System shows units that can be assigned.
6. Operations Lead selects a response unit.
7. Frontend sends assignment request to backend.
8. Backend validates that the incident can receive an assignment.
9. Backend validates that the response unit is available.
10. Backend creates an assignment record.
11. Backend updates incident status to Assigned.
12. Backend updates response unit status to Assigned or En Route depending on the selected action.
13. Backend records history entries.
14. Backend publishes SignalR assignment and status events.
15. Dashboard, incident detail, unit list, and metrics update for all users.

### Business Rules

- A closed incident cannot receive a new assignment.
- An unavailable response unit cannot be assigned.
- A response unit already assigned to an active incident should not be assigned to a second incident unless later multi-assignment support is explicitly added.
- Assignment should be auditable.

### Expected UI Behavior

- Available units should be easy to identify.
- Unavailable units should be disabled or clearly marked.
- The user should see a confirmation after assignment.
- The incident timeline should show the assignment event.
- The response unit should no longer appear as available after assignment.

### Backend Requirements

- Transactionally create assignment and update statuses.
- Prevent invalid assignment through backend validation.
- Persist assignment history.
- Publish live updates after successful commit.

### Test Coverage

- Assign available unit.
- Reject unavailable unit.
- Reject assignment to closed incident.
- Verify incident and unit statuses update together.
- Verify SignalR events are published.

## Workflow 4: Incident Status Progression

### Goal

Allow authorized users to move an incident through its operational lifecycle.

### Status Lifecycle

```text
New -> Assigned -> In Progress -> Resolved -> Closed
```

### Main Flow

1. User opens an incident detail page.
2. User selects a new status.
3. Frontend confirms the action when the status change is significant.
4. Frontend sends status update request to backend.
5. Backend validates the transition.
6. Backend updates the incident.
7. Backend records a history entry.
8. Backend publishes a SignalR status event.
9. Frontend updates dashboard metrics, lists, and detail views.

### Status Definitions

| Status | Meaning |
| --- | --- |
| New | Incident has been created but no unit has been assigned. |
| Assigned | One or more response units have been assigned. |
| In Progress | Response work is actively underway. |
| Resolved | Operational response is complete, pending final closure. |
| Closed | Incident is finished and no longer active. |

### Business Rules

- Status transitions should be validated by the backend.
- Closing an incident should require the incident to be resolved first unless an authorized override is added later.
- Status changes should always write an update history record.
- Closed incidents should remain visible in historical views but should not count as active incidents.

### Test Coverage

- Valid status transition.
- Invalid status transition.
- History is recorded.
- Dashboard metrics update.
- Live update reaches connected clients.

## Workflow 5: Response Unit Availability Management

### Goal

Allow the Operations Lead to understand and manage response unit readiness.

### Unit Statuses

```text
Available
Assigned
En Route
On Scene
Unavailable
```

### Main Flow

1. Operations Lead opens the response units page or dashboard capacity panel.
2. System lists response units with current status.
3. Operations Lead filters units by availability, type, or capability.
4. Operations Lead opens a unit detail view.
5. Operations Lead updates unit status when needed.
6. Backend validates the update.
7. Backend persists the status change.
8. Backend publishes a SignalR unit status event.
9. Dashboard capacity and assignment screens update.

### Expected UI Behavior

- Unit status should use clear badges.
- Available units should be visually distinct from unavailable units.
- Unit details should show active assignment if one exists.
- Capacity metrics should update immediately.

### Backend Requirements

- Store unit metadata and status.
- Link active assignments to units.
- Prevent status changes that conflict with active assignments unless explicitly allowed.
- Publish ResponseUnitStatusChanged events.

## Workflow 6: Live Update Handling

### Goal

Keep all active users aligned without requiring manual refresh.

### Main Flow

1. User logs in.
2. Frontend connects to SignalR hub.
3. User loads dashboard data through REST API.
4. Another user creates or updates an incident.
5. Backend commits the change to the database.
6. Backend publishes a SignalR event.
7. Connected clients receive the event.
8. Frontend either applies the event payload directly or refreshes the affected data.
9. UI highlights the change in the relevant panels.

### Expected UI Behavior

- Connection status should be visible somewhere in the application shell.
- Updates should be noticeable but not disruptive.
- Users should not lose form input because of live updates.
- If live updates disconnect, the user should see a reconnecting or offline-live indicator.

## Workflow 7: Incident Resolution And Closure

### Goal

Allow the Operations Lead to complete the operational record and remove the incident from active work.

### Main Flow

1. Operations Lead opens incident detail.
2. Operations Lead reviews status, assigned units, and latest notes.
3. Operations Lead changes status to Resolved.
4. System records resolution timestamp and history.
5. Operations Lead performs final review.
6. Operations Lead changes status to Closed.
7. System releases assigned response units if applicable.
8. System records closure timestamp and history.
9. Incident no longer appears in active lists.
10. Incident remains available in historical or filtered views.

### Business Rules

- Closure should be auditable.
- Closed incidents should not accept normal operational updates.
- Response units should be returned to Available or another configured status after closure.

## Edge Cases And Failure Handling

### Duplicate Incident

If a new report appears to duplicate an existing incident, the first version can allow manual handling. Later versions may add duplicate detection or merge workflows.

### Assignment Conflict

If two Operations Leads try to assign the same unit at the same time, the backend must enforce consistency. The second request should fail with a clear conflict response.

### Lost Real-Time Connection

If SignalR disconnects, REST API actions should still work. The frontend should retry the live connection and refresh critical dashboard data after reconnecting.

### Backend Validation Failure

Frontend validation improves usability, but backend validation is the source of truth. All commands must handle backend errors gracefully.

## Workflow Acceptance Criteria

The workflows are ready for implementation when:

- Each role has a clear default landing experience.
- Incident creation can be completed without leaving the operational context.
- Assignments update incident and unit state together.
- Status changes are persisted and visible in history.
- Live updates reach other connected users.
- Core workflows are covered by backend tests and frontend smoke tests.
