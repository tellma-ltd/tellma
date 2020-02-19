import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { Observable, Subject, forkJoin, of } from 'rxjs';
import { WorkspaceService, TenantWorkspace } from './workspace.service';
import { StorageService } from './storage.service';
import { SettingsForClient } from './dto/settings-for-client';
import { ApiService } from './api.service';
import { DataWithVersion } from './dto/data-with-version';
import { PermissionsForClient } from './dto/permissions-for-client';
import { tap, map, catchError, finalize, retry } from 'rxjs/operators';
import { CanActivate } from '@angular/router';
import { UserSettingsForClient } from './dto/user-settings-for-client';
import { ProgressOverlayService } from './progress-overlay.service';
import { DefinitionsForClient } from './dto/definitions-for-client';

export const SETTINGS_PREFIX = 'settings';
export const DEFINITIONS_PREFIX = 'definitions';
export const PERMISSIONS_PREFIX = 'permissions';
export const USER_SETTINGS_PREFIX = 'user_settings';

// Those are incremented when the structure of the data changes
export const SETTINGS_METAVERSION = '1.1';
export const DEFINITIONS_METAVERSION = '1.0';
export const PERMISSIONS_METAVERSION = '1.0';
export const USER_SETTINGS_METAVERSION = '1.0';

export function storageKey(prefix: string, tenantId: number) { return `${prefix}_${tenantId}`; }
export function versionStorageKey(prefix: string, tenantId: number) { return `${prefix}_${tenantId}_version`; }
export function metaVersionStorageKey(prefix: string, tenantId: number) { return `${prefix}_${tenantId}_metaversion`; }

export function handleFreshSettings(
  result: DataWithVersion<SettingsForClient>,
  tenantId: number, tws: TenantWorkspace, storage: StorageService) {

  const settings = result.Data;
  const version = result.Version;
  const prefix = SETTINGS_PREFIX;
  const metaversion = SETTINGS_METAVERSION;
  storage.setItem(storageKey(prefix, tenantId), JSON.stringify(settings));
  storage.setItem(versionStorageKey(prefix, tenantId), version);
  storage.setItem(metaVersionStorageKey(prefix, tenantId), metaversion);

  tws.settings = settings;
  tws.settingsVersion = version;
  tws.notifyStateChanged();
}

export function handleFreshDefinitions(
  result: DataWithVersion<DefinitionsForClient>,
  tenantId: number, tws: TenantWorkspace, storage: StorageService) {

  const definitions = result.Data;
  const version = result.Version;
  const prefix = DEFINITIONS_PREFIX;
  const metaversion = DEFINITIONS_METAVERSION;
  storage.setItem(storageKey(prefix, tenantId), JSON.stringify(definitions));
  storage.setItem(versionStorageKey(prefix, tenantId), version);
  storage.setItem(metaVersionStorageKey(prefix, tenantId), metaversion);

  tws.definitions = definitions;
  tws.definitionsVersion = version;
  tws.notifyStateChanged();
}

export function handleFreshPermissions(
  result: DataWithVersion<PermissionsForClient>,
  tenantId: number, tws: TenantWorkspace, storage: StorageService) {

  const permissions = result.Data;
  const version = result.Version;
  const prefix = PERMISSIONS_PREFIX;
  const metaversion = PERMISSIONS_METAVERSION;
  storage.setItem(storageKey(prefix, tenantId), JSON.stringify(permissions));
  storage.setItem(versionStorageKey(prefix, tenantId), version);
  storage.setItem(metaVersionStorageKey(prefix, tenantId), metaversion);

  tws.permissions = permissions;
  tws.permissionsVersion = version;
  tws.notifyStateChanged();
}

export function handleFreshUserSettings(
  result: DataWithVersion<UserSettingsForClient>,
  tenantId: number, tws: TenantWorkspace, storage: StorageService) {

  const userSettings = result.Data;
  const version = result.Version;
  const prefix = USER_SETTINGS_PREFIX;
  const metaversion = USER_SETTINGS_METAVERSION;
  storage.setItem(storageKey(prefix, tenantId), JSON.stringify(userSettings));
  storage.setItem(versionStorageKey(prefix, tenantId), version);
  storage.setItem(metaVersionStorageKey(prefix, tenantId), metaversion);

  tws.userSettings = userSettings;
  tws.userSettingsVersion = version;
  tws.notifyStateChanged();
}

