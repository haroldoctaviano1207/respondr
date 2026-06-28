# Respondr CI/CD

## Purpose

This document defines the deployment pipeline checks for Respondr. Testing and linting run as part of CI/CD so each deployment protects the highest-risk workflows: login, incident creation, response unit assignment, incident status changes, and live updates.

## Pipeline Checks

The deployment pipeline should run these checks before releasing:

- Restore backend and frontend dependencies.
- Lint and format-check frontend code.
- Format-check backend code.
- Run backend unit and integration tests.
- Run frontend unit tests where useful.
- Run frontend smoke tests for critical workflows.
- Build backend API hosts.
- Build the Angular frontend.
- Build Docker images.
- Deploy only after required checks pass.

## Testing Goals

CI/CD checks should provide confidence that:

- Users can authenticate.
- Role-based access works.
- Incidents can be created and updated.
- Response units can be assigned safely.
- Invalid operations are rejected.
- Operational history is recorded.
- SignalR events are published after successful changes.
- The frontend can complete critical workflows.
- The application can start in local/containerized environments.

## Test Types

| Test Type | Location | Tooling | Purpose |
| --- | --- | --- | --- |
| Backend unit tests | `tests/backend` | xUnit | Validate business rules and services. |
| Backend integration tests | `tests/backend` | xUnit + test database or test container | Validate API, EF Core, persistence, and transactions. |
| Frontend unit tests | `tests/frontend` or Angular project | Angular test tooling | Validate components and services where useful. |
| Frontend smoke tests | `tests/frontend` | Playwright | Validate critical browser workflows. |
| Realtime tests | Backend and frontend | xUnit, mocked hub, Playwright where practical | Validate SignalR publishing and client behavior. |

## Backend Testing

### Unit Test Focus

Use xUnit for backend unit tests.

High-value unit test areas:

- Incident status transition rules.
- Response unit assignment rules.
- Priority update rules.
- Request validation.
- Authorization policy behavior where practical.
- Event publishing decisions.
- Incident number generation if custom logic exists.

### Integration Test Focus

Integration tests should verify that the API, database, and EF Core configuration work together.

High-value integration tests:

- Create incident through API.
- Assign response unit through API.
- Reject unavailable unit assignment.
- Update incident status through API.
- Verify incident history is persisted.
- Verify response unit status changes after assignment.
- Verify closed incident cannot receive new assignment.

### Backend Test Examples

#### Incident Creation

Given a valid Dispatcher user and a valid incident request:

- API returns success.
- Incident is stored.
- Incident status is New.
- Incident history contains Created entry.
- IncidentCreated event is published.

#### Assignment

Given an active incident and an available unit:

- API creates assignment.
- Incident status becomes Assigned.
- Unit status becomes Assigned or EnRoute.
- History contains assignment entry.
- UnitAssigned event is published.

#### Assignment Conflict

Given a unit already assigned to an active incident:

- Second assignment request is rejected.
- No duplicate active assignment is created.
- No success event is published.

## Frontend Testing

### Unit Test Focus

Frontend unit tests should be used selectively for logic-heavy pieces.

Good candidates:

- API services.
- SignalR service wrapper.
- State mapping functions.
- Guards.
- Form validators.
- Status badge helpers.

Avoid spending too much effort on brittle presentation-only tests early.

### Smoke Test Focus

Use Playwright smoke tests for critical end-to-end browser behavior.

Recommended smoke tests:

- User can log in.
- Dashboard loads.
- Incident list is visible.
- User can create an incident.
- User can open incident detail.
- Operations Lead can assign a unit.
- Incident status badge updates.
- Live connection indicator appears.

## Realtime Testing

Realtime behavior is important but can become difficult to test if overcomplicated.

Recommended approach:

- Backend unit tests verify that event publisher methods are called after successful commands.
- Backend tests verify failed commands do not publish events.
- Frontend tests mock SignalR events and verify UI reaction.
- One smoke test can verify the client shows live connection status.

Future enhancement:

- Multi-browser Playwright test where one session creates an incident and another session sees it appear.

## Test Data

Use predictable seed data for development and tests.

Suggested seed users:

| Role | Email |
| --- | --- |
| Dispatcher | dispatcher@respondr.local |
| Operations Lead | lead@respondr.local |

Suggested seed units:

- Rescue Team A
- Medical Team D
- Relief Truck C
- Fire Truck B

Suggested seed incidents:

- Critical flood incident
- High medical incident
- Medium utility incident

## Test Environment Strategy

### Local Tests

Developers should be able to run backend and frontend tests locally.

### Docker-Based Tests

When Docker setup is added, integration tests should be able to run against a disposable database.

### CI Tests

CI should eventually run:

1. Backend restore/build.
2. Backend unit tests.
3. Backend integration tests.
4. Frontend install/build.
5. Frontend unit tests where present.
6. Playwright smoke tests where practical.

## Acceptance Criteria By Feature

### Auth

- Valid login succeeds.
- Invalid login fails.
- Protected endpoints reject anonymous requests.
- Role information is returned correctly.

### Dashboard

- Summary endpoint returns expected counts.
- Active incident count excludes closed incidents.
- Critical count reflects priority.
- Frontend dashboard renders major panels.

### Incidents

- Create incident.
- Edit incident.
- Change status.
- Change priority.
- Add note.
- View history.

### Response Units

- List units.
- Filter available units.
- Update status.
- Prevent invalid assignment status.

### Assignments

- Assign available unit.
- Reject unavailable unit.
- Release unit.
- Verify incident and unit state changes.

### Realtime

- Publish event after incident creation.
- Publish event after assignment.
- Publish event after status change.
- Frontend handles simulated event.

## Quality Gates

Before merging implementation work:

- Relevant backend tests pass.
- Frontend builds successfully.
- No known critical workflow regression exists.
- New business rules include tests.
- API changes are reflected in docs when needed.

Before deployment:

- Backend tests pass in CI.
- Frontend build passes in CI.
- Smoke tests pass against deployed environment.
- Database migration process is verified.
- SignalR connection is verified in the target environment.

## Manual QA Checklist

Manual testing should be lightweight but structured.

Checklist:

- Login as Dispatcher.
- Create a critical incident.
- Confirm incident appears on dashboard.
- Login as Operations Lead.
- Assign available response unit.
- Confirm Dispatcher sees update.
- Move incident to In Progress.
- Resolve incident.
- Close incident.
- Confirm closed incident no longer appears in active count.

## Testing Risks

| Risk | Mitigation |
| --- | --- |
| Too many brittle UI tests | Keep smoke tests focused on user-critical paths. |
| Business rules only tested through UI | Add backend unit and integration tests. |
| Realtime behavior hard to automate | Test publisher behavior and mock client events first. |
| Database provider differences | Run integration tests against the chosen provider. |
| Missing role tests | Add explicit authorization test cases for protected actions. |
