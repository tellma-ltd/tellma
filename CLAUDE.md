# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What is Tellma

Tellma is a multi-tenant ERP system with a 3-tier architecture:
- **Database Tier**: SQL Server (Admin DB: `Tellma`, App DBs: `Tellma.101`, `Tellma.102`, …)
- **Application Tier**: ASP.NET Core 8 REST API (`Tellma.Api.Web`)
- **Client Tier**: Angular 21 SPA (`Tellma.Api.Web/ClientApp`)

## Commands

### Backend (ASP.NET Core)
```bash
# Build the entire solution
dotnet build Tellma.sln

# Run the web server (defaults to https://localhost:5001)
dotnet run --project Tellma.Api.Web

# Run all C# unit tests
dotnet test Tellma.Api.Tests/Tellma.Api.Tests.csproj

# Run C# unit tests with DB settings (for repository/database tests)
dotnet test --settings Tellma.runsettings

# Run integration tests
dotnet test Tellma.IntegrationTests/Tellma.IntegrationTests.csproj
```

### Frontend (Angular)
All frontend commands run from `Tellma.Api.Web/ClientApp/`:
```bash
cd Tellma.Api.Web/ClientApp

# Install dependencies (first time or after package.json changes)
npm install

# Serve development build pointing at the default backend (localhost:5001)
ng serve -o

# Serve on all interfaces (for network access)
ng serve --host 0.0.0.0 --configuration=local

# Run Angular tests
ng test

# Production build
ng build

# Lint
ng lint
```

### Database
Database projects (`Tellma.Database.Admin`, `Tellma.Database.Application`) must be published via Visual Studio's **Publish** dialog (SSDT), not `dotnet`. They target SQL Server at `.` with Windows auth.

## Project Structure

```
Tellma.Api.Web/          # ASP.NET Core host: Startup.cs, controllers, Angular SPA host
Tellma.Api/              # Business logic services (the "service layer")
Tellma.Api.Dto/          # API data transfer objects (request/response shapes)
Tellma.Api.Web.Dto/      # Web-specific DTOs
Tellma.Model.Application/ # C# entity models for application database
Tellma.Model.Admin/      # C# entity models for admin database
Tellma.Model.Common/     # Shared model base classes (Entity, EntityWithKey, etc.)
Tellma.Repository.Application/ # Data access for application DB (calls stored procs)
Tellma.Repository.Admin/  # Data access for admin DB
Tellma.Repository.Common/ # Shared query infrastructure (Queryex, EntityQuery, etc.)
Tellma.Database.Application/ # SQL Server project: schema, stored procedures, functions
Tellma.Database.Admin/   # SQL Server project: admin schema
Tellma.Database.Tests/   # SQL test database project
Tellma.Api.Tests/        # xUnit tests: Templating, Metadata, ImportExport
Tellma.IntegrationTests/ # End-to-end HTTP integration tests
Tellma.Client/           # .NET client library for consuming the Tellma API
Tellma.Integration.Zatca/ # Saudi ZATCA e-invoicing integration
Tellma.Utilities.*/      # Utility libraries: Blobs, Caching, Calendars, Email, Logging, Sharding, SendGrid, Sms, Twilio
Tellma.Resources/        # Localization resource (RESX) files
```

## Architecture Patterns

### Backend Layer Stack
`Controller → Service → Repository → SQL Stored Procedures`

- **Controllers** (`Tellma.Api.Web/Controllers/`) are deliberately thin. They translate HTTP requests to C# calls and return responses. Validation is **not** done here.
- **Services** (`Tellma.Api/`) contain all business logic and validation. Each resource type has a dedicated service (e.g., `DocumentsService`, `AgentsService`).
- **Repositories** (`Tellma.Repository.Application/`) execute SQL stored procedures against the application database. They never contain business logic.
- **Stored Procedures** live in `Tellma.Database.Application/` under `dal/` (data access layer) and `bll/` (business logic layer in SQL).