@Injectable({
  providedIn: 'root'
})
export class TenantResolverGuard implements CanActivate {

  // Note: we used a guard here instead of a resolver to guarantee that the user cannot
  // navigate to the application until the global values of that tenant are resolved first

  private cancellationToken$: Subject<void>;
  private settingsApi: () => Observable<DataWithVersion<SettingsForClient>>;
  private definitionsApi: () => Observable<DataWithVersion<DefinitionsForClient>>;
  private permissionsApi: () => Observable<DataWithVersion<PermissionsForClient>>;
  private userSettingsApi: () => Observable<DataWithVersion<UserSettingsForClient>>;
  private ping: () => Observable<any>;

  constructor(
    private workspace: WorkspaceService, private storage: StorageService,
    private router: Router, private api: ApiService, private progress: ProgressOverlayService) {

    this.cancellationToken$ = new Subject<void>();
    const settingsApi = this.api.settingsApi(this.cancellationToken$);
    this.settingsApi = settingsApi.getForClient;
    this.ping = settingsApi.ping;
    this.definitionsApi = this.api.definitionsApi(this.cancellationToken$).getForClient;
    this.permissionsApi = this.api.permissionsApi(this.cancellationToken$).getForClient;
    this.userSettingsApi = this.api.usersApi(this.cancellationToken$).getForClient;
  }

