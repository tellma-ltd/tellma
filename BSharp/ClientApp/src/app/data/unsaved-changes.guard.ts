import { Injectable } from '@angular/core';
import { CanDeactivate } from '@angular/router';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UnsavedChangesGuard implements CanDeactivate<ICanDeactivate> {

  canDeactivate(component: ICanDeactivate) {
    return component.canDeactivate ? component.canDeactivate() : true;
  }

}

export interface ICanDeactivate {
  canDeactivate: () => Observable<boolean> | Promise<boolean> | boolean;
}
