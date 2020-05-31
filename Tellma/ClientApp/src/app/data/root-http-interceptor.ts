import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HttpResponseBase } from '@angular/common/http';
import { Observable, Subject, throwError, timer, of } from 'rxjs';
import { WorkspaceService } from './workspace.service';
import { tap, exhaustMap, retry, catchError, concatMap } from 'rxjs/operators';
import { ApiService } from './api.service';
import { Versioned } from './dto/versioned';
import { SettingsForClient } from './dto/settings-for-client';
import { PermissionsForClient } from './dto/permissions-for-client';
import { StorageService } from './storage.service';
import {
  handleFreshPermissions, versionStorageKey, storageKey,
  SETTINGS_PREFIX, PERMISSIONS_PREFIX, USER_SETTINGS_PREFIX, DEFINITIONS_PREFIX,
  handleFreshUserSettings, handleFreshSettings, handleFreshDefinitions
} from './tenant-resolver.guard';
import {
  handleFreshPermissions as handleFreshAdminPermissions, handleFreshSettings as handleFreshAdminSettings,
  handleFreshUserSettings as handleFreshAdminUserSettings, ADMIN_PERMISSIONS_PREFIX, ADMIN_SETTINGS_PREFIX,
  ADMIN_USER_SETTINGS_PREFIX, versionStorageKey as adminVersionStorageKey, storageKey as adminStorageKey
} from './admin-resolver.guard';
import { Router } from '@angular/router';
import { UserSettingsForClient } from './dto/user-settings-for-client';
import { OAuthStorage } from 'angular-oauth2-oidc';
import { CleanerService } from './cleaner.service';
import { TranslateService } from '@ngx-translate/core';
import { GlobalSettingsForClient } from './dto/global-settings';
import { handleFreshGlobalSettings } from './global-resolver.guard';
import { DefinitionsForClient } from './dto/definitions-for-client';
import { AdminSettingsForClient } from './dto/admin-settings-for-client';
import { AdminPermissionsForClient } from './dto/admin-permissions-for-client';
import { AdminUserSettingsForClient } from './dto/admin-user-settings-for-client';
import { toLocalDateISOString } from './util';

type VersionStatus = 'Fresh' | 'Stale' | 'Unauthorized';

export class RootHttpInterceptor implements HttpInterceptor {

  // Global
  private notifyRefreshGlobalSettings$: Subject<void>;

  // Admin Console
  private notifyRefreshSettings$: Subject<void>;
  private notifyRefreshDefinitions$: Subject<void>;
  private notifyRefreshPermissions$: Subject<void>;
  private notifyRefreshUserSettings$: Subject<void>;

  // Application
  private notifyRefreshAdminSettings$: Subject<void>;
  private notifyRefreshAdminPermissions$: Subject<void>;
  private notifyRefreshAdminUserSettings$: Subject<void>;

  // Misc.
  private notifyPingAfterOneSecond$: Subject<void>;
  private cancellationToken$: Subject<void>;

  // Global
  private globalSettingsApi: () => Observable<Versioned<GlobalSettingsForClient>>;

  // Admin Console
  private adminSettingsApi: () => Observable<Versioned<AdminSettingsForClient>>;
  private adminPermissionsApi: () => Observable<Versioned<AdminPermissionsForClient>>;
  private adminUserSettingsApi: () => Observable<Versioned<AdminUserSettingsForClient>>;

  // Application
  private settingsApi: () => Observable<Versioned<SettingsForClient>>;
  private definitionsApi: () => Observable<Versioned<DefinitionsForClient>>;
  private permissionsApi: () => Observable<Versioned<PermissionsForClient>>;
  private userSettingsApi: () => Observable<Versioned<UserSettingsForClient>>;

  // Misc.
  private pingApi: () => Observable<void>;