  canActivate(next: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean | Observable<boolean> {
    const tenantIdSring = next.paramMap.get('tenantId');
    if (!!tenantIdSring) {
      const tenantId = +tenantIdSring;
      if (!!tenantId) {
        // set the Tenant ID
        this.workspace.setTenantId(tenantId);

        // take a concrete reference just in case it changes
        const current = this.workspace.currentTenant;

        // check settings
        const getSettingsFromStorage = () => {

          // Try to retrieve the settings from local storage
          const prefix = SETTINGS_PREFIX;
          const cachedSettings = JSON.parse(this.storage.getItem(storageKey(prefix, tenantId))) as SettingsForClient;
          const cachedSettingsVersion = this.storage.getItem(versionStorageKey(prefix, tenantId));
          const cachedSettingsMetaVersion = this.storage.getItem(metaVersionStorageKey(prefix, tenantId));
          if (!!cachedSettings && cachedSettingsMetaVersion === SETTINGS_METAVERSION) {
            current.settings = cachedSettings;
            current.settingsVersion = cachedSettingsVersion || '???';
          }
        };

        if (!current.settings) {
          getSettingsFromStorage();
        }

        // check definitions
        const getDefinitionsFromStorage = () => {

          // Try to retrieve the definitions from local storage
          const prefix = DEFINITIONS_PREFIX;
          const cachedDefinitions = JSON.parse(this.storage.getItem(storageKey(prefix, tenantId))) as DefinitionsForClient;
          const cachedDefinitionsVersion = this.storage.getItem(versionStorageKey(prefix, tenantId));
          const cachedDefinitionsMetaVersion = this.storage.getItem(metaVersionStorageKey(prefix, tenantId));
          if (!!cachedDefinitions && cachedDefinitionsMetaVersion === DEFINITIONS_METAVERSION) {
            current.definitions = cachedDefinitions;
            current.definitionsVersion = cachedDefinitionsVersion || '???';
          }
        };

        if (!current.definitions) {
          getDefinitionsFromStorage();
        }

        // check permissions
        const getPermissionsFromStorage = () => {

          // Try to retrieve the permissions from local storage
          const prefix = PERMISSIONS_PREFIX;
          const cachedPermissions = JSON.parse(this.storage.getItem(storageKey(prefix, tenantId))) as PermissionsForClient;
          const cachedPermissionsVersion = this.storage.getItem(versionStorageKey(prefix, tenantId));
          const cachedPermissionsMetaVersion = this.storage.getItem(metaVersionStorageKey(prefix, tenantId));
          if (!!cachedPermissions && cachedPermissionsMetaVersion === PERMISSIONS_METAVERSION) {
            current.permissions = cachedPermissions;
            current.permissionsVersion = cachedPermissionsVersion || '???';
          }
        };

        // check permissions
        if (!current.permissions) {
          getPermissionsFromStorage();
        }

        // check user settings
        const getUserSettingsFromStorage = () => {

          // Try to retrieve the user settings from local storage
          const prefix = USER_SETTINGS_PREFIX;
          const cachedUserSettings = JSON.parse(this.storage.getItem(storageKey(prefix, tenantId))) as UserSettingsForClient;
          const cachedUserSettingsVersion = this.storage.getItem(versionStorageKey(prefix, tenantId));
          const cachedUserSettingsMetaVersion = this.storage.getItem(metaVersionStorageKey(prefix, tenantId));
          if (!!cachedUserSettings && cachedUserSettingsMetaVersion === USER_SETTINGS_METAVERSION) {

            current.userSettings = cachedUserSettings;
            current.userSettingsVersion = cachedUserSettingsVersion || '???';
          }
        };

        // check user settings
        if (!current.userSettings) {
          getUserSettingsFromStorage();
        }

        // subscribe to changes in the storage
        // If the user signs out or signs in from another window
        // we automatically sign out/in in this window in order to
        // achieve a consistent experience

        this.storage.changed$.subscribe(e => {
          // settings
          const settingsKey = versionStorageKey(SETTINGS_PREFIX, tenantId);
          if (e.key === settingsKey) {
            if (e.newValue !== current.settingsVersion) {
              getSettingsFromStorage();
            }
          }

          // definitions
          const definitionsKey = versionStorageKey(DEFINITIONS_PREFIX, tenantId);
          if (e.key === definitionsKey) {
            if (e.newValue !== current.definitionsVersion) {
              getDefinitionsFromStorage();
            }
          }

          // permissions
          const permissionsKey = versionStorageKey(PERMISSIONS_PREFIX, tenantId);
          if (e.key === permissionsKey) {
            if (e.newValue !== current.permissionsVersion) {
              getPermissionsFromStorage();
            }
          }

          // user settings
          const userSettingsKey = versionStorageKey(USER_SETTINGS_PREFIX, tenantId);
          if (e.key === userSettingsKey) {
            if (e.newValue !== current.userSettingsVersion) {
              getUserSettingsFromStorage();
            }
          }

        });

        // IF this is a new browser/machine, need to get the globals from the backend
        if (current.settings && current.definitionsVersion && current.permissions && current.userSettings) {
          // In case our cached globals are stale this will trigger their refresh
          this.ping().pipe(retry(2)).subscribe();

          // we can log in to the tenant immediately based on the cached globals, don't wait till they are refreshed
          return true;
        } else {
          const key = `tenant_${tenantId}`;
          this.progress.startAsyncOperation(key, 'LoadingCompanySettings'); // To show the rotator

          // using forkJoin is recommended for running HTTP calls in parallel
          const obs$ = forkJoin(this.settingsApi(), this.definitionsApi(), this.permissionsApi(), this.userSettingsApi()).pipe(
            tap(result => {
              this.progress.completeAsyncOperation(key);
              // cache the settings and set it in the workspace
              handleFreshSettings(result[0], tenantId, current, this.storage);
              handleFreshDefinitions(result[1], tenantId, current, this.storage);
              handleFreshPermissions(result[2], tenantId, current, this.storage);
              handleFreshUserSettings(result[3], tenantId, current, this.storage);
            }),
            map(() => true),
            catchError((err: { status: number, error: any }) => {
              this.progress.completeAsyncOperation(key);

              if (err.status === 403) {
                this.router.navigate(['root', 'error', 'unauthorized'], { queryParams: { retryUrl: state.url } });
              } else {
                this.workspace.ws.errorMessage = err.error;
                this.router.navigate(['root', 'error', 'loading-company-settings'], { queryParams: { retryUrl: state.url } });
              }

              // Prevent navigation
              return of(false);
            }),
            finalize(() => {
              this.progress.completeAsyncOperation(key);
            })
          );

          return obs$;
        }
      }
    }

    this.router.navigate(['page-not-found']);
    return false;
  }
}
