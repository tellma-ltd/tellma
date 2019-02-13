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
    this.cleanLocalStorage();
  }

  cleanWorkspace(): any {
    this.workspace.reset();
  }

  cleanLocalStorage(): any {
    this.storage.clear();
  }
}
