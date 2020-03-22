import { Injectable } from '@angular/core';
import { CanDeactivate, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { ProgressOverlayService } from './progress-overlay.service';
import { ApiService } from './api.service';
import { WorkspaceService } from './workspace.service';

@Injectable({
  providedIn: 'root'
})
export class UnsavedChangesGuard implements CanDeactivate<ICanDeactivate> {

  constructor(private api: ApiService, private progress: ProgressOverlayService, private workspace: WorkspaceService) {
  }

  canDeactivate(
    component: ICanDeactivate, _: ActivatedRouteSnapshot,
    currentState: RouterStateSnapshot, nextState?: RouterStateSnapshot) {

    if (this.workspace.current.unauthorized) {
      return true;
    }

    return !this.api.showRotator && !this.progress.asyncOperationInProgress &&
      component.canDeactivate ? component.canDeactivate(currentState.url, nextState.url) : true;
  }
}

export interface ICanDeactivate {
  canDeactivate: (currentUrl?: string, nextUrl?: string) => Observable<boolean> | Promise<boolean> | boolean;
}
