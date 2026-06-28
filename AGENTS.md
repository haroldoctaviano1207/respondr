# Respondr Agent Instructions

This repository may use AI coding agents as development assistants. Agents should help with scaffolding, repetitive edits, documentation drafts, tests, refactoring suggestions, and consistency checks. The developer owns the architecture, code review, testing, and final technical decisions.

## Project Direction

Respondr is a disaster response management system built as a service-oriented modular monolith with multiple ASP.NET Core API hosts and a shared core.

The intended structure is:

```text
respondr/
  backend/
    Respondr.sln
  frontend/
    respondr-web/
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
  docker-compose.yml
  README.md
  .gitignore
```

## Architecture Guardrails

Use the agreed v1 architecture:

- Multiple ASP.NET Core API hosts.
- Shared `Respondr.Application`, `Respondr.Domain`, and `Respondr.Infrastructure` core projects.
- Shared `Respondr.Shared` and `Respondr.Contracts` building blocks.
- One SQL Server database first.
- Schema separation for `identity`, `incidents`, `dispatch`, `resources`, and `notifications`.
- Controller-based APIs for CRUD and business modules.
- Minimal API for Identity.
- Minimal API plus SignalR hubs for Realtime.
- Docker locally before Azure deployment.
- Azure deployment later, after local Docker works.

Do not introduce full microservices, separate databases, Kubernetes, RabbitMQ, Azure Service Bus, Azure Functions, event sourcing, advanced CQRS, or other larger patterns unless the developer explicitly requests them.

## Backend Layout

Use this backend structure:

```text
backend/
  Respondr.sln
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

## Project Responsibilities

- `Respondr.Domain` contains entities, enums, and simple business rules.
- `Respondr.Application` contains use cases, handlers, services, interfaces, DTOs, and business workflows.
- `Respondr.Infrastructure` contains EF Core, DbContext, migrations, configurations, JWT service, password hashing, SignalR publisher implementation, and external implementations.
- `Respondr.Shared` contains reusable helpers only, such as `Result`, `Error`, `PagedResult`, `PaginationRequest`, schema names, policy names, role names, header names, date/time provider, and common exceptions.
- `Respondr.Contracts` contains request models, response models, integration event models, and SignalR event payloads.

Do not put business logic in `Respondr.Shared`.

Do not put EF entities, handlers, services, validation logic, or business rules in `Respondr.Contracts`.

## API Style Rules

- `Respondr.Incidents.Api`, `Respondr.Dispatch.Api`, `Respondr.Resources.Api`, and `Respondr.Notifications.Api` use controllers.
- `Respondr.Identity.Api` uses Minimal API.
- `Respondr.Realtime.Api` uses Minimal API style and SignalR hub mappings.
- Health checks may use Minimal API.

## Milestone Workflow

Keep work milestone-based:

1. Solution and Docker base
2. BuildingBlocks: Shared and Contracts
3. Core: Domain, Application, Infrastructure
4. Identity API
5. Incidents API
6. Realtime API / SignalR
7. Dispatch API
8. Notifications API
9. Resources API
10. Full Docker run
11. Angular integration
12. Azure deployment

Do not create future milestone implementation early. It is fine to leave placeholders only when they are required for compilation or project structure.

## AI-Assisted Development Rules

Agents may help with:

- Boilerplate and project scaffolding.
- Documentation drafts.
- Unit test scaffolding.
- Refactoring suggestions.
- Consistency checks across files.
- Small implementation tasks within the active milestone.

Agents must not:

- Make architecture decisions without explicit developer direction.
- Silently introduce new architecture patterns.
- Add unnecessary files or layers.
- Create future milestones early.
- Replace developer review.
- Ignore Docker compatibility.

Every generated change should:

- Follow the agreed project structure.
- Keep changes small and reviewable.
- Compile before being considered done.
- Add or update tests when behavior changes.
- Preserve Docker compatibility.
- Be reviewed by the developer before commit.

## Review Expectations

Before finishing a change, agents should check:

- The change belongs to the current milestone.
- The file was placed in the correct project or docs folder.
- No unnecessary architecture pattern was added.
- Existing public contracts were not changed accidentally.
- Tests were added or updated when behavior changed.
- The solution builds where applicable.
- Docker-related files remain consistent where applicable.

Keep the repository simple, professional, and explainable in an interview.

## Project Review Agents

Reusable pre-PR agent prompts live in `.codex/agents/`.

- `review`: Lead Developer review agent for architecture, maintainability, project boundaries, API shape, and build/test risk.
- `quality-check`: Staff QA / Principal-level quality agent for acceptance criteria, test coverage, edge cases, regression risk, and release readiness.

Human-readable agent descriptions live in `docs/development/agents/`.