### Base Classes (key inheritance chains)
- `ServiceBase` → `FactServiceBase` → `FactGetByIdServiceBase` → `CrudServiceBase` → concrete services
- `ControllerBase` → `FactControllerBase` → `FactGetByIdControllerBase` → `CrudControllerBase` → concrete controllers
- Services are injected with `IServiceContextAccessor` which provides tenant context (database ID, user ID).

### Multi-Tenancy via Sharding
- The Admin database (`Tellma`) catalogs all tenant databases and their SQL server locations.
- Each tenant has their own application database (`Tellma.101`, `Tellma.102`, etc.).
- `Tellma.Utilities.Sharding` resolves which database to connect to for a given tenant ID.
- API routes include a `tenantId` to identify the tenant.

### Queryex — Custom Query Language
`Tellma.Repository.Common/Queryex/` implements a custom expression parser and compiler. Controllers accept string parameters like `filter`, `orderby`, `select`, `expand` and these are compiled to SQL. Use this pattern when adding filterable endpoints — do not write raw SQL strings in service code.

### Definitions System
Many entities (Agents, Resources, Lookups, Documents, Lines) are "definition-based": a `DefinitionId` points to a metadata record that controls which fields are visible, required, or labeled differently. This is how one "Agent" collection serves customers, suppliers, employees, etc.

### Templating Engine
`Tellma.Api/Templating/` contains a custom template language (Templex) used for printing templates and email/SMS templates. Templates can embed server-side queries (`QueryInfo`, `QueryEntitiesInfo`) that are resolved before rendering.

### Identity
By default the application runs an **embedded IdentityServer** (configured in `Startup.cs`). In production, an external OIDC authority can be used instead. The `EmbeddedIdentityServerEnabled` flag in `appsettings.json` controls this.

## Frontend Architecture

### State Management — WorkspaceService
`src/app/data/workspace.service.ts` is the central state store. It holds:
- Fetched entity caches (indexed by type and ID)
- `SettingsForClient`, `DefinitionsForClient`, `PermissionsForClientViews`
- Navigation state (`MasterDetailsStore`) for each screen

There is no NgRx/Redux. Components read from and write to the workspace directly.

### Entity Metadata System
Every entity type has a TypeScript metadata descriptor exported as `metadata_<EntityType>` from its entity file (e.g., `src/app/data/entities/agent.ts`). These descriptors are registered in `src/app/data/entities/base/metadata.ts` and describe properties, navigation paths, display formats, and labels. The shared `master` and `details` components use these descriptors to render generic list and form screens.

### Screen Pattern (features/)
Each feature module in `src/app/features/<entity-name>/` follows a consistent pattern:
- A `*-master.component` for listing (uses `<t-master>`)
- A `*-details.component` for editing (uses `<t-details>`)
- A routing module connecting the two

The shared `MasterComponent` and `DetailsComponent` in `src/app/shared/` handle all common behavior (pagination, saving, error display, keyboard shortcuts).

### Key Services
- `ApiService` (`src/app/data/api.service.ts`) — all HTTP calls to the backend
- `AuthService` — OIDC authentication using `angular-oauth2-oidc`
- `WorkspaceService` — entity cache and client-side state
- `TranslateService` — i18n (ngx-translate, with custom loader from API)

## Configuration

### Development
- Admin DB connection string in `appsettings.Development.json`: `Server=.;Database=Tellma;Trusted_Connection=true;...`
- `EmbeddedClientApplicationEnabled: false` in development — the Angular app runs separately at `localhost:4200`
- The backend expects the Angular dev server at `http://localhost:4200` (set in user secrets or `appsettings.Development.json` → `ClientApplications.WebClientUri`)

### Optional Features (off by default)
Enabled via `appsettings.json` flags or user secrets:
- `EmailEnabled` → requires SendGrid API key
- `SmsEnabled` → requires Twilio credentials
- `AzureBlobStorageEnabled` → requires Azure Storage connection string
- `Azure.SignalR.Enabled` → use Azure SignalR Service instead of in-process

### Default Dev Credentials
- Admin user: `admin@tellma.com` / `Admin@123`
- Backend URL: `https://localhost:5001`
- Frontend URL: `http://localhost:4200`
