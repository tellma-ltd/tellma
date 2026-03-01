# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Development server (proxies to localhost:5001 ASP.NET backend)
npm run start         # ng serve
npm run local         # ng serve --host 0.0.0.0 --configuration=local

# Build
npm run build         # ng build (dev)
ng build --configuration=production

# Tests (Karma/Jasmine)
npm run test          # ng test (all tests)
ng test --include="**/units*.spec.ts"   # single spec file pattern

# Lint
npm run lint          # ng lint (tslint)

# Bundle analysis
npm run build-for-analyzer && npm run analyze
```

The Angular dev server proxies API calls to `https://localhost:5001` (configured in `src/environments/environment.ts`). A running ASP.NET backend is required.

## Path Alias

`~/` resolves to `src/` (configured in `tsconfig.json`). Use it for all imports from the `src/` tree:
```ts
import { WorkspaceService } from '~/app/data/workspace.service';
```

## Architecture Overview

### Module Structure

The app has three Angular modules:
- **RootModule** (`src/app/root.module.ts`): Bootstrap module. Routes `/root/*` (landing, companies list, errors), lazy-loads the other two.
- **ApplicationModule** (`src/app/features/application.module.ts`): Main ERP tenant application, route prefix `/app/:tenantId/`.
- **AdminModule** (`src/app/features/admin.module.ts`): Admin console for identity server users/clients, route prefix `/admin/`.

### Central State: WorkspaceService

`src/app/data/workspace.service.ts` is the single source of truth for all app state. It has two sub-workspaces:
- `ws.currentTenant` (a `TenantWorkspace`): Holds per-tenant entities (accounts, documents, etc.), settings, definitions, permissions, and user settings.
- `ws.admin` (an `AdminWorkspace`): Holds admin-area state.

**Entity storage pattern**: Server responses are merged into the workspace entity maps (e.g., `ws.currentTenant.Agent[id]`, `ws.currentTenant.Document[id]`) by `addToWorkspace()` / `addSingleToWorkspace()` in `src/app/data/util.ts`. Components read entities from the workspace by ID rather than holding their own copies—this ensures a single entity update is reflected everywhere it's displayed.

### Master-Details Component Pattern

Every entity type follows this pattern:
- `XxxMasterComponent` — list/grid screen, extends `MasterBaseComponent`, wraps `<t-master>` in its template
- `XxxDetailsComponent` — single-item form screen, extends `DetailsBaseComponent`, wraps `<t-details>` in its template
- `XxxPickerComponent` — inline popup for selecting an entity in a form, wraps `XxxMasterComponent` in popup mode

`MasterBaseComponent` (`src/app/shared/master-base/`) and `DetailsBaseComponent` (`src/app/shared/details-base/`) are thin base classes that wire up events from the underlying `t-master` / `t-details` generic components.

The generic `t-master` (`src/app/shared/master/`) and `t-details` (`src/app/shared/details/`) components contain all the reusable CRUD behavior (pagination, search, save, delete, error display, navigation, unsaved-changes guard, etc.).

### Metadata System

`src/app/data/entities/base/metadata.ts` maps every entity collection name (e.g., `'Agent'`, `'Document'`) to a `metadata_X` function that returns an `EntityDescriptor`. The descriptor provides:
- Display format function (`format`)
- Property descriptors (`properties`) — used by `t-auto-cell` and `t-auto-label` to render any property generically
- `apiEndpoint`, `masterScreenUrl`, `select`, `orderby`, `inactiveFilter`
- Navigation functions (`navigateToDetails`)

When adding a new entity, a `metadata_X` function must be registered in the `metadata` map.

### ApiService

`src/app/data/api.service.ts` exposes typed API factory methods, one per entity type:
```ts
this.unitsApi = this.api.unitsApi(this.notifyDestruct$);
// then: this.unitsApi.get(...), this.unitsApi.getById(...), this.unitsApi.save(...)
```
The `notifyDestruct$` subject cancels in-flight HTTP requests when the component is destroyed (`takeUntil` pattern).

### Settings, Definitions & Caching

On navigation to a tenant, `TenantResolverGuard` loads and caches four versioned blobs in localStorage:
- `settings` — tenant configuration
- `definitions` — dynamic entity definitions (Documents, Agents, Resources, Lookups, Lines, etc.)
- `permissions` — user's CRUD permissions per view
- `user_settings` — user preferences

`RootHttpInterceptor` watches response headers for version tokens; when the server signals a stale version, it triggers a background refresh of the appropriate blob without blocking the user.

`DefinitionsForClient` (`src/app/data/dto/definitions-for-client.ts`) drives most of the app's dynamic behavior—column visibility, entry layouts, main menu items, etc.

### Authentication

`angular-oauth2-oidc` with `localStorage` as `OAuthStorage`. The identity server address comes from `appsettings` in `global-resolver.guard.ts` (loaded from the server at startup as a static JSON or via `/api/global-settings`).

### Internationalization

`@ngx-translate/core` with `CustomTranslationsLoader` which shows a progress overlay while loading translations. Language is driven by user/tenant settings stored in `WorkspaceService`.

### Component Conventions

- **Selector prefix**: `t` (e.g., `<t-master>`, `<t-details>`, `<t-auto-cell>`)
- **All components**: `standalone: false` (the app is NgModule-based, not standalone)
- **Styles**: Inline SCSS (configured in `angular.json` schematics)
- **Tests**: Skipped by default for components, services, guards, pipes, and directives via `skipTests: true` in angular.json schematics
- **Shared components**: Declared only in `SharedModule` (`src/app/shared/shared.module.ts`) and imported by both ApplicationModule and AdminModule
