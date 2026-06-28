# AI-Assisted Development Workflow

Respondr may use AI tools as part of the development workflow. AI is treated as a pair-programming assistant for drafting, scaffolding, refactoring suggestions, documentation support, and consistency checks.

AI does not replace developer judgment. The developer owns architecture, implementation review, testing, and final technical decisions.

## How AI Is Used

AI assistance may be used for:

- Drafting documentation and README updates.
- Creating initial boilerplate for agreed project structures.
- Generating repetitive scaffolding after the structure is already decided.
- Suggesting refactors for readability and consistency.
- Drafting unit test cases or test skeletons.
- Checking whether files follow repository conventions.
- Summarizing implementation tradeoffs for developer review.

## How AI Is Not Used

AI should not be used to:

- Make architecture decisions without developer direction.
- Introduce new architectural patterns silently.
- Add future milestones before they are planned.
- Over-engineer the solution with unnecessary abstractions.
- Replace code review.
- Skip build, test, or Docker compatibility checks.
- Commit changes without developer review.

## Developer Ownership

The developer remains responsible for:

- Product direction and feature scope.
- Architecture and technology decisions.
- Code review and acceptance.
- Testing strategy and quality gates.
- Security-sensitive decisions.
- Deployment decisions.
- Final repository commits.

This keeps the project transparent about AI assistance while ensuring that technical ownership remains with the developer.

## Repository Expectations

AI-assisted changes must follow the existing Respondr direction:

- Service-oriented modular monolith.
- Multiple ASP.NET Core API hosts.
- Shared Application, Domain, and Infrastructure core.
- Shared Contracts and Shared building blocks.
- One SQL Server database for v1.
- Schema separation by module.
- Docker first, Azure later.

AI-assisted changes should not introduce full microservices, separate databases, Kubernetes, message brokers, Azure Functions, event sourcing, or advanced CQRS unless the developer explicitly asks for those changes.

## Review Checklist

Before committing AI-assisted code, review:

- Does the change match the current milestone?
- Is each file in the correct project or documentation folder?
- Did the change avoid unnecessary new layers or patterns?
- Does the code compile?
- Were tests added or updated when behavior changed?
- Does the change preserve Docker compatibility?
- Are public API contracts and event payloads intentional?
- Is the implementation understandable without relying on AI-specific context?

The goal is to use AI productively while keeping Respondr simple, professional, and explainable as a developer-owned portfolio project.
