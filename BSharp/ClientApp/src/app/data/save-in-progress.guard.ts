import { Injectable } from '@angular/core';
import { CanDeactivate } from '@angular/router';
import { ApiService } from './api.service';

@Injectable({
  providedIn: 'root'
})
export class SaveInProgressGuard implements CanDeactivate<any> {

  constructor(private api: ApiService) {
  }

  canDeactivate(): boolean {
    // This guard prevents any navigation while there is a save in progress
    return !this.api.saveInProgress;
  }
}