  constructor(
    private workspace: WorkspaceService, private api: ApiService,
    private storage: StorageService, private router: Router,
    private authStorage: OAuthStorage, private cleaner: CleanerService,
    private translate: TranslateService) {
    // Note: We use exhaustMap to prevent making another call while a call is in progress
    // https://www.learnrxjs.io/operators/transformation/exhaustmap.html

    // Global
    this.notifyRefreshGlobalSettings$ = new Subject<void>();
    this.notifyRefreshGlobalSettings$.pipe(
      exhaustMap(() => this.doRefreshGlobalSettings())
    ).subscribe();

    // Admin Console
    this.notifyRefreshAdminSettings$ = new Subject<void>();
    this.notifyRefreshAdminSettings$.pipe(
      exhaustMap(() => this.doRefreshAdminSettings())
    ).subscribe();

    this.notifyRefreshAdminPermissions$ = new Subject<void>();
    this.notifyRefreshAdminPermissions$.pipe(
      exhaustMap(() => this.doRefreshAdminPermissions())
    ).subscribe();

    this.notifyRefreshAdminUserSettings$ = new Subject<void>();
    this.notifyRefreshAdminUserSettings$.pipe(
      exhaustMap(() => this.doRefreshAdminUserSettings())
    ).subscribe();

    // Application
    this.notifyRefreshSettings$ = new Subject<void>();
    this.notifyRefreshSettings$.pipe(
      exhaustMap(() => this.doRefreshSettings())
    ).subscribe();

    this.notifyRefreshDefinitions$ = new Subject<void>();
    this.notifyRefreshDefinitions$.pipe(
      exhaustMap(() => this.doRefreshDefinitions())
    ).subscribe();

    this.notifyRefreshPermissions$ = new Subject<void>();
    this.notifyRefreshPermissions$.pipe(
      exhaustMap(() => this.doRefreshPermissions())
    ).subscribe();

    this.notifyRefreshUserSettings$ = new Subject<void>();
    this.notifyRefreshUserSettings$.pipe(
      exhaustMap(() => this.doRefreshUserSettings())
    ).subscribe();

    // Misc.
    this.notifyPingAfterOneSecond$ = new Subject<void>();
    this.notifyPingAfterOneSecond$.pipe(
      exhaustMap(() => this.doPingAfterOneSecond())
    ).subscribe();

    this.cancellationToken$ = new Subject<void>();

    // Global
    this.globalSettingsApi = this.api.globalSettingsApi(this.cancellationToken$).getForClient;

    // Admin Console
    this.adminSettingsApi = this.api.adminSettingsApi(this.cancellationToken$).getForClient;
    this.adminPermissionsApi = this.api.adminPermissionsApi(this.cancellationToken$).getForClient;
    this.adminUserSettingsApi = this.api.adminUsersApi(this.cancellationToken$).getForClient;

    // Application
    this.settingsApi = this.api.settingsApi(this.cancellationToken$).getForClient;
    this.definitionsApi = this.api.definitionsApi(this.cancellationToken$).getForClient;
    this.permissionsApi = this.api.permissionsApi(this.cancellationToken$).getForClient;
    this.userSettingsApi = this.api.usersApi(this.cancellationToken$).getForClient;

    // Misc.
    this.pingApi = this.api.pingApi(this.cancellationToken$).ping;
  }

  public intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

    // Captured here for response interceptor
    const tenantId = this.workspace.ws.tenantId;

