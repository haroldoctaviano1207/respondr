# Respondr

Respondr is an emergency response operations platform for tracking incidents, coordinating response units, assigning teams, and keeping operators updated in real time.

The first version is focused on a small operations team with two primary roles:

- Dispatcher: records emergency reports, creates incidents, monitors updates, and keeps the incident record accurate.
- Operations Lead: reviews active incidents, assigns response units, monitors operational capacity, and moves incidents through resolution.

The planned technical stack is:

- Frontend: Angular, TypeScript, SCSS or CSS
- Backend: .NET Web API, C#, Entity Framework Core
- Database: SQL Server or PostgreSQL
- Real-time updates: SignalR
- Local runtime: Docker and Docker Compose
- Later hosting: Azure App Service or Azure Container Apps, managed database, optional Azure SignalR Service

## Product Scope

Respondr provides a shared operational dashboard for incident response teams. The application should make it easy to:

- Log in securely.
- See current emergency activity.
- Create and update incidents.
- View response unit availability.
- Assign units to incidents.
- Track incident lifecycle state.
- Receive live updates without refreshing the page.
- Review assignment and status history.

The initial product intentionally stays focused. Advanced capabilities such as GPS tracking, maps, SMS, email notifications, file uploads, mobile native applications, AI assistance, offline mode, and multi-agency workflows are outside the first version unless they are added through a later planning decision.

## Repository Structure

```text
respondr/
  docs/
    product-requirements.md
    user-workflows.md
    technical/
      system-architecture.md
      data-model.md
      api-spec.md
      realtime-spec.md
    deployment/
      deployment-guide.md
      ci-cd.md
  backend/
    Respondr.sln
  frontend/
    respondr-web/
  README.md
  docker-compose.yml
  .gitignore
```

The documentation currently defines the intended product, user workflows, architecture, data model, API surface, real-time behavior, deployment direction, and CI/CD quality checks.

Source code, Docker files, deployment assets, and CI/CD configuration will be added later.

## Documentation Index

- [Product Requirements](docs/product-requirements.md)
- [User Workflows](docs/user-workflows.md)
- [System Architecture](docs/technical/system-architecture.md)
- [Data Model](docs/technical/data-model.md)
- [API Spec](docs/technical/api-spec.md)
- [Realtime Spec](docs/technical/realtime-spec.md)
- [Deployment Guide](docs/deployment/deployment-guide.md)
- [CI/CD](docs/deployment/ci-cd.md)

## Architecture Summary

The intended system flow is:

```text
Angular frontend -> .NET Web API -> Database
Angular frontend -> SignalR hub -> live updates
.NET Web API -> SignalR hub -> push changes after commands
```

The user works in the browser. The Angular frontend loads the Respondr UI, calls the .NET REST API for commands and queries, and connects to SignalR for live operational updates. The backend persists users, incidents, response units, assignments, and history in the database.

## Core Modules

- Auth: login, logout, identity, and role-based access.
- Dashboard: operational overview, active incidents, critical incidents, available units, and live updates.
- Incidents: incident creation, editing, status changes, priority changes, and details.
- Response Units: team or vehicle availability, unit status, unit metadata, and capacity tracking.
- Assignments: linking response units to incidents and preventing invalid assignments.
- Real-Time Updates: SignalR events for incident creation, assignment changes, unit changes, and status updates.

## Development Direction

The application should be built as a clear enterprise dashboard, with frontend components that map cleanly to Angular modules, routed pages, shared components, and services.

The backend should expose a REST API for predictable commands and queries. SignalR should be used only for live notifications and state refresh triggers, not as the primary command channel.

The database design should support auditability. Important operational changes should be recorded in history tables or update records so that users can understand what happened, when it happened, and who performed the action.

## Local Setup

Local setup is planned to use Docker Compose with separate services for:

- Angular frontend
- .NET API
- Database

Exact Docker files, environment variables, and commands will be added when the implementation structure is created.

## Status

This repository is currently in planning and documentation setup. Implementation work will follow the documented architecture and task breakdown.
