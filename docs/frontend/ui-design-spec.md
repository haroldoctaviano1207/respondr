# Respondr UI Specification

## Purpose

This document defines the Respondr application screens, user interface behavior, and frontend implementation expectations. Visual styling and reusable component rules are defined separately in `docs/frontend/design-system.md`.

The UI must support the core product workflows documented in `docs/product-requirements.md` and `docs/user-workflows.md`: incident intake, incident monitoring, dispatch coordination, resource tracking, notifications, and realtime operational awareness.

## Primary Users

### Dispatcher

The Dispatcher creates incidents, updates incident details, monitors operational changes, and needs fast access to active incident state.

### Operations Lead

The Operations Lead reviews active incidents, assigns responders, monitors resource capacity, and moves incidents through resolution.

## Application Routes

Required v1 routes:

```text
/login
/dashboard
/incidents
/incidents/new
/incidents/:id
/dispatch
/resources
/notifications
```

Optional later routes:

```text
/reports
/settings
/profile
```

Reports and Settings should not delay the core v1 operational workflow.

## Authenticated App Shell

The authenticated app uses a persistent shell:

```text
Sidebar
  Brand
  Dashboard
  Incidents
  Dispatch
  Resources
  Notifications
  Optional: Reports
  Optional: Settings
  New Incident action
  Logout

Topbar
  Mobile menu button
  Global search
  Sidebar collapse button
  Live connection indicator
  Notifications menu
  User profile menu

Content
  Current routed page
```

Global search should support incident ID, incident title, location, reporter, responder name, and resource request terms where available.

## Login

The login screen should provide secure access without marketing-style content.

Required elements:

- Respondr brand.
- Short product context.
- Email or username field.
- Password field with show/hide control.
- Remember me checkbox if supported.
- Forgot password link if supported.
- Inline validation.
- Loading state on submit.

Successful login behavior:

- Navigate to Dashboard.
- Load current user profile.
- Start SignalR connection.
- Show a subtle confirmation that the dashboard is synced.

Failed login behavior:

- Show an accessible form-level error.
- Preserve entered username/email.
- Do not reveal whether username or password was incorrect.

## Dashboard

The Dashboard is the default authenticated screen. It provides immediate operational awareness.

Required sections:

- Page heading with `Log New Incident` primary action.
- KPI cards.
- Active operational zone panel.
- Resource summary.
- Recent alerts.
- Operations feed.
- Priority tasks or pending actions.

Recommended KPI cards:

- Active Incidents.
- Critical Incidents.
- Units Deployed.
- Available Units.
- Average Response Time.
- Pending Resource Requests.

Dashboard behavior:

- Counters update after relevant API responses or realtime events.
- Critical new incidents may trigger a visible toast.
- Empty dashboard states should explain that no active operations are currently available.
- The operational zone panel may be a non-interactive visual summary in v1.

## Incidents List

The Incidents page is the primary operational list for incident monitoring.

Required elements:

- Page heading.
- `New Incident` action.
- Search input.
- Quick filters: All, Critical, Active, New, Resolved.
- Advanced filter drawer.
- Paginated incident table.
- Row action to open incident detail.

Required columns:

- Incident ID.
- Type.
- Priority.
- Status.
- Location.
- Reporter.
- Reported time.
- Assigned unit or assignment count.
- Row actions.

Filter drawer options:

- Reported timeframe.
- Operational region or dispatch zone.
- Incident type.
- Priority.
- Status.
- Resource requirement, optional.

Realtime behavior:

- `IncidentCreated` adds or refreshes the list.
- `IncidentStatusChanged` updates the affected row.
- Updated rows may be briefly highlighted.
- Active filters must remain applied after refresh.

## Create Incident

The Create Incident page supports fast, accurate incident intake.

Required sections:

- Core Incident Details.
- Location and Timeline.
- Reporter Information.
- Initial Assignment, optional.
- Situation Summary.

Required fields:

- Incident title.
- Incident type.
- Priority.
- Location or address.
- Reported time.
- Situation summary.

Recommended fields:

- Dispatch zone.
- Reporter name.
- Reporter contact.
- Initial responder/unit.
- Primary hazard.

Interaction rules:

- Default incident status is `New`.
- Required fields validate inline.
- Controlled values should match backend enums.
- Primary submit action is `Create Incident Record`.
- Cancel returns to the incidents list.
- Save Draft should only be implemented if backend support exists.

Successful creation:

- Navigate to incident detail or incidents list.
- Show confirmation.
- Refresh dashboard counters.

## Incident Detail

The Incident Detail page combines operational state, assignment state, event history, communication, and resource needs.

Required sections:

- Breadcrumbs.
- Incident summary header.
- Priority and status badges.
- Location and reported time.
- Primary actions.
- Event description.
- Operational timeline.
- Assigned units panel.
- Communication or activity log.
- Active resource requests.

Primary actions:

- Assign Unit.
- Update Status.
- Update Priority, if allowed.
- Mark Resolved.
- Close Incident, when eligible.

Timeline entries should show:

- Event title.
- Actor or source.
- Timestamp.
- Event category.

Realtime behavior:

