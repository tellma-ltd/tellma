import { HttpEvent, HttpHandler, HttpInterceptor, HttpRequest, HttpResponse, HttpErrorResponse, HttpClient } from '@angular/common/http';
import { Observable, Subject, of, throwError } from 'rxjs';
import { WorkspaceService } from './workspace.service';
import { tap, exhaustMap, retry, catchError } from 'rxjs/operators';
import { ApiService } from './api.service';
import { DataWithVersion } from './dto/data-with-version';
import { SettingsForClient } from './dto/settings';
import { PermissionsForClient } from './dto/permission';
import { StorageService } from './storage.service';
import {
  handleFreshPermissions, versionStorageKey,
  storageKey, SETTINGS_PREFIX, PERMISSIONS_PREFIX, USER_SETTINGS_PREFIX, handleFreshUserSettings, handleFreshSettings
} from './tenant-resolver.guard';
import { Router } from '@angular/router';
import { UserSettingsForClient } from './dto/local-user';
import { OAuthStorage } from 'angular-oauth2-oidc';
import { CleanerService } from './cleaner.service';
import { apiTranslateLoaderFactory } from './api-translate-loader';
import { TranslateService } from '@ngx-translate/core';

type VersionStatus = 'Fresh' | 'Stale' | 'Unauthorized';

// Translations stuff

export function translationStorageKey(cultureName: string) { return `translations_${cultureName}`; }
export function translationVersionStorageKey(cultureName: string) { return `translations_${cultureName}_version`; }

export function saveTranslationsInStorage(cultureName: string, version: string, translations: any, storage: StorageService) {
  const key = translationStorageKey(cultureName);
  const versionKey = translationVersionStorageKey(cultureName);
  storage.setItem(key, JSON.stringify(translations));
  storage.setItem(versionKey, version);
}

export class RootHttpInterceptor implements HttpInterceptor {

  private notifyRefreshTranslations$: Subject<string>;
  private notifyRefreshSettings$: Subject<void>;
  private notifyRefreshPermissions$: Subject<void>;
  private notifyRefreshUserSettings$: Subject<void>;
  private cancellationToken$: Subject<void>;
  private translationsApi: (cultureName: string) => Observable<DataWithVersion<any>>;
  private settingsApi: () => Observable<DataWithVersion<SettingsForClient>>;
  private permissionsApi: () => Observable<DataWithVersion<PermissionsForClient>>;
  private userSettingsApi: () => Observable<DataWithVersion<UserSettingsForClient>>;

