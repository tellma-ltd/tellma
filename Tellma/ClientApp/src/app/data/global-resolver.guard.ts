import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { Observable, of, throwError } from 'rxjs';
import { WorkspaceService } from './workspace.service';
import { StorageService } from './storage.service';
import { DataWithVersion } from './dto/data-with-version';
import { GlobalSettingsForClient } from './dto/global-settings';
import { retry, tap, map, catchError, finalize, concatMap } from 'rxjs/operators';
import { ProgressOverlayService } from './progress-overlay.service';
import { HttpClient } from '@angular/common/http';
import { environment } from '~/environments/environment';
import { friendlify } from './util';
import { TranslateService } from '@ngx-translate/core';

export const GLOBAL_SETTINGS_KEY = 'global_settings';
export const GLOBAL_SETTINGS_VERSION_KEY = 'global_settings_version';
export const GLOBAL_SETTINGS_METAVERSION_KEY = 'global_settings_metaversion';
export const GLOBAL_SETTINGS_METAVERSION = '1.0';

export function handleFreshGlobalSettings(
  result: DataWithVersion<GlobalSettingsForClient>,
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
   * Base address of the API server, e.g.: 'https://www.tellma.com/'
   * (with and without forward slash are fine)
   */
  apiAddress?: string;

  /**
   * Base address of the identity server: 'https://www.tellma.com'
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
    loginUrl: string;

    /**
     * Relative, e.g. '/connect/checksession'
     */
    sessionCheckIFrameUrl: string;

    /**
     * Relative, e.g. '/connect/endsession'
     */
    logoutUrl: string;

    /**
     * Periodicaly and silently refresh the access token every X seconds
     */
    tokenRefreshPeriodInSeconds: number;
  };
}

let appsettingsIsLoaded = false;
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
   * This observable must complete before ANY api calls,
   * it will retrive among other things the url of the server
   */
  private loadAppSettings(): Observable<boolean> {

    // The guard is called twice when the user navigates to the base URL
    if (!!appsettingsIsLoaded) {
      return of(true);
    }

    // This function handles the settings retrieved from the JSON file
    const readAppSettings = (jsonSettings: AppSettings) => {
      jsonSettings = jsonSettings || {};
      Object.assign(appsettings, jsonSettings);

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
          idConfig.loginUrl = appsettings.identityAddress + loginUrl;
        }

        const iframeUrl = idConfig.sessionCheckIFrameUrl;
        if (!!iframeUrl && !iframeUrl.startsWith(appsettings.identityAddress)) {
          idConfig.sessionCheckIFrameUrl = appsettings.identityAddress + iframeUrl;
        }

        const logoutUrl = idConfig.logoutUrl;
        if (!!logoutUrl && !logoutUrl.startsWith(appsettings.identityAddress)) {
          idConfig.logoutUrl = appsettings.identityAddress + logoutUrl;
        }

        // Refresh rate defaults to once per hour
        idConfig.tokenRefreshPeriodInSeconds = idConfig.tokenRefreshPeriodInSeconds || 60 * 60;
      }

      appsettingsIsLoaded = true; // No need to load twice
    };

    const appsettingsUri = environment.production ? '/assets/appsettings.json' : '/assets/appsettings.development.json';
    return this.http.get<AppSettings>(appsettingsUri)
      .pipe(
        // Add defaults and massage the results
        tap(readAppSettings),
        map(_ => true),
        catchError(error => {
          console.error(error);
          const friendlyError = friendlify(error, this.trx);
          return throwError(friendlyError);
        })
      );
  }

  private getGlobalSettingsForClient(apiAddress: string): Observable<DataWithVersion<GlobalSettingsForClient>> {

    const url = apiAddress + `api/global-settings/client`;
    const obs$ = this.http.get<DataWithVersion<GlobalSettingsForClient>>(url).pipe(
      catchError(error => {
        console.error(error);
        const friendlyError = friendlify(error, this.trx);
        return throwError(friendlyError);
      })
    );

    return obs$;
  }

  private ping(apiAddress: string): Observable<any> {
    const url = apiAddress + `api/global-settings/ping`;
    const obs$ = this.http.get(url);
    return obs$;
  }

  canActivate(
    _: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean> | Promise<boolean> | boolean {

    const ws = this.workspace;

    // check settings
    const getGlobalSettingsFromStorage = () => {
      try {
        // Try to retrieve the settings from local storage
        const key = GLOBAL_SETTINGS_KEY;
        const versionKey = GLOBAL_SETTINGS_VERSION_KEY;
        const cachedGlobalSettings = JSON.parse(this.storage.getItem(key)) as GlobalSettingsForClient;
        const cachedGlobalSettingsVersion = this.storage.getItem(versionKey);
        const cachedGlobalSettingsMetaVersion = this.storage.getItem(GLOBAL_SETTINGS_METAVERSION_KEY);
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

    // IF this is a new browser/machine, need to get the globals from the backend
    let result$: Observable<boolean>;
    if (!!ws.globalSettings) {
      // we can log in to the tenant immediately based on the cached globals, don't wait till they
      // are refreshed, (the app settings are cached by the service worker, so they load instantly)
      // once appsettings are loaded we asynchronously ping, in case our cached globals are stale
      // this will trigger their refresh
      result$ = this.loadAppSettings()
        .pipe(
          tap(__ => {
            this.ping(appsettings.apiAddress).pipe(retry(2)).subscribe();
          })
        );
    } else {
      // Show the rotator
      this.progress.startAsyncOperation(asyncKey, 'LoadingSystemSettings');

      // Otherwise load the static app settings first and then the global settings
      result$ = this.loadAppSettings().pipe(
        concatMap(__ => this.getGlobalSettingsForClient(appsettings.apiAddress)
          .pipe(
            tap(result => handleFreshGlobalSettings(result, ws, this.storage)),
            map(() => true)
          ))
      );
    }

    // Error handling and remove the rotator animation
    return result$.pipe(
      tap(() => this.progress.completeAsyncOperation(asyncKey)),
      catchError((err: { status: number, error: any }) => {
        this.progress.completeAsyncOperation(asyncKey);
        this.workspace.ws.errorMessage = err.error;
        this.router.navigate(['error', 'loading-global-settings'], { queryParams: { retryUrl: state.url } });

        // Prevent navigation
        return of(false);
      }),
      finalize(() => this.progress.completeAsyncOperation(asyncKey))
    );
  }
}
