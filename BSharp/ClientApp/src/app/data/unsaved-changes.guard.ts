import { Injectable } from '@angular/core';
import { CanDeactivate } from '@angular/router';
import { Observable } from 'rxjs';
import { ProgressOverlayService } from './progress-overlay.service';
import { ApiService } from './api.service';

@Injectable({
  providedIn: 'root'
})
export class UnsavedChangesGuard implements CanDeactivate<ICanDeactivate> {

  constructor(private api: ApiService, private progress: ProgressOverlayService) {
  }

  canDeactivate(component: ICanDeactivate) {
    return !this.api.showRotator && !this.progress.asyncOperationInProgress &&
      component.canDeactivate ? component.canDeactivate() : true;
  }

}

export interface ICanDeactivate {
  canDeactivate: () => Observable<boolean> | Promise<boolean> | boolean;
}
