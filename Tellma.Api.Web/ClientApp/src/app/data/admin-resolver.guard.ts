import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { Observable, Subject, forkJoin, of } from 'rxjs';
import { WorkspaceService, AdminWorkspace } from './workspace.service';
import { StorageService } from './storage.service';
import { ApiService } from './api.service';
import { Versioned } from './dto/versioned';
import { tap, map, catchError, finalize, retry } from 'rxjs/operators';

import { ProgressOverlayService } from './progress-overlay.service';
import { AdminSettingsForClient } from './dto/admin-settings-for-client';
import { AdminUserSettingsForClient } from './dto/admin-user-settings-for-client';
import { AdminPermissionsForClient } from './dto/admin-permissions-for-client';
import { transformPermissions } from './util';

export const ADMIN_SETTINGS_PREFIX = 'admin_settings';
export const ADMIN_PERMISSIONS_PREFIX = 'admin_permissions';
export const ADMIN_USER_SETTINGS_PREFIX = 'admin_user_settings';

// Those are incremented when the structure of the data changes
export const ADMIN_SETTINGS_METAVERSION = '1.0';
export const ADMIN_PERMISSIONS_METAVERSION = '1.1';
export const ADMIN_USER_SETTINGS_METAVERSION = '1.0';

export function storageKey(prefix: string) { return `${prefix}`; }
export function versionStorageKey(prefix: string) { return `${prefix}_version`; }
export function metaVersionStorageKey(prefix: string) { return `${prefix}_metaversion`; }

export function handleFreshSettings(
  result: Versioned<AdminSettingsForClient>, aws: AdminWorkspace, storage: StorageService) {

  const settings = result.Data;
  const version = result.Version;
  const prefix = ADMIN_SETTINGS_PREFIX;
  const metaversion = ADMIN_SETTINGS_METAVERSION;
  storage.setItem(storageKey(prefix), JSON.stringify(settings));
  storage.setItem(versionStorageKey(prefix), version);
  storage.setItem(metaVersionStorageKey(prefix), metaversion);

  aws.settings = settings;
  aws.settingsVersion = version;
  aws.notifyStateChanged();
}

export function handleFreshPermissions(
  result: Versioned<AdminPermissionsForClient>, aws: AdminWorkspace, storage: StorageService) {

  transformPermissions(result.Data);

  const permissions = result.Data;
  const version = result.Version;
  const prefix = ADMIN_PERMISSIONS_PREFIX;
  const metaversion = ADMIN_PERMISSIONS_METAVERSION;
  storage.setItem(storageKey(prefix), JSON.stringify(permissions));
  storage.setItem(versionStorageKey(prefix), version);
  storage.setItem(metaVersionStorageKey(prefix), metaversion);

  aws.permissions = permissions.Views;
  aws.permissionsVersion = version;
  aws.notifyStateChanged();
}

export function handleFreshUserSettings(
  result: Versioned<AdminUserSettingsForClient>, aws: AdminWorkspace, storage: StorageService) {

  const userSettings = result.Data;
  const version = result.Version;
  const prefix = ADMIN_USER_SETTINGS_PREFIX;
  const metaversion = ADMIN_USER_SETTINGS_METAVERSION;
  storage.setItem(storageKey(prefix), JSON.stringify(userSettings));
  storage.setItem(versionStorageKey(prefix), version);
  storage.setItem(metaVersionStorageKey(prefix), metaversion);

  aws.userSettings = userSettings;
  aws.userSettingsVersion = version;
  aws.notifyStateChanged();
}

@Injectable({
  providedIn: 'root'
})
export class AdminResolverGuard  {

  // Note: we used a guard here instead of a resolver to guarantee that the user cannot
  // navigate to the admin portal until the global values are retrieved first

  private cancellationToken$: Subject<void>;
  private settingsApi: () => Observable<Versioned<AdminSettingsForClient>>;
  private permissionsApi: () => Observable<Versioned<AdminPermissionsForClient>>;
  private userSettingsApi: () => Observable<Versioned<AdminUserSettingsForClient>>;
  private ping: () => Observable<any>;

  constructor(
    private workspace: WorkspaceService, private storage: StorageService,
    private router: Router, private api: ApiService, private progress: ProgressOverlayService) {

    this.cancellationToken$ = new Subject<void>();
    const settingsApi = this.api.adminSettingsApi(this.cancellationToken$);
    this.settingsApi = settingsApi.getForClient;
    this.ping = settingsApi.ping;
    this.permissionsApi = this.api.adminPermissionsApi(this.cancellationToken$).getForClient;
    this.userSettingsApi = this.api.adminUsersApi(this.cancellationToken$).getForClient;
  }