- Timeline updates without full page refresh.
- Status and priority badges update after relevant events.
- Assigned units panel updates after assignment events.
- Unsaved edits must not be overwritten.

## Dispatch

The Dispatch page supports responder assignment and assignment tracking.

Required sections:

- Active assignments table.
- Available responders or units list.
- Assignment detail panel.
- Unit availability summary.
- Assignment status controls.

Core actions:

- Assign responder or unit to incident.
- Unassign or release responder.
- Update assignment status.
- Update responder availability.

Required assignment states:

- Pending.
- Assigned.
- En Route.
- On Scene.
- Completed.
- Released.
- Cancelled.

Required responder states:

- Available.
- Assigned.
- En Route.
- On Scene.
- Unavailable.

Validation behavior:

- Prevent assigning unavailable responders.
- Prevent assigning responders to closed incidents.
- Show conflict errors clearly and close to the action.

## Resources

The Resources page supports requests, approvals, and allocations.

Required sections:

- Resource requests table.
- Create resource request action.
- Available resources summary.
- Pending approvals.
- Active allocations.

Core actions:

- Create resource request.
- Approve request.
- Reject request.
- Mark allocation fulfilled.
- Cancel request, when allowed.

Required request statuses:

- Pending.
- Approved.
- Rejected.
- Allocated.
- Fulfilled.
- Cancelled.

Realtime behavior:

- Approved or rejected requests update visible lists.
- Related notifications update unread counts.

## Notifications

Notifications support operational awareness without overwhelming the user.

Required surfaces:

- Topbar notification menu.
- Notifications page.
- Toasts for high-value events.

Notification menu:

- Shows unread count.
- Shows recent notifications.
- Provides link to full notifications page.
- Allows mark as read where practical.

Notifications page:

- Lists all notifications.
- Supports unread filter.
- Links to related incident, assignment, or resource request.

High-value notification examples:

- Critical incident created.
- Responder assigned.
- Assignment status changed.
- Resource request approved or rejected.
- SignalR connection lost or restored.

## Reports

Reports are optional for v1.

Potential reports:

- Incident volume.
- Severity distribution.
- Response time trends.
- Unit utilization.
- Resource request volume.

Do not add advanced analytics until the core operational workflows are implemented.

## Settings

Settings are optional for v1 unless needed for authentication or roles.

Potential settings:

- User directory.
- Roles.
- Organization settings.
- Notification preferences.

Do not implement broad admin configuration before Identity, Incidents, Dispatch, Resources, and Notifications are stable.

## Realtime Event Mapping

SignalR events should update the UI predictably.

| Event | UI Response |
| --- | --- |
| `IncidentCreated` | Add to dashboard feed and incident list; show toast for critical incidents. |
| `IncidentStatusChanged` | Update incident row, detail header, dashboard counters, and timeline. |
| `IncidentPriorityChanged` | Update incident row, detail header, dashboard counters, and alerts. |
| `ResponderAssigned` | Update dispatch list, assigned units panel, incident timeline, and operations feed. |
| `AssignmentStatusChanged` | Update dispatch list, incident timeline, and dashboard feed. |
| `ResourceRequestApproved` | Update resources list and related notification state. |
| `NotificationCreated` | Increment unread count and add item to notification menu. |
| `DashboardUpdated` | Refresh dashboard counters and resource summary. |

Realtime rules:

- REST commands remain the source of user-initiated changes.
- SignalR should notify or patch visible state after committed backend changes.
- If SignalR disconnects, REST workflows should remain usable.
- The UI should show degraded live state and retry connection.

## Loading, Empty, And Error States

Every major page needs:

- Initial loading state.
- Empty state.
- Recoverable error state.
- Permission denied state where authorization applies.
- Degraded live connection state where realtime matters.

Examples:

- No active incidents.
- No available responders.
- No pending resource requests.
- No notifications.
- Unable to load dashboard summary.
- Live updates temporarily unavailable.

## Angular Implementation Notes

Suggested structure:

```text
frontend/
  respondr-web/
    src/
      app/
        core/
          auth/
          api/
          realtime/
          guards/
        shared/
          components/
          forms/
          layout/
          tables/
          feedback/
        features/
          auth/
          dashboard/
          incidents/
          dispatch/
          resources/
          notifications/
          reports/
          settings/
```

`core` should contain application-wide services:

- Auth/session service.
- API client configuration.
- Route guards.
- SignalR connection service.
- Notification service.

`shared` should contain reusable UI:

- App shell.
- Buttons.
- Badges.
- Form field wrappers.
- Tables.
- Modals.
- Drawers.
- Toasts.
- Loading, empty, and error states.

Feature folders should own routed pages, feature-specific components, feature services, and feature state.

## Definition Of Done

A Respondr UI implementation is acceptable when:

- Required v1 routes exist.
- The authenticated app shell works on desktop and mobile.
- Dashboard, Incidents, Incident Detail, Create Incident, Dispatch, Resources, and Notifications support their documented workflows.
- Required forms validate inline.
- Operational tables support search, filters, and pagination where documented.
- Realtime events update visible state without disrupting user input.
- Loading, empty, error, permission, and degraded-live states exist.
- Components follow `docs/frontend/design-system.md`.
- The UI remains aligned with the backend API, data model, and realtime specs.