  constructor(private workspace: WorkspaceService, private api: ApiService,
    private storage: StorageService, private router: Router,
    private authStorage: OAuthStorage, private cleaner: CleanerService,
    private translate: TranslateService) {
    // Note: We use exhaustMap to prevent making another call while a call is in progress
    // https://www.learnrxjs.io/operators/transformation/exhaustmap.html

    this.notifyRefreshTranslations$ = new Subject<string>();
    this.notifyRefreshTranslations$.pipe(
      exhaustMap(cultureName => this.doRefreshTranslations(cultureName))
    ).subscribe();

    this.notifyRefreshSettings$ = new Subject<void>();
    this.notifyRefreshSettings$.pipe(
      exhaustMap(() => this.doRefreshSettings())
    ).subscribe();

    this.notifyRefreshPermissions$ = new Subject<void>();
    this.notifyRefreshPermissions$.pipe(
      exhaustMap(() => this.doRefreshPermissions())
    ).subscribe();

    this.notifyRefreshUserSettings$ = new Subject<void>();
    this.notifyRefreshUserSettings$.pipe(
      exhaustMap(() => this.doRefreshUserSettings())
    ).subscribe();

    this.cancellationToken$ = new Subject<void>();

    this.translationsApi = this.api.tranlationsApi(this.cancellationToken$).getForClient;
    this.settingsApi = this.api.settingsApi(this.cancellationToken$).getForClient;
    this.permissionsApi = this.api.permissionsApi(this.cancellationToken$).getForClient;
    this.userSettingsApi = this.api.localUsersApi(this.cancellationToken$).getForClient;


    // Monitor local storage for changes to translations
    storage.changed$.subscribe(e => {
      if (!!e.key && e.key.startsWith('translations_') && !e.key.endsWith('_version')) {
        const cultureName = e.key.split('_')[1];
        const translations = JSON.parse(e.newValue);
        translate.setTranslation(cultureName, translations, false);
      }

      // TODO the other versions
    });
  }

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {

    // we accumulate all the headers params in these objects
    const headers = {};
    const params = {};

    // tenant ID
    const tenantId = this.workspace.ws.tenantId;
    if (!!tenantId) {
      // This piece of logic does not really belong to the root module and is
      // specific to the application module, but moving it there is not worth
      // the hassle now
      headers['X-Tenant-Id'] = tenantId.toString();

      // Even though API response caching is disabled with server headers, this is a last defense
      // to absolutely guarantee that caching will never cause one tenant's data to show up while
      // you're logged into another tenant, but the server only relies on the header X-Tenant-Id
      params['tenantId'] = tenantId.toString();
    }

    if (!!this.authStorage) {
      const accessToken = this.authStorage.getItem('access_token');
      if (!!accessToken) {
        headers['Authorization'] = `Bearer ${accessToken}`;
      }
    }

    // UI culture
    const culture = this.workspace.ws.culture;
    if (!!culture) {
      params['ui-culture'] = culture;
    }

    // the version refresh APIs should not include the version headers
    const isVersionRefreshRequest = req.url.endsWith('/client') || req.url.indexOf('/client/') !== -1;

    // tenant versions
    const current = this.workspace.current;
    if (!!current && !isVersionRefreshRequest) {
      headers['X-Settings-Version'] = current.settingsVersion || '???';
      headers['X-Permissions-Version'] = current.permissionsVersion || '???';
      headers['X-User-Settings-Version'] = current.userSettingsVersion || '???';
    }

    // global versions
    const translationsKey = translationVersionStorageKey(culture);
    const translationsVersion = this.storage.getItem(translationsKey);
    if (!!translationsVersion) {
      headers['X-Translations-Version'] = translationsVersion || '???';
    }

    // clone the request and set the headers and parameters
    req = req.clone({
      setHeaders: headers,
      setParams: params
    });

    // TODO add authorization header
    // TODO intercept 401 responses and log the user out

    return next.handle(req).pipe(
      tap(e => this.handleServerVersions(e, tenantId, culture)),
      catchError(e => {
        this.handleServerVersions(e, tenantId, culture);
        // If it's a 401 then quickly delete the app state and challenge user
        if (e instanceof HttpErrorResponse && e.status === 401) {
          this.router.navigateByUrl('/welcome?error=401');
          this.cleaner.cleanState();
        }
        return throwError(e);
      })
    );
  }

  handleServerVersions = (e: any, tenantId: number, culture: string) => {

    if (!!e && !!e.headers) {

      // global versions
      if (!!culture) {

        // translations
        const v = <VersionStatus>e.headers.get('x-translations-version');
        if (v === 'Stale') {
          this.refreshTranslations(culture);
        }
      }

      // tenant versions
      if (!!tenantId) {
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
            this.storage.removeItem(storageKey(USER_SETTINGS_PREFIX, tenantId));
            this.storage.removeItem(versionStorageKey(USER_SETTINGS_PREFIX, tenantId));

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

        // user settings
        {
          const v = <VersionStatus>e.headers.get('x-user-settings-version');
          if (v === 'Stale') {
            this.refreshUserSettings();
          }
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

  refreshUserSettings() {
    this.notifyRefreshUserSettings$.next();
  }

  doRefreshUserSettings = () => {

    const current = this.workspace.current;
    const tenantId = this.workspace.ws.tenantId;

    // using forkJoin is recommended for running HTTP calls in parallel
    const obs$ = this.userSettingsApi().pipe(
      tap(result => {
        // Cache the user settings and set them in the workspace
        handleFreshUserSettings(result, tenantId, current, this.storage);
      }),
      retry(2)
    );

    return obs$;
  }

  refreshTranslations(cultureName: string) {
    this.notifyRefreshTranslations$.next(cultureName);
  }

  private doRefreshTranslations = (cultureName: string) => {

    // using forkJoin is recommended for running HTTP calls in parallel
    const obs$ = this.translationsApi(cultureName).pipe(
      tap(dwv => {
        // cache the translations in local storage
        saveTranslationsInStorage(cultureName, dwv.Version, dwv.Data, this.storage);

        // notify the translate service in order to update the UI
        this.translate.setTranslation(cultureName, dwv.Data);
      }),
      retry(2)
    );

    return obs$;
  }

}
