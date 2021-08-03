import { Injectable } from '@angular/core';
import { WorkspaceService } from './workspace.service';
import { StorageService } from './storage.service';
import { ServerNotificationsService } from './server-notifications.service';

@Injectable({
  providedIn: 'root'
})
export class CleanerService {

  constructor(
    private workspace: WorkspaceService, private storage: StorageService,
    private notifications: ServerNotificationsService) { }

  public cleanState(): void {
    this.cleanWorkspace();
    this.cleanLocalStorage(true);
  }

  public cleanWorkspace(): void {
    this.workspace.reset();
  }

  public cleanLocalStorage(preserveCache?: boolean): void {
    if (!!preserveCache) {
      const preservedKeys = this.storage.keys.filter(k =>
        k === 'user_culture' || k.startsWith('translations_') || k.startsWith('global_settings')
      );

      const preserved: { [key: string ]: string } = {};
      preservedKeys.forEach(key => {
        preserved[key] = this.storage.getItem(key);
      });

      this.storage.clear();

      preservedKeys.forEach(key => {
        this.storage.setItem(key, preserved[key]);
      });
    } else {
      this.storage.clear();
    }
  }

  public cleanServerNotifications(): void {
    this.notifications.signout();
  }
}
