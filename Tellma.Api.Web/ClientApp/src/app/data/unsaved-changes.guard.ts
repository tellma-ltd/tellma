import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, RouterStateSnapshot, CanDeactivateFn } from '@angular/router';
import { Observable } from 'rxjs';
import { ProgressOverlayService } from './progress-overlay.service';
import { ApiService } from './api.service';
import { WorkspaceService } from './workspace.service';

// @Injectable({
//   providedIn: 'root'
// })
// export class UnsavedChangesGuard implements CanDeactivate<ICanDeactivate> {

//   constructor(private api: ApiService, private progress: ProgressOverlayService, private workspace: WorkspaceService) {
//   }

//   canDeactivate(
//     component: ICanDeactivate, x: ActivatedRouteSnapshot,
//     currentState: RouterStateSnapshot, nextState?: RouterStateSnapshot) {

//     if (this.workspace.current.unauthorized) {
//       return true;
//     }

//     // Using this guard already covers the functionality of SaveInProgressGuard
//     return !this.api.showRotator && !this.progress.asyncOperationInProgress &&
//       component.canDeactivate ? component.canDeactivate(currentState.url, nextState.url) : true;
//   }
// }

export interface ICanDeactivate {
  canDeactivate: (currentUrl?: string, nextUrl?: string) => Observable<boolean> | Promise<boolean> | boolean;
}

export const unsavedChangesGuard: CanDeactivateFn<ICanDeactivate> = (
  component: ICanDeactivate,
  _: ActivatedRouteSnapshot,
  currentState: RouterStateSnapshot,
  nextState?: RouterStateSnapshot): Observable<boolean> | Promise<boolean> | boolean => {

  const ws = inject(WorkspaceService);
  const api = inject(ApiService);
  const progress = inject(ProgressOverlayService);

  if (ws.current.unauthorized) {
    return true;
  }

  // Using this guard already covers the functionality of SaveInProgressGuard
  return !api.showRotator && !progress.asyncOperationInProgress &&
    component.canDeactivate ? component.canDeactivate(currentState.url, nextState.url) : true;
}