    // Some APIs are left untouched
    if (!req.url.endsWith('/appsettings.json') &&
      !req.url.endsWith('/appsettings.development.json') &&
      !req.url.endsWith('api/ping')) {

      // we accumulate all the headers & params in these objects
      const headers: { [key: string]: string } = {};
      const params: { [key: string]: string } = {};

      // Today
      headers['X-Today'] = toLocalDateISOString(new Date());

      if (!!this.authStorage) {
        const accessToken = this.authStorage.getItem('access_token');
        if (!!accessToken) {
          headers.Authorization = `Bearer ${accessToken}`;
        }
      }

      // UI culture
      const culture = this.workspace.ws.culture || this.translate.currentLang || this.translate.defaultLang;
      if (!!culture) {
        params['ui-culture'] = culture;
      }

      // True if this is an API request to refresh the
      const isVersionRefreshRequest = req.url.endsWith('/client') ||
        req.url.indexOf('/client?') !== -1 || req.url.indexOf('/client/') !== -1;

      // Workspace specific headers
      if (this.workspace.isApp) {

        // tenant ID
        if (!!tenantId) {
          // This piece of logic does not really belong to the root module and is
          // specific to the application module, but moving it there is not worth
          // the hassle now
          headers['X-Tenant-Id'] = tenantId.toString();

          // Even though API response caching is disabled with server headers, this is a last defense
          // to absolutely guarantee that caching will never cause one tenant's data to show up while
          // you're logged into another tenant, but the server only relies on the header X-Tenant-Id
          params['tenant-id'] = tenantId.toString();
        }

        // the version refresh APIs should not include the version headers
        if (!isVersionRefreshRequest) {
          // tenant versions
          const current = this.workspace.currentTenant;
          if (!!current) {
            headers['X-Settings-Version'] = current.settingsVersion || '???';
            headers['X-Definitions-Version'] = current.definitionsVersion || '???';
            headers['X-Permissions-Version'] = current.permissionsVersion || '???';
            headers['X-User-Settings-Version'] = current.userSettingsVersion || '???';
          }
        }
      } else if (this.workspace.isAdmin) {
        // the version refresh APIs should not include the version headers
        if (!isVersionRefreshRequest) {
          // tenant versions
          const admin = this.workspace.admin;
          if (!!admin) {
            headers['X-Admin-Settings-Version'] = admin.settingsVersion || '???';
            headers['X-Admin-Permissions-Version'] = admin.permissionsVersion || '???';
            headers['X-Admin-User-Settings-Version'] = admin.userSettingsVersion || '???';
          }
        }
      }

      if (!isVersionRefreshRequest) {
        // global versions
        if (!!this.workspace.globalSettingsVersion) {
          headers['X-Global-Settings-Version'] = this.workspace.globalSettingsVersion;
        }
      }

      // clone the request and set the headers and parameters
      req = req.clone({
        setHeaders: headers,
        setParams: params
      });
    }

