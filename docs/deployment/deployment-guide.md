# Respondr Deployment Design

## Purpose

This document describes the planned deployment direction for Respondr. It covers the intended local Docker setup and the later Azure deployment shape.

This guide now reflects the current local Docker-first implementation and the planned later Azure direction.

## Deployment Goals

Respondr should be deployable in a way that is:

- Understandable for developers.
- Consistent between local and hosted environments.
- Friendly to Angular and .NET development workflows.
- Ready for managed database hosting.
- Ready for real-time SignalR traffic.
- Easy to automate later through GitHub Actions or Azure DevOps.

## Logical Services

Respondr is planned as three primary runtime services:

| Service | Technology | Responsibility |
| --- | --- | --- |
| Frontend | Angular | Browser UI and static assets. |
| Backend | .NET Web API | REST API, SignalR hub, auth, business rules. |
| Database | SQL Server or PostgreSQL | Persistent operational data. |

## Local Docker Design

Local development currently uses Docker Compose to run:

- SQL Server container
- Identity API container
- Incidents API container
- Dispatch API container
- Resources API container
- Notifications API container
- Realtime API container

Recommended local flow:

```text
Developer browser
  -> API containers on published localhost ports
  -> SQL Server container

Future Angular frontend
  -> REST API over local Docker-exposed HTTP
  -> SignalR hub over WebSocket or Long Polling
```

## Local Service Responsibilities

### Frontend Container

No frontend container is included in the current milestone because the Angular project is not yet present in this repository.

### Backend Container

Responsibilities:

- Run each .NET API host in its own container.
- Expose REST endpoints and SignalR hubs on consistent published ports.
- Apply EF Core migrations from the Identity API container.
- Connect to SQL Server using environment variables from Docker Compose.

### Database Container

Responsibilities:

- Provide local SQL Server.
- Persist data using a Docker volume.
- Support API connectivity checks through container health and DB health endpoints.

## Environment Variables

Expected environment variable categories:

### Backend

- ASPNETCORE_ENVIRONMENT
- ConnectionStrings__RespondrDb
- Authentication settings
- SignalR settings if needed
- Logging level

### Frontend

- Future API base URL
- Future SignalR hub URL
- Future build environment name

### Database

- Database name
- Database user
- Database password
- Port configuration
- Volume configuration

## CORS And SignalR

The backend must allow the frontend origin during local development and hosted deployment.

SignalR currently requires correct handling for:

- WebSocket connections.
- Authorization headers or cookies.
- HTTPS in hosted environments.
- Reverse proxy behavior when deployed behind Azure services.

## Azure Deployment Direction

The planned Azure deployment can use one of these hosting options:

### Option A: Azure App Service

Use Azure App Service for:

- Frontend hosting or static frontend hosting separately.
- Backend .NET API and SignalR hub.

Database:

- Azure SQL Database or Azure Database for PostgreSQL.

Benefits:

- Familiar PaaS model.
- Good fit for .NET applications.
- Straightforward scaling and deployment slots.

Tradeoffs:

- Container orchestration flexibility is lower than Azure Container Apps.

### Option B: Azure Container Apps

Use Azure Container Apps for:

- Frontend container.
- Backend container.

Database:

- Azure SQL Database or Azure Database for PostgreSQL.

Benefits:

- Works well with containerized services.
- Good fit if Docker Compose structure maps naturally to containers.
- Flexible scaling model.

Tradeoffs:

- More platform concepts to manage.

## Azure SignalR Service

Azure SignalR Service is optional for the first hosted version.

Use it when:

- Multiple backend instances need reliable SignalR scale-out.
- WebSocket traffic needs managed scaling.
- Connection count grows beyond simple App Service handling.

For an early deployment, the backend-hosted SignalR hub may be enough.

## Database Hosting

Supported planned choices:

- Azure SQL Database
- Azure Database for PostgreSQL

Selection criteria:

- Team familiarity.
- EF Core provider maturity.
- Cost.
- Operational tooling.
- Existing organizational preference.

## CI/CD Direction

CI/CD can be implemented later through:

- GitHub Actions
- Azure DevOps Pipelines

Expected pipeline stages:

1. Restore dependencies.
2. Run backend build.
3. Run backend tests.
4. Run frontend install/build.
5. Run frontend tests or smoke tests.
6. Build container images.
7. Push images to registry.
8. Deploy to Azure environment.
9. Run post-deployment smoke checks.

## Environment Strategy

Recommended environments:

| Environment | Purpose |
| --- | --- |
| Local | Developer machine using Docker Compose. |
| Development | Shared hosted environment for integration testing. |
| Staging | Production-like validation environment. |
| Production | Live operational environment. |

## Secrets Management

Secrets should not be committed to Git.

Local:

- Use local `.env` files or developer secret storage.

Azure:

- Use Azure App Service configuration, Container Apps secrets, Azure Key Vault, or managed identity depending on final hosting choice.

Secrets may include:

- Database password.
- JWT signing key or auth secret.
- Connection strings.
- External service keys if added later.

## Logging And Monitoring

The hosted application should eventually capture:

- API request logs.
- Backend errors.
- Authentication failures.
- SignalR connection issues.
- Database connectivity issues.
- Incident command failures.

Azure options:

- Application Insights
- Log Analytics
- Azure Monitor

## Deployment Risks

| Risk | Mitigation |
| --- | --- |
| SignalR fails behind proxy | Validate WebSocket configuration early. |
| CORS misconfiguration | Define allowed origins per environment. |
| Database migrations fail | Use controlled migration process and backups. |
| Secrets leak | Use environment variables and Azure secret management. |
| Frontend points to wrong API | Use environment-specific frontend configuration. |

## Deployment Acceptance Criteria

Local deployment is ready when:

- `docker compose config` succeeds.
- SQL Server starts with a persistent volume.
- All backend API containers start through Docker Compose.
- Health endpoints respond successfully.
- Database-backed endpoints can connect to SQL Server.

Azure deployment is ready when:

- Application is accessible over HTTPS.
- Backend API is reachable from frontend.
- SignalR works in hosted environment.
- Database connection is secured.
- Logs are available.
- CI/CD can deploy repeatably.
