import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { Observable, of, throwError } from 'rxjs';
import { WorkspaceService } from './workspace.service';
import { StorageService } from './storage.service';
import { Versioned } from './dto/versioned';
import { GlobalSettingsForClient } from './dto/global-settings';
import { retry, tap, map, catchError, finalize, concatMap } from 'rxjs/operators';
import { ProgressOverlayService } from './progress-overlay.service';
import { HttpClient } from '@angular/common/http';
import { environment, appsettings as envsettings } from '~/environments/environment';
import { friendlify } from './util';
import { TranslateService } from '@ngx-translate/core';

export const GLOBAL_SETTINGS_KEY = 'global_settings';
export const GLOBAL_SETTINGS_VERSION_KEY = 'global_settings_version';
export const GLOBAL_SETTINGS_METAVERSION_KEY = 'global_settings_metaversion';
export const GLOBAL_SETTINGS_METAVERSION = '1.1';

export function handleFreshGlobalSettings(
  result: Versioned<GlobalSettingsForClient>,
  workspace: WorkspaceService, storage: StorageService) {

  const globalSettings = result.Data;
  const version = result.Version;
  const key = GLOBAL_SETTINGS_KEY;
  storage.setItem(key, JSON.stringify(globalSettings));
  storage.setItem(GLOBAL_SETTINGS_VERSION_KEY, version);
  storage.setItem(GLOBAL_SETTINGS_METAVERSION_KEY, GLOBAL_SETTINGS_METAVERSION);

  workspace.globalSettings = globalSettings;
  workspace.globalSettingsVersion = version;
  workspace.notifyStateChanged();
}

export interface AppSettings {

  /**
   * Base address of the API server, e.g.: 'https://web.tellma.com/'
   * (with and without forward slash are fine)
   */
  apiAddress?: string;

  /**
   * Base address of the identity server: 'https://web.tellma.com'
   * (with and without forward slash are fine)
   */
  identityAddress?: string;

  /**
   * Authentication settings
   */
  identityConfig?: {

    /**
     * Encryption information, optionally captured here for startup performance
     */
    jwks: {
      keys: {
        kty: string;
        use: string;
        kid: string;
        x5t?: string;
        x5c?: string[];
        e: string;
        alg: string;
        n: string;
      }[]
    };

    /**
     * Relative, e.g. '/connect/authorize'
     */
    loginUrl?: string;

    /**
     * Relative, e.g. '/connect/checksession'
     */
    sessionCheckIFrameUrl?: string;

    /**
     * Relative, e.g. '/connect/endsession'
     */
    logoutUrl?: string;

    /**
     * Periodicaly and silently refresh the access token every X seconds
     */
    tokenRefreshPeriodInSeconds?: number;
  };
}

let appsettingsIsSet = false;
export let appsettings: AppSettings = {};

@Injectable({
  providedIn: 'root'
})
export class GlobalResolverGuard implements CanActivate {

  constructor(
    private workspace: WorkspaceService, private storage: StorageService, public http: HttpClient,
    private router: Router, private progress: ProgressOverlayService, private trx: TranslateService) {
  }

  /**
   * This must be called at least once at the very beginning before ANY
   * api calls, the appsettings include among other things the url of the server
   */
  private setAppSettings(): void {

    // The guard is called twice when the user navigates to the base URL
    if (!!appsettingsIsSet) {
      return;
    }

    // (1) Read the settings from the environment.ts file (differnet in development and production)
    let sourceSettings: AppSettings = envsettings;

    // (2) In case the developer wanted to override the settings in their local dev machine without
    // modifying the source files, they can set different settings as JSON in their browser's localStorage
    if (!environment.production) {
      const jsonFromStorage = this.storage.getItem('appsettings');
      if (!!jsonFromStorage) { // They are set try to parse them
        try {
          sourceSettings = JSON.parse(jsonFromStorage);
        } catch (ex) {
          console.error('Error parsing appsettings from localStorage.', ex);
        }
      }
    }

    // (3) Extract the settings from the source
    sourceSettings = sourceSettings || {};
    Object.assign(appsettings, sourceSettings);

    // Base addresses default to same origin
    const sameOrigin = window.location.origin.replace('http://', 'https://') + '/';
    appsettings.apiAddress = appsettings.apiAddress || sameOrigin;
    appsettings.identityAddress = appsettings.identityAddress || appsettings.apiAddress;

    // The api address should end with a forward slash
    if (!appsettings.apiAddress.endsWith('/')) {
      appsettings.apiAddress += '/';
    }

    // The identity address should not end with forward slash
    while (appsettings.identityAddress.length > 0 && appsettings.identityAddress.endsWith('/')) {
      appsettings.identityAddress = appsettings.identityAddress.slice(0, -1);
    }

    // If not the case, append the identity server url to the 3 urls below
    const idConfig = appsettings.identityConfig;
    if (!!idConfig) {
      const loginUrl = idConfig.loginUrl;
      if (!!loginUrl && !loginUrl.startsWith(appsettings.identityAddress)) {
        const slash = loginUrl.startsWith('/') ? '' : '/';
        idConfig.loginUrl = appsettings.identityAddress + slash + loginUrl;
      }

      const iframeUrl = idConfig.sessionCheckIFrameUrl;
      if (!!iframeUrl && !iframeUrl.startsWith(appsettings.identityAddress)) {
        const slash = iframeUrl.startsWith('/') ? '' : '/';
        idConfig.sessionCheckIFrameUrl = appsettings.identityAddress + slash + iframeUrl;
      }

      const logoutUrl = idConfig.logoutUrl;
      if (!!logoutUrl && !logoutUrl.startsWith(appsettings.identityAddress)) {
        const slash = logoutUrl.startsWith('/') ? '' : '/';
        idConfig.logoutUrl = appsettings.identityAddress + slash + logoutUrl;
      }

      // Refresh rate defaults to once per hour
      idConfig.tokenRefreshPeriodInSeconds = idConfig.tokenRefreshPeriodInSeconds || 60 * 60;
    }

    // (4) Make sure settings are set once
    appsettingsIsSet = true; // No need to load twice
  }

