// tslint:disable:member-ordering
import { Component, ViewChild } from '@angular/core';
import { Observable } from 'rxjs';
import { ICanDeactivate } from '~/app/data/unsaved-changes.guard';
import { SettingsComponent } from '../settings/settings.component';

/**
 * a convenience class that canonical settings components can inherit from
 * it provides all common functionality of the TypeScript part of the component
 * and complements a t-settings in the root of its HTML template
 */
@Component({
  template: ''
})
export class SettingsBaseComponent implements ICanDeactivate {

  @ViewChild(SettingsComponent, { static: false })
  settings: SettingsComponent;

  // triggers the user confirmation modal
  canDeactivate(): boolean | Observable<boolean> {
    return !!this.settings ? this.settings.canDeactivate() : true;
  }
}
