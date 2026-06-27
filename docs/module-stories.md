# Respondr — Module Stories

## 1. Auth Module

### Story

As a Dispatcher or Operations Lead, I want to log in to Respondr so that I can access the pages and actions available to my role.

### Scope

* Handles login and logout.
* Handles user access.
* Checks whether the user is a Dispatcher or Operations Lead.
* Controls which pages the user can access.

---

## 2. Dashboard Module

### Story

As a Dispatcher or Operations Lead, I want to see the current emergency situation so that I can quickly understand what needs attention.

### Scope

* Shows active incidents.
* Shows critical incidents.
* Shows available response units.
* Shows recent updates.
* Shows real-time changes when incidents or units are updated.

---

## 3. Incident Module

### Story

As a Dispatcher, I want to create and update emergency incidents so that the operations team can track and respond to them.

### Scope

* Creates a new incident.
* Shows the incident list.
* Shows incident details.
* Updates incident priority.
* Updates incident status.
* Tracks the incident from New, to Assigned, to In Progress, to Resolved, and then Closed.

---

## 4. Response Unit Module

### Story

As an Operations Lead, I want to view and manage response units so that I know which teams or vehicles are available.

### Scope

* Shows response units.
* Tracks unit status.
* Shows whether a unit is Available, Assigned, En Route, On Scene, or Unavailable.
* Manages units such as Rescue Team A, Medical Team D, Relief Truck C, and Fire Truck B.

---

## 5. Assignment Module

### Story

As an Operations Lead, I want to assign an available response unit to an incident so that help can be sent to the emergency.

### Scope

* Shows available units for an incident.
* Assigns a selected unit to an incident.
* Prevents unavailable units from being assigned.
* Updates the incident status after assignment.
* Updates the response unit status after assignment.

---

## 6. Real-Time Updates Module

### Story

As a logged-in user, I want to receive live updates so that I can see important changes without refreshing the page.

### Scope

* Notifies users when an incident is created.
* Notifies users when a unit is assigned.
* Notifies users when an incident status changes.
* Updates the dashboard when important changes happen.
