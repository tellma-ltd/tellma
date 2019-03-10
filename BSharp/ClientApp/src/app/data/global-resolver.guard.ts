import { Injectable } from '@angular/core';
import { CanActivate, ActivatedRouteSnapshot, RouterStateSnapshot, Router } from '@angular/router';
import { Observable, Subject, of } from 'rxjs';
import { WorkspaceService } from './workspace.service';
import { StorageService } from './storage.service';
import { DataWithVersion } from './dto/data-with-version';
import { GlobalSettingsForClient } from './dto/global-settings';
import { ApiService } from './api.service';
import { retry, tap, map, catchError, finalize } from 'rxjs/operators';
import { ProgressOverlayService } from './progress-overlay.service';


export const GLOBAL_SETTINGS_KEY = 'global_settings';
export const GLOBAL_SETTINGS_VERSION_KEY = 'global_settings_version';

export function handleFreshGlobalSettings(result: DataWithVersion<GlobalSettingsForClient>,
  tws: WorkspaceService, storage: StorageService) {

  const globalSettings = result.Data;
  const version = result.Version;
  const key = GLOBAL_SETTINGS_KEY;
  storage.setItem(key, JSON.stringify(globalSettings));
  storage.setItem(GLOBAL_SETTINGS_VERSION_KEY, version);

  tws.globalSettings = globalSettings;
  tws.globalSettingsVersion = version;
}

@Injectable({
  providedIn: 'root'
})
export class GlobalResolverGuard implements CanActivate {


  private cancellationToken$: Subject<void>;
  private globalSettingsApi: () => Observable<DataWithVersion<GlobalSettingsForClient>>;
  private ping: () => Observable<any>;

  constructor(private workspace: WorkspaceService, private storage: StorageService,
    private router: Router, private api: ApiService, private progress: ProgressOverlayService) {

    this.cancellationToken$ = new Subject<void>();
    const globalSettingsApi = this.api.globalSettingsApi(this.cancellationToken$);
    this.globalSettingsApi = globalSettingsApi.getForClient;
    this.ping = globalSettingsApi.ping;
  }

  canActivate(
    _: ActivatedRouteSnapshot,
    state: RouterStateSnapshot): Observable<boolean> | Promise<boolean> | boolean {

    const wss = this.workspace;

    // check settings
    const getGlobalSettingsFromStorage = () => {
      try {
        // Try to retrieve the settings from local storage
        const key = GLOBAL_SETTINGS_KEY;
        const versionKey = GLOBAL_SETTINGS_VERSION_KEY;
        const cachedGlobalSettings = <GlobalSettingsForClient>JSON.parse(this.storage.getItem(key));
        const cachedGlobalSettingsVersion = this.storage.getItem(versionKey);
        if (!!cachedGlobalSettings) {
          wss.globalSettings = cachedGlobalSettings;
          wss.globalSettingsVersion = cachedGlobalSettingsVersion || '???';
        }
      } catch {}
    };

    if (!wss.globalSettings) {
      getGlobalSettingsFromStorage();
    }

    this.storage.changed$.subscribe(e => {
      // settings
      const globalSettingsKey = GLOBAL_SETTINGS_KEY;
      if (e.key === globalSettingsKey) {
        if (e.newValue !== wss.globalSettingsVersion) {
          getGlobalSettingsFromStorage();
        }
      }
    });


    // IF this is a new browser/machine, need to get the globals from the backend
    if (!!wss.globalSettings) {
      // In case our cached globals are stale this will trigger their refresh
      this.ping().pipe(retry(2)).subscribe();

      // we can log in to the tenant immediately based on the cached globals, don't wait till they are refreshed
      return true;
    } else {
      const key = 'global_settings';
      this.progress.startAsyncOperation(key, 'LoadingSystemSettings');

      // using forkJoin is recommended for running HTTP calls in parallel
      const obs$ = this.globalSettingsApi().pipe(
        tap(() => this.progress.completeAsyncOperation(key)),
        tap(result => handleFreshGlobalSettings(result, wss, this.storage)),
        map(() => true),
        catchError((err: { status: number, error: any }) => {
          this.progress.completeAsyncOperation(key);
          this.workspace.ws.errorMessage = err.error;
          this.router.navigate(['error', 'loading-global-settings'], { queryParams: { retryUrl: state.url } });

          // Prevent navigation
          return of(false);
        }),
        finalize(() => this.progress.completeAsyncOperation(key))
      );

      return obs$;
    }
  }
}
