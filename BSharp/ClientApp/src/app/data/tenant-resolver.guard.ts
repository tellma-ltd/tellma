import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, RouterStateSnapshot, Router, Resolve, Params } from '@angular/router';
import { Observable, Subject, forkJoin, of } from 'rxjs';
import { WorkspaceService, TenantWorkspace } from './workspace.service';
import { StorageService } from './storage.service';
import { SettingsForClient } from './dto/settings';
import { ApiService } from './api.service';
import { DataWithVersion } from './dto/data-with-version';
import { PermissionsForClient } from './dto/permission';
import { tap, map, catchError, finalize, retry } from 'rxjs/operators';
import { CanActivate } from '@angular/router';

export const SETTINGS_PREFIX = 'settings';
export const PERMISSIONS_PREFIX = 'permissions';

export function storageKey(prefix: string, tenantId: number) { return `${prefix}_${tenantId}`; }
export function versionStorageKey(prefix: string, tenantId: number) { return `${prefix}_${tenantId}_version`; }
export function handleFreshSettings(result: DataWithVersion<SettingsForClient>,
  tenantId: number, tws: TenantWorkspace, storage: StorageService) {

  const settings = result.Data;
  const version = result.Version;
  const prefix = SETTINGS_PREFIX;
  storage.setItem(storageKey(prefix, tenantId), JSON.stringify(settings));
  storage.setItem(versionStorageKey(prefix, tenantId), version);

  tws.settings = settings;
  tws.settingsVersion = version;
}

export function handleFreshPermissions(result: DataWithVersion<PermissionsForClient>,
  tenantId: number, tws: TenantWorkspace, storage: StorageService) {

  const permissions = result.Data;
  const version = result.Version;
  const prefix = PERMISSIONS_PREFIX;
  storage.setItem(storageKey(prefix, tenantId), JSON.stringify(permissions));
  storage.setItem(versionStorageKey(prefix, tenantId), version);

  tws.permissions = permissions;
  tws.permissionsVersion = version;
}

@Injectable({
  providedIn: 'root'
})
export class TenantResolverGuard implements CanActivate {

  // Note: we used a guard here instead of a resolver to guarantee that the user cannot
  // navigate to the application until the global values of that tenant are resolved first

  private cancellationToken$: Subject<void>;
  private settingsApi: () => Observable<DataWithVersion<SettingsForClient>>;
  private permissionsApi: () => Observable<DataWithVersion<PermissionsForClient>>;
  private ping: () => Observable<any>;

  constructor(private workspace: WorkspaceService, private storage: StorageService,
    private router: Router, private api: ApiService) {

    this.cancellationToken$ = new Subject<void>();
    const settingsApi = this.api.settingsApi(this.cancellationToken$);
    this.settingsApi = settingsApi.getForClient;
    this.ping = settingsApi.ping;
    this.permissionsApi = this.api.permissionsApi(this.cancellationToken$).getForClient;
  }

  canActivate(next: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean | Observable<boolean> {
    const tenantIdSring = next.paramMap.get('tenantId');
    if (!!tenantIdSring) {
      const tenantId = +tenantIdSring;
      if (!!tenantId) {
        // set the Tenant ID
        this.workspace.ws.tenantId = tenantId;

        // take a concrete reference just in case it changes
        const current = this.workspace.current;

        // check settings
        const getSettingsFromStorage = () => {

          // Try to retrieve the settings from local storage
          const prefix = SETTINGS_PREFIX;
          const cachedSettings = <SettingsForClient>JSON.parse(this.storage.getItem(storageKey(prefix, tenantId)));
          const cachedSettingsVersion = this.storage.getItem(versionStorageKey(prefix, tenantId));
          if (!!cachedSettings) {
            current.settings = cachedSettings;
            current.settingsVersion = cachedSettingsVersion || '???';
          }
        };

        if (!current.settings) {
          getSettingsFromStorage();
        }

        // check permissions
        const getPermissionsFromStorage = () => {

          // Try to retrieve the permissions from local storage
          const prefix = PERMISSIONS_PREFIX;
          const cachedPermissions = <PermissionsForClient>JSON.parse(this.storage.getItem(storageKey(prefix, tenantId)));
          const cachedPermissionsVersion = this.storage.getItem(versionStorageKey(prefix, tenantId));
          if (!!cachedPermissions) {
            current.permissions = cachedPermissions;
            current.permissionsVersion = cachedPermissionsVersion || '???';
          }
        };

        // check permissions
        if (!current.permissions) {
          getPermissionsFromStorage();
        }

        // subscribe to changes in the storage
        // If the user signs out or signs in from another window
        // we automatically sign out/in in this window in order to
        // achieve a consistent experience
        if (!!addEventListener) {
          addEventListener('storage', (e: StorageEvent) => {
            // settings
            const settingsKey = versionStorageKey(SETTINGS_PREFIX, tenantId);
            if (e.key === settingsKey) {
              if (e.newValue !== current.settingsVersion) {
                getSettingsFromStorage();
              }
            }

            // permissions
            const permissionsKey = versionStorageKey(PERMISSIONS_PREFIX, tenantId);
            if (e.key === permissionsKey) {
              if (e.newValue !== current.permissionsVersion) {
                getPermissionsFromStorage();
              }
            }
          }, false);
        }

        // IF this is a new browser/machine, need to get the globals from the backend
        if (current.settings && current.permissions) {
          // In case our cached globals are stale this will trigger their refresh
          this.ping().pipe(retry(2)).subscribe();

          // we can log in to the tenant immediately based on the cached globals, don't wait till they are refreshed
          return true;
        } else {

          this.api.saveInProgress = true; // To show the rotator

          // using forkJoin is recommended for running HTTP calls in parallel
          const obs$ = forkJoin(this.settingsApi(), this.permissionsApi()).pipe(
            tap(result => {
              this.api.saveInProgress = false;
              // cache the settings and set it in the workspace
              handleFreshSettings(result[0], tenantId, current, this.storage);
              handleFreshPermissions(result[1], tenantId, current, this.storage);
            }),
            map(() => true),
            catchError((err: { status: number, error: any }) => {
              this.api.saveInProgress = false;

              if (err.status === 403) {
                this.router.navigate(['unauthorized']);
              } else {
                this.workspace.ws.errorLoadingCompanyMessage = err.error;
                this.router.navigate(['error-loading-company'], { queryParams: { retryUrl: state.url }});
              }

              // Prevent navigation
              return of(false);
            }),
            finalize(() => {
              this.api.saveInProgress = false;
            })
          );

          return obs$;
        }
      }
    }

    console.log('from guard!');
    this.router.navigate(['page-not-found']);
    return false;
  }
}
