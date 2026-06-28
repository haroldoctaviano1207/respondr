# GitHub Copilot Instructions for Respondr

Respondr is a disaster response management system built as a service-oriented modular monolith. Use multiple ASP.NET Core API hosts with a shared core. Keep changes small, milestone-based, and easy to review.

## Architecture

Use:

- Multiple ASP.NET Core API hosts.
- Shared `Respondr.Application`, `Respondr.Domain`, and `Respondr.Infrastructure`.
- `Respondr.Shared` for reusable technical helpers only.
- `Respondr.Contracts` for request models, response models, integration events, and SignalR payloads.
- One SQL Server database first.
- Schema separation: `identity`, `incidents`, `dispatch`, `resources`, `notifications`.
- Docker locally before Azure deployment.

Do not introduce full microservices, separate databases, Kubernetes, RabbitMQ, Azure Service Bus, Azure Functions, event sourcing, advanced CQRS, or new future modules unless explicitly requested.

## Backend Structure

Place backend files under:

```text
backend/
  src/
    Hosts/
      Respondr.Identity.Api/
      Respondr.Incidents.Api/
      Respondr.Dispatch.Api/
      Respondr.Resources.Api/
      Respondr.Notifications.Api/
      Respondr.Realtime.Api/
    Core/
      Respondr.Application/
      Respondr.Domain/
      Respondr.Infrastructure/
    BuildingBlocks/
      Respondr.Shared/
      Respondr.Contracts/
  tests/
    Respondr.Application.Tests/
    Respondr.Domain.Tests/
    Respondr.Api.Tests/
```

## API Style

- Use controllers for `Incidents`, `Dispatch`, `Resources`, and `Notifications`.
- Use Minimal API for `Identity`.
- Use Minimal API plus SignalR hub mappings for `Realtime`.
- Health checks may use Minimal API.

## Project Responsibilities

- `Domain`: entities, enums, simple business rules.
- `Application`: use cases, handlers, services, interfaces, DTOs, workflows.
- `Infrastructure`: EF Core, DbContext, migrations, configurations, JWT, password hashing, SignalR publisher, external implementations.
- `Shared`: reusable helpers such as `Result`, `Error`, paging, constants, date/time provider, common exceptions.
- `Contracts`: API requests, API responses, integration events, SignalR event payloads.

Do not put business logic in `Shared`.

Do not put EF entities, handlers, services, validation logic, or business rules in `Contracts`.

## Development Rules

- Follow the active milestone.
- Do not create future milestone implementation early.
- Do not invent new architecture patterns.
- Keep generated changes small and reviewable.
- Preserve Docker compatibility.
- Add or update tests when behavior changes.
- Generated code must compile.