  canActivate(next: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean | Observable<boolean> {

    // set the Tenant ID
    this.workspace.setAdmin();
    const admin = this.workspace.admin;

    // check settings
    const getSettingsFromStorage = () => {

      // Try to retrieve the settings from local storage
      const prefix = ADMIN_SETTINGS_PREFIX;
      const cachedSettings = JSON.parse(this.storage.getItem(storageKey(prefix))) as AdminSettingsForClient;
      const cachedSettingsVersion = this.storage.getItem(versionStorageKey(prefix));
      const cachedSettingsMetaVersion = this.storage.getItem(metaVersionStorageKey(prefix));
      if (!!cachedSettings && cachedSettingsMetaVersion === ADMIN_SETTINGS_METAVERSION) {
        admin.settings = cachedSettings;
        admin.settingsVersion = cachedSettingsVersion || '???';
      }
    };

    if (!admin.settings) {
      getSettingsFromStorage();
    }

    // check permissions
    const getPermissionsFromStorage = () => {

      // Try to retrieve the permissions from local storage
      const prefix = ADMIN_PERMISSIONS_PREFIX;
      const cachedPermissions = (JSON.parse(this.storage.getItem(storageKey(prefix))) || {}) as AdminPermissionsForClient;
      const cachedPermissionsVersion = this.storage.getItem(versionStorageKey(prefix));
      const cachedPermissionsMetaVersion = this.storage.getItem(metaVersionStorageKey(prefix));
      if (!!cachedPermissions && cachedPermissionsMetaVersion === ADMIN_PERMISSIONS_METAVERSION) {
        admin.permissions = cachedPermissions.Views;
        admin.permissionsVersion = cachedPermissionsVersion || '???';
      }
    };

    // check permissions
    if (!admin.permissions) {
      getPermissionsFromStorage();
    }

    // check user settings
    const getUserSettingsFromStorage = () => {

      // Try to retrieve the user settings from local storage
      const prefix = ADMIN_USER_SETTINGS_PREFIX;
      const cachedUserSettings = JSON.parse(this.storage.getItem(storageKey(prefix))) as AdminUserSettingsForClient;
      const cachedUserSettingsVersion = this.storage.getItem(versionStorageKey(prefix));
      const cachedUserSettingsMetaVersion = this.storage.getItem(metaVersionStorageKey(prefix));
      if (!!cachedUserSettings && cachedUserSettingsMetaVersion === ADMIN_USER_SETTINGS_METAVERSION) {

        admin.userSettings = cachedUserSettings;
        admin.userSettingsVersion = cachedUserSettingsVersion || '???';
      }
    };

    // check user settings
    if (!admin.userSettings) {
      getUserSettingsFromStorage();
    }

    // subscribe to changes in the storage
    // If the user signs out or signs in from another window
    // we automatically sign out/in in this window in order to
    // achieve a consistent experience

    this.storage.changed$.subscribe(e => {
      // settings
      const settingsKey = versionStorageKey(ADMIN_SETTINGS_PREFIX);
      if (e.key === settingsKey) {
        if (e.newValue !== admin.settingsVersion) {
          getSettingsFromStorage();
        }
      }

      // permissions
      const permissionsKey = versionStorageKey(ADMIN_PERMISSIONS_PREFIX);
      if (e.key === permissionsKey) {
        if (e.newValue !== admin.permissionsVersion) {
          getPermissionsFromStorage();
        }
      }

      // user settings
      const userSettingsKey = versionStorageKey(ADMIN_USER_SETTINGS_PREFIX);
      if (e.key === userSettingsKey) {
        if (e.newValue !== admin.userSettingsVersion) {
          getUserSettingsFromStorage();
        }
      }

    });

    // IF this is a new browser/machine, need to get the globals from the backend
    if (admin.settings && admin.permissions && admin.userSettings) {
      // In case our cached globals are stale this will trigger their refresh
      this.ping().pipe(retry(2)).subscribe();

      // we can log in to the tenant immediately based on the cached globals, don't wait till they are refreshed
      return true;
    } else {
      const key = `admin`;
      this.progress.startAsyncOperation(key, 'LoadingAdminConsoleSettings'); // To show the rotator

      // using forkJoin is recommended for running HTTP calls in parallel
      const obs$ = forkJoin([this.settingsApi(), this.permissionsApi(), this.userSettingsApi()]).pipe(
        tap(result => {
          this.progress.completeAsyncOperation(key);
          // cache the settings and set it in the workspace
          handleFreshSettings(result[0], admin, this.storage);
          handleFreshPermissions(result[1], admin, this.storage);
          handleFreshUserSettings(result[2], admin, this.storage);
        }),
        map(() => true),
        catchError((err: { status: number, error: any }) => {
          this.progress.completeAsyncOperation(key);

          if (err.status === 403) {
            this.router.navigate(['root', 'error', 'admin-unauthorized'], { queryParams: { retryUrl: state.url } });
          } else {
            this.workspace.ws.errorMessage = err.error;
            this.router.navigate(['root', 'error', 'loading-admin-settings'], { queryParams: { retryUrl: state.url } });
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
