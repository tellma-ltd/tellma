import { Injectable } from '@angular/core';
import { WorkspaceService, TenantWorkspace, AdminWorkspace } from './workspace.service';
import { Subject, Observable, of } from 'rxjs';
import { switchMap, tap, catchError } from 'rxjs/operators';
import { ApiService } from './api.service';
import { handleFreshUserSettings as handleFreshAdminUserSettings } from './admin-resolver.guard';
import { StorageService } from './storage.service';
import { handleFreshUserSettings } from './tenant-resolver.guard';

interface SaveCustomUserSettingsArgs {
  key: string;
  value: string;
  workspace: TenantWorkspace | AdminWorkspace;
  tenantId: number;
  isAdmin: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class CustomUserSettingsService {

  private notifyDestruct$ = new Subject<void>();
  private notifySaveSettingsOnServer$ = new Subject<SaveCustomUserSettingsArgs>();

  constructor(private workspace: WorkspaceService, private api: ApiService, private storage: StorageService) {

    this.notifySaveSettingsOnServer$.pipe(
      switchMap((args: SaveCustomUserSettingsArgs) => this.doSaveSettingsOnServer(args))
    ).subscribe();
  }

  private get customSettings() {
    const settings = this.workspace.current.userSettings;
    settings.CustomSettings = settings.CustomSettings || {};
    return settings.CustomSettings;
  }

  public getString(key: string): string {
    return this.customSettings[key];
  }

  public get<T>(key: string): T {
    try {
      const resultString = this.customSettings[key];
      if (!!resultString) {
        const result = JSON.parse(resultString) as T;
        return result;
      }
    } catch { }

    return undefined;
  }

  public save(key: string, value: string) {
    const customSettings = this.customSettings;

    if (!!value) {
      customSettings[key] = value;
    } else if (customSettings[key]) {
      delete customSettings[key];
    }

    // switch map ensures that only the last save is persisted in the workspace
    const workspace = this.workspace.current;
    const tenantId = this.workspace.ws.tenantId;
    const isAdmin = this.workspace.isAdmin;
    this.notifySaveSettingsOnServer$.next({ key, value, workspace, isAdmin, tenantId });
  }

  private doSaveSettingsOnServer(args: SaveCustomUserSettingsArgs): Observable<any> {
    if (args.isAdmin) {
      return this.api.adminUsersApi(this.notifyDestruct$).saveForClient(args.key, args.value)
      .pipe(
        tap(result => {
          const workspace = args.workspace as AdminWorkspace;
          handleFreshAdminUserSettings(result, workspace, this.storage);
        }),
        catchError(friendlyError => {
          console.error(friendlyError.error);
          return of(null);
        })
      );
    } else {
      return this.api.usersApi(this.notifyDestruct$).saveForClient(args.key, args.value)
      .pipe(
        tap(result => {
          const workspace = args.workspace as TenantWorkspace;
          handleFreshUserSettings(result, args.tenantId, workspace, this.storage);
        }),
        catchError(friendlyError => {
          console.error(friendlyError.error);
          return of(null);
        })
      );
    }
  }
}