  /**
   * Get the global settings from the server
   */
  private getGlobalSettingsForClient(apiAddress: string): Observable<Versioned<GlobalSettingsForClient>> {

    const url = apiAddress + `api/global-settings/client`;
    const obs$ = this.http.get<Versioned<GlobalSettingsForClient>>(url).pipe(
      catchError(error => {
        console.error(error);
        const friendlyError = friendlify(error, this.trx);
        return throwError(friendlyError);
      })
    );

    return obs$;
  }

  /**
   * We call this asynchrously at the beginning just to check the global cache version
   */
  private ping(apiAddress: string): Observable<any> {
    const url = apiAddress + `api/global-settings/ping`;
    const obs$ = this.http.get(url);
    return obs$;
  }

  canActivate(
    _: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean> | Promise<boolean> | boolean {

    // Very first thing, set the appsettings variable
    this.setAppSettings();

    const ws = this.workspace;

    // check settings
    const getGlobalSettingsFromStorage = () => {
      try {
        // Try to retrieve the settings from local storage
        const key = GLOBAL_SETTINGS_KEY;
        const versionKey = GLOBAL_SETTINGS_VERSION_KEY;
        const metaversionKey = GLOBAL_SETTINGS_METAVERSION_KEY;
        const cachedGlobalSettings = JSON.parse(this.storage.getItem(key)) as GlobalSettingsForClient;
        const cachedGlobalSettingsVersion = this.storage.getItem(versionKey);
        const cachedGlobalSettingsMetaVersion = this.storage.getItem(metaversionKey);
        if (!!cachedGlobalSettings && cachedGlobalSettingsMetaVersion === GLOBAL_SETTINGS_METAVERSION) {
          ws.globalSettings = cachedGlobalSettings;
          ws.globalSettingsVersion = cachedGlobalSettingsVersion || '???';
          ws.notifyStateChanged();
        }
      } catch { }
    };

    if (!ws.globalSettings) {
      getGlobalSettingsFromStorage();
    }

    this.storage.changed$.subscribe(e => {
      // settings
      const globalSettingsKey = GLOBAL_SETTINGS_KEY;
      if (e.key === globalSettingsKey) {
        if (e.newValue !== ws.globalSettingsVersion) {
          getGlobalSettingsFromStorage();
        }
      }
    });

    const asyncKey = 'global_settings';

    let result$: Observable<boolean>;
    if (!!ws.globalSettings) {
      // We can launch the app immediately based on the cached global settings and THEN ping asynchronously
      // to check the cache version.
      this.ping(appsettings.apiAddress).pipe(retry(2)).subscribe();
      result$ = of(true);
    } else {
      // This is a new browser/machine, get the globals from the backend
      this.progress.startAsyncOperation(asyncKey, 'LoadingSystemSettings'); // Rotator
      result$ = this.getGlobalSettingsForClient(appsettings.apiAddress)
        .pipe(
          tap(result => {
            this.progress.completeAsyncOperation(asyncKey);
            handleFreshGlobalSettings(result, ws, this.storage);
          }),
          catchError((err: { status: number, error: any }) => {
            this.progress.completeAsyncOperation(asyncKey);
            this.workspace.ws.errorMessage = err.error;
            this.router.navigate(['error', 'loading-global-settings'], { queryParams: { retryUrl: state.url } });

            // Prevent navigation
            return of(false);
          }),
          finalize(() => this.progress.completeAsyncOperation(asyncKey)),
          map(() => true)
        );
    }

    // Error handling and remove the rotator animation
    return result$;
  }
}
