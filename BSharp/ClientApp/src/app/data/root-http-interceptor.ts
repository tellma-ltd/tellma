import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HttpResponse } from '@angular/common/http';
import { Observable, Subject, of, throwError } from 'rxjs';
import { WorkspaceService } from './workspace.service';
import { tap, exhaustMap, retry, catchError } from 'rxjs/operators';
import { ApiService } from './api.service';
import { DataWithVersion } from './dto/data-with-version';
import { SettingsForClient } from './dto/settings';
import { PermissionsForClient } from './dto/permission';
import { StorageService } from './storage.service';
import {
  handleFreshSettings, handleFreshPermissions, versionStorageKey,
  storageKey, SETTINGS_PREFIX, PERMISSIONS_PREFIX
} from './tenant-resolver.guard';
import { Router } from '@angular/router';

type VersionStatus = 'Fresh' | 'Stale' | 'Unauthorized';

export class RootHttpInterceptor implements HttpInterceptor {

  private notifyRefreshSettings$: Subject<void>;
  private notifyRefreshPermissions$: Subject<void>;
  private cancellationToken$: Subject<void>;
  private settingsApi: () => Observable<DataWithVersion<SettingsForClient>>;
  private permissionsApi: () => Observable<DataWithVersion<PermissionsForClient>>;

  constructor(private workspace: WorkspaceService, private api: ApiService, private storage: StorageService, private router: Router) {
    // Note: We use exhaustMap to prevent making another call while a call is in progress
    // https://www.learnrxjs.io/operators/transformation/exhaustmap.html
    this.notifyRefreshSettings$ = new Subject<void>();
    this.notifyRefreshSettings$.pipe(
      exhaustMap(() => this.doRefreshSettings())
    ).subscribe();

    this.notifyRefreshPermissions$ = new Subject<void>();
    this.notifyRefreshPermissions$.pipe(
      exhaustMap(() => this.doRefreshPermissions())
    ).subscribe();

    this.cancellationToken$ = new Subject<void>();
    this.settingsApi = this.api.settingsApi(this.cancellationToken$).getForClient;
    this.permissionsApi = this.api.permissionsApi(this.cancellationToken$).getForClient;
  }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {


    // we accumulate all the headers params in these objects
    const headers = {};
    const params = {};

    // cache buster
    params['t'] = encodeURIComponent(new Date().getTime().toString());

    // tenant ID
    const tenantId = this.workspace.ws.tenantId;
    if (!!tenantId) {
      // This piece of logic does not really belong to the root module and is
      // specific to the application module, but moving it there is not worth
      // the hassle now
      headers['X-Tenant-Id'] = tenantId.toString();
    }

    // UI culture
    const culture = this.workspace.ws.culture;
    if (!!culture) {
      params['ui-culture'] = culture;
    }

    // global versions
    const current = this.workspace.current;
    if (!!current) {
      headers['X-Settings-Version'] = current.settingsVersion || '???';
      headers['X-Permissions-Version'] = current.permissionsVersion || '???';
    }

    // clone the request and set the headers and parameters
    req = req.clone({
      setHeaders: headers,
      setParams: params
    });

    // TODO add authorization header
    // TODO add cache versions and intercept responses
    // TODO intercept 401 responses and log the user out

    return next.handle(req).pipe(
      tap(e => this.handleServerVersions(e, tenantId)),
      catchError(e => {
        this.handleServerVersions(e, tenantId);
        return throwError(e);
      })
    );
  }

  handleServerVersions = (e: any, tenantId: number) => {
    if (!!e && !!e.headers && tenantId) {
      // settings
      {
        const v = <VersionStatus>e.headers.get('x-settings-version');
        if (v === 'Stale') {
          this.refreshSettings();
        }

        if (v === 'Unauthorized') {
          // This means the user is no longer a member of this tenant
          // (1) Delete the workspace of this tenant
          delete this.workspace.ws.tenants[tenantId];

          // (2) Delete from local storage everything related
          this.storage.removeItem(storageKey(SETTINGS_PREFIX, tenantId));
          this.storage.removeItem(versionStorageKey(SETTINGS_PREFIX, tenantId));
          this.storage.removeItem(storageKey(PERMISSIONS_PREFIX, tenantId));
          this.storage.removeItem(versionStorageKey(PERMISSIONS_PREFIX, tenantId));

          // (3) Take the user to unauthorized screen
          this.router.navigate(['unauthorized']);
        }
      }

      // permissions
      {
        const v = <VersionStatus>e.headers.get('x-permissions-version');
        if (v === 'Stale') {
          this.refreshPermissions();
        }
      }
    }
  }

  refreshSettings() {
    this.notifyRefreshSettings$.next();
  }

  doRefreshSettings = () => {

    const current = this.workspace.current;
    const tenantId = this.workspace.ws.tenantId;

    // using forkJoin is recommended for running HTTP calls in parallel
    const obs$ = this.settingsApi().pipe(
      tap(result => {
        // Cache the settings and set them in the workspace
        handleFreshSettings(result, tenantId, current, this.storage);
      }),
      catchError((err: { status: number, error: any }) => {
        if (err.status === 403) {
          // Delete all cached information
          delete this.workspace.ws.tenants[tenantId];
          // TODO: Clear tenant workspace and navigate to you cannot access this company page
        } else {
          return throwError(err);
        }
      }),
      retry(2)
    );

    return obs$;
  }

  refreshPermissions() {
    this.notifyRefreshPermissions$.next();
  }

  doRefreshPermissions = () => {

    const current = this.workspace.current;
    const tenantId = this.workspace.ws.tenantId;

    // using forkJoin is recommended for running HTTP calls in parallel
    const obs$ = this.permissionsApi().pipe(
      tap(result => {
        // Cache the permissions and set them in the workspace
        handleFreshPermissions(result, tenantId, current, this.storage);
      }),
      retry(2)
    );

    return obs$;
  }

  clearTenant(tenantId: number) {

  }

}