    // Here we intercept the response
    return next.handle(req).pipe(
      tap(e => {
        if (e instanceof HttpResponseBase) {
          this.handleServerVersions(e, tenantId);

          // The client is definitely online
          if (this.workspace.offline) {
            this.workspace.offline = false;
            this.workspace.notifyStateChanged();
          }
        }
      }),
      catchError(e => {
        if (e instanceof HttpResponseBase) {
          this.handleServerVersions(e, tenantId);

          // If it's a 401 then quickly delete the app state and challenge user
          if (e.status === 401) {
            this.router.navigateByUrl('/welcome?error=401').then(() => {
              this.cleaner.cleanState();
            });
          }

          if (e.status === 0 || e.status === 504) {
            // The client is probably offline
            if (!this.workspace.offline) {
              this.workspace.offline = true;
              this.workspace.notifyStateChanged();
            }

            // By the time we reach here, the first observable isnt over yet
            // if we call ping immediately, the exhaustmap will not run it, therefore we add this timer(1)
            timer(1).subscribe(() => this.pingAfterOneSecond());
          }
        }

        return throwError(e);
      })
    );
  }

  private handleServerVersions = (e: HttpResponseBase, tenantId: number) => {

    if (!!e && !!e.headers) {

      // Global
      {
        // global settings
        const v = e.headers.get('x-global-settings-version') as VersionStatus;
        if (v === 'Stale') {
          this.refreshGlobalSettings();
        }
      }

      // Admin Console
      {
        // admin settings
        {
          const v = e.headers.get('x-admin-settings-version') as VersionStatus;
          if (v === 'Stale') {
            this.refreshAdminSettings();
          }

          if (v === 'Unauthorized') {
            // this means the user can no longer access the admin console

            // (1) triggers a refresh next time the user navigates to "my companies"
            this.workspace.ws.companiesStatus = null;

            // (2) Delete from local storage everything related
            this.storage.removeItem(adminStorageKey(ADMIN_SETTINGS_PREFIX));
            this.storage.removeItem(adminVersionStorageKey(ADMIN_SETTINGS_PREFIX));
            this.storage.removeItem(adminStorageKey(ADMIN_PERMISSIONS_PREFIX));
            this.storage.removeItem(adminVersionStorageKey(ADMIN_PERMISSIONS_PREFIX));
            this.storage.removeItem(adminStorageKey(ADMIN_USER_SETTINGS_PREFIX));
            this.storage.removeItem(adminVersionStorageKey(ADMIN_USER_SETTINGS_PREFIX));

            // (3) Take the user to unauthorized screen and then delete the workspace
            this.workspace.ws.admin.unauthorized = true;
            this.router.navigate(['root', 'error', 'admin-unauthorized']).then(() => {
              delete this.workspace.ws.admin;
            });
          }
        }

        // admin permissions
        {
          const v = e.headers.get('x-admin-permissions-version') as VersionStatus;
          if (v === 'Stale') {
            this.refreshAdminPermissions();
          }
        }

        // admin user settings
        {
          const v = e.headers.get('x-admin-user-settings-version') as VersionStatus;
          if (v === 'Stale') {
            this.refreshAdminUserSettings();
          }
        }
      }

      // Application
      if (!!tenantId) {
        // settings
        {
          const v = e.headers.get('x-settings-version') as VersionStatus;
          if (v === 'Stale') {
            this.refreshSettings();
          }

          if (v === 'Unauthorized') {
            // this means the user is no longer a member of this tenant
            // (1) triggers a refresh next time the user navigates to "my companies"
            this.workspace.ws.companiesStatus = null;

            // (2) Delete from local storage everything related
            this.storage.removeItem(storageKey(SETTINGS_PREFIX, tenantId));
            this.storage.removeItem(versionStorageKey(SETTINGS_PREFIX, tenantId));
            this.storage.removeItem(storageKey(DEFINITIONS_PREFIX, tenantId));
            this.storage.removeItem(versionStorageKey(DEFINITIONS_PREFIX, tenantId));
            this.storage.removeItem(storageKey(PERMISSIONS_PREFIX, tenantId));
            this.storage.removeItem(versionStorageKey(PERMISSIONS_PREFIX, tenantId));
            this.storage.removeItem(storageKey(USER_SETTINGS_PREFIX, tenantId));
            this.storage.removeItem(versionStorageKey(USER_SETTINGS_PREFIX, tenantId));

            // (3) Take the user to unauthorized screen and then clean the workspace
            this.workspace.ws.tenants[tenantId].unauthorized = true;
            this.router.navigate(['root', 'error', 'unauthorized']).then(() => {
              delete this.workspace.ws.tenants[tenantId];
            });
          }
        }

        // definitions
        {
          const v = e.headers.get('x-definitions-version') as VersionStatus;
          if (v === 'Stale') {
            this.refreshDefinitions();
          }
        }

        // permissions
        {
          const v = e.headers.get('x-permissions-version') as VersionStatus;
          if (v === 'Stale') {
            this.refreshPermissions();
          }
        }

        // user settings
        {
          const v = e.headers.get('x-user-settings-version') as VersionStatus;
          if (v === 'Stale') {
            this.refreshUserSettings();
          }
        }

      }
    }
  }

  // Global

  private refreshGlobalSettings() {
    this.notifyRefreshGlobalSettings$.next();
  }

  private doRefreshGlobalSettings() {

    const ws = this.workspace;
    const obs$ = this.globalSettingsApi().pipe(
      tap(result => {
        // Cache the permissions and set them in the workspace
        handleFreshGlobalSettings(result, ws, this.storage);
      }),
      retry(2),
      catchError(_ => of(null))
    );

    return obs$;
  }

  // Admin Console

  private refreshAdminSettings() {
    this.notifyRefreshAdminSettings$.next();
  }

  private doRefreshAdminSettings = () => {
    if (!this.workspace.isAdmin) {
      // It means the user switched to a company before
      // the stale report came back
      return of(null);
    }

    const current = this.workspace.admin;
    const obs$ = this.adminSettingsApi().pipe(
      tap(result => {
        // Cache the admin settings and set them in the workspace
        handleFreshAdminSettings(result, current, this.storage);
      }),
      catchError((err: { status: number, error: any }) => {
        if (err.status === 403) {
          // Delete all cached information
          delete this.workspace.ws.admin;
        } else {
          return throwError(err);
        }
      }),
      retry(2),
      catchError(_ => of(null))
    );

    return obs$;
  }

  private refreshAdminPermissions() {
    this.notifyRefreshAdminPermissions$.next();
  }

  private doRefreshAdminPermissions = () => {
    if (!this.workspace.isAdmin) {
      // It means the user switched to a company before
      // the stale report came back
      return of(null);
    }

    const current = this.workspace.admin;
    const obs$ = this.adminPermissionsApi().pipe(
      tap(result => {
        // Cache the admin permissions and set them in the workspace
        handleFreshAdminPermissions(result, current, this.storage);
      }),
      retry(2),
      catchError(_ => of(null))
    );

    return obs$;
  }

  private refreshAdminUserSettings() {
    this.notifyRefreshAdminUserSettings$.next();
  }

  private doRefreshAdminUserSettings = () => {
    if (!this.workspace.isAdmin) {
      // It means the user switched to a company before
      // the stale report came back
      return of(null);
    }

    const current = this.workspace.admin;
    const obs$ = this.adminUserSettingsApi().pipe(
      tap(result => {
        // Cache the user settings and set them in the workspace
        handleFreshAdminUserSettings(result, current, this.storage);
      }),
      retry(2),
      catchError(_ => of(null))
    );

    return obs$;
  }

  // Application

  private refreshDefinitions() {
    this.notifyRefreshDefinitions$.next();
  }

  private doRefreshDefinitions = () => {
    if (!this.workspace.isApp) {
      // It means the user switched to admin console before
      // the stale report came back
      return of(null);
    }

    const current = this.workspace.currentTenant;
    const tenantId = this.workspace.ws.tenantId;

    const obs$ = this.definitionsApi().pipe(
      tap(result => {
        // Cache the definitions and set them in the workspace
        handleFreshDefinitions(result, tenantId, current, this.storage);
      }),
      catchError((err: { status: number, error: any }) => {
        if (err.status === 403) {
          // Delete all cached information
          delete this.workspace.ws.tenants[tenantId];
        } else {
          return throwError(err);
        }
      }),
      retry(2),
      catchError(_ => of(null))
    );

    return obs$;
  }

  private refreshSettings() {
    this.notifyRefreshSettings$.next();
  }

  private doRefreshSettings = () => {
    if (!this.workspace.isApp) {
      // It means the user switched to admin console before
      // the stale report came back
      return of(null);
    }

    const current = this.workspace.currentTenant;
    const tenantId = this.workspace.ws.tenantId;

    const obs$ = this.settingsApi().pipe(
      tap(result => {
        // Cache the settings and set them in the workspace
        handleFreshSettings(result, tenantId, current, this.storage);
      }),
      catchError((err: { status: number, error: any }) => {
        if (err.status === 403) {
          // Delete all cached information
          delete this.workspace.ws.tenants[tenantId];
        } else {
          return throwError(err);
        }
      }),
      retry(2),
      catchError(_ => of(null))
    );

    return obs$;
  }

  private refreshPermissions() {
    this.notifyRefreshPermissions$.next();
  }

  private doRefreshPermissions = () => {
    if (!this.workspace.isApp) {
      // It means the user switched to admin console before
      // the stale report came back
      return of(null);
    }

    const current = this.workspace.currentTenant;
    const tenantId = this.workspace.ws.tenantId;

    const obs$ = this.permissionsApi().pipe(
      tap(result => {
        // Cache the permissions and set them in the workspace
        handleFreshPermissions(result, tenantId, current, this.storage);
      }),
      retry(2),
      catchError(_ => of(null))
    );

    return obs$;
  }

  private refreshUserSettings() {
    this.notifyRefreshUserSettings$.next();
  }

  private doRefreshUserSettings = () => {
    if (!this.workspace.isApp) {
      // It means the user switched to admin console before
      // the stale report came back
      return of(null);
    }

    const current = this.workspace.currentTenant;
    const tenantId = this.workspace.ws.tenantId;

    const obs$ = this.userSettingsApi().pipe(
      tap(result => {
        // Cache the user settings and set them in the workspace
        handleFreshUserSettings(result, tenantId, current, this.storage);
      }),
      retry(2),
      catchError(_ => of(null))
    );

    return obs$;
  }

  // Misc.

  private pingAfterOneSecond(): void {
    this.notifyPingAfterOneSecond$.next();
  }

  private doPingAfterOneSecond(): Observable<void> {
    const obs$ = timer(1000).pipe(
      concatMap(_ => this.pingApi()),
      catchError(_ => of(null))
    );

    return obs$;
  }

}
