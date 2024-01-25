import { inject } from '@angular/core';
import { CanDeactivateFn } from '@angular/router';
import { ApiService } from './api.service';
import { ProgressOverlayService } from './progress-overlay.service';

// @Injectable({
//   providedIn: 'root'
// })
// export class SaveInProgressGuard2 implements CanDeactivate<any> {

//   constructor(private api: ApiService, private progress: ProgressOverlayService) {
//   }

//   canDeactivate(): boolean {
//     // This guard prevents any navigation while there is a save in progress
//     return !this.api.showRotator && !this.progress.asyncOperationInProgress;
//   }
// }

export const saveInProgressGuard: CanDeactivateFn<any> = (): boolean => {

  const api = inject(ApiService);
  const progress = inject(ProgressOverlayService);

  // This guard prevents any navigation while there is a save in progress
  return !api.showRotator && !progress.asyncOperationInProgress;
}