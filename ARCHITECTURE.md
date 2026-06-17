# Tellma Architecture

This document describes how Tellma is structured **today**. It is a living document — edit it when the system changes. For the *history* of why things are the way they are, see [`docs/adr/`](docs/adr/).

## System Overview

Tellma is a multi-tenant ERP with a three-tier architecture:

- **Database tier** — SQL Server. Admin DB (`Tellma`) + one application DB per tenant (`Tellma.101`, `Tellma.102`, …).
- **Application tier** — ASP.NET Core REST API (`Tellma.Api.Web`).
- **Client tier** — Angular SPA served from the same host in production.

## Backend Layering

Every request flows through the same stack:

```
HTTP → Controller → Service → Repository → Stored Procedure → SQL Server
```

- **Controllers** (`Tellma.Api.Web/Controllers/`) are deliberately thin. They translate HTTP to C# calls. No validation, no business logic.
- **Services** (`Tellma.Api/`) hold all business logic and validation. One service per resource type (`DocumentsService`, `AgentsService`, …). Injected with `IServiceContextAccessor` for tenant/user context.
- **Repositories** (`Tellma.Repository.Application/`) execute stored procedures. No business logic here either.
- **Stored procedures** live in `Tellma.Database.Application/` under `dal/` (data access) and `bll/` (business logic in SQL).

### Base classes

Services and controllers share a layered inheritance chain:

- `ServiceBase` → `FactServiceBase` → `FactGetByIdServiceBase` → `CrudServiceBase` → concrete
- `ControllerBase` → `FactControllerBase` → `FactGetByIdControllerBase` → `CrudControllerBase` → concrete

Pick the base class matching the capabilities you need (read-only vs. full CRUD).

## Multi-Tenancy (Sharding)

- The Admin DB catalogs all tenants and the SQL server hosting each tenant DB.
- `Tellma.Utilities.Sharding` resolves `tenantId → connection string` per request.
- API routes include `tenantId` so the middleware can pick the right DB.

See [ADR-0002](docs/adr/0002-sharded-multitenancy.md) for the reasoning.

## Queryex — Server-Side Expression Language

`Tellma.Repository.Common/Queryex/` is a custom expression parser and compiler. Controllers accept `filter`, `orderby`, `select`, `expand` strings and compile them to SQL at query time.

- Do **not** write raw SQL string concatenation in service code. Use Queryex.
- To add a new function or operator, see the `t-queryex-extend` skill.

See [ADR-0001](docs/adr/0001-queryex-over-ef-linq.md) for the reasoning.

## Definitions System

Many entities (Agents, Resources, Lookups, Documents, Lines) are **definition-based**: a `DefinitionId` points to a metadata record that controls which fields are visible, required, or labeled. This is how one `Agents` table serves customers, suppliers, employees, etc.

## Templating (Templex)

`Tellma.Api/Templating/` is a custom template language for print / email / SMS templates. Templates can embed server-side queries (`QueryInfo`, `QueryEntitiesInfo`) that resolve before rendering.

## Identity

- **Default:** embedded IdentityServer (configured in `Startup.cs`).
- **Production option:** external OIDC authority.
- Toggle via `EmbeddedIdentityServerEnabled` in `appsettings.json`.

## Frontend

- **State:** `WorkspaceService` (`src/app/data/workspace.service.ts`) holds entity caches, settings, definitions, permissions, and navigation state. No NgRx.
- **Metadata-driven rendering:** every entity type exports a `metadata_<EntityType>` descriptor. Shared `MasterComponent` and `DetailsComponent` read these descriptors to render generic list and form screens.
- **Feature layout:** each feature in `src/app/features/<entity>/` ships a `*-master` + `*-details` component plus a routing module.
- **HTTP:** all calls go through `ApiService`.
- **Auth:** `AuthService` wraps `angular-oauth2-oidc`.
- **i18n:** `ngx-translate` with a custom loader that fetches strings from the API.

## Project Map

| Project | Purpose |
|---|---|
| `Tellma.Api.Web` | ASP.NET host, controllers, SPA host |
| `Tellma.Api` | Service layer (business logic) |
| `Tellma.Api.Dto` / `Tellma.Api.Web.Dto` | Request/response shapes |
| `Tellma.Model.Application` / `Tellma.Model.Admin` | Entity models |
| `Tellma.Model.Common` | Shared base classes (`Entity`, `EntityWithKey`) |
| `Tellma.Repository.Application` / `Tellma.Repository.Admin` | Data access |
| `Tellma.Repository.Common` | Query infrastructure (Queryex, `EntityQuery`) |
| `Tellma.Database.Application` / `Tellma.Database.Admin` | SQL schema + stored procs (SSDT projects) |
| `Tellma.Database.Tests` | SQL test database |
| `Tellma.Api.Tests` | xUnit tests (Templating, Metadata, ImportExport) |
| `Tellma.IntegrationTests` | End-to-end HTTP tests |
| `Tellma.Client` | .NET client library |
| `Tellma.Integration.Zatca` | Saudi ZATCA e-invoicing |
| `Tellma.Utilities.*` | Blobs, Caching, Calendars, Email, Logging, Sharding, SendGrid, Sms, Twilio |
| `Tellma.Resources` | Localization RESX files |

## Where to Learn More

- [`CLAUDE.md`](CLAUDE.md) — quick orientation for Claude Code sessions.
- [`queryex-tutorial.md`](queryex-tutorial.md) — Queryex user-facing reference.
- [`docs/adr/`](docs/adr/) — history of architectural decisions and their reasoning.
