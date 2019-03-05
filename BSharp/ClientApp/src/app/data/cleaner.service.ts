import { Injectable } from '@angular/core';
import { WorkspaceService } from './workspace.service';
import { StorageService } from './storage.service';

@Injectable({
  providedIn: 'root'
})
export class CleanerService {

  constructor(private workspace: WorkspaceService, private storage: StorageService) { }

  cleanState(): any {
    this.cleanWorkspace();
    this.cleanLocalStorage(true);
  }

  cleanWorkspace(): any {
    this.workspace.reset();
  }

  cleanLocalStorage(preserveCache?: boolean): any {

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
}
