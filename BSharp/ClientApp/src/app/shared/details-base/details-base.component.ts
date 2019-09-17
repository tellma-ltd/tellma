import { Component, ViewChild, Input, EventEmitter, OnDestroy, Output } from '@angular/core';
import { DetailsComponent } from '~/app/shared/details/details.component';
import { Observable, Subscription, Subject } from 'rxjs';
import { ICanDeactivate } from '~/app/data/unsaved-changes.guard';
import { TenantWorkspace } from '~/app/data/workspace.service';

@Component({
  template: ''
})
export class DetailsBaseComponent implements ICanDeactivate, OnDestroy {

  // a convenience class that canonical details components can inherit from
  // it provides all common functionality of the TypeScript part of the component
  // and complements a b-details in the root of its HTML template
  @Input()
  public mode: 'screen' | 'popup' = 'screen';

  // public mode: 'screen' | 'popup' = 'screen';

  @Input()
  public idString: string | number;

  @Input()
  public initialText: string;

  @Output()
  saved = new EventEmitter<number | string>();

  @Output()
  cancel = new EventEmitter<void>();

  private _details: DetailsComponent;
  private detailsSave: Subscription;
  private detailsCancel: Subscription;
  public notifyDestruct$ = new Subject<void>();

  @ViewChild(DetailsComponent, { static: true })
  set details(v: DetailsComponent) {
    if (this._details !== v) {

      // unsubscribe from old details events
      if (!!this.detailsSave) {
        this.detailsSave.unsubscribe();
      }

      if (!!this.detailsCancel) {
        this.detailsCancel.unsubscribe();
      }

      // set the new NgControl
      this._details = v;

      // subscribe to new details events
      if (!!this._details) {
        this.detailsSave = this._details.saved.subscribe(this.saved);
        this.detailsCancel = this._details.cancel.subscribe(this.cancel);
      }
    }
  }

  get details(): DetailsComponent {
    return this._details;
  }

  ngOnDestroy(): void {
    // cleanup duty
    if (!!this.detailsSave) {
      this.detailsSave.unsubscribe();
    }

    if (!!this.detailsCancel) {
      this.detailsCancel.unsubscribe();
    }

    this.notifyDestruct$.next();
  }

  // triggers the user confirmation modal
  canDeactivate(): boolean | Observable<boolean> {
    return !!this.details ? this.details.canDeactivate() : true;
  }

  getMultilingualValue(item: any, propName: string, ws: TenantWorkspace) {
    if (!!propName) {
      const propName2 = propName + '2';
      const propName3 = propName + '3';

      if (!!item) {
        if (ws.isSecondaryLanguage && !!item[propName2]) {
          return item[propName2];
        } else if (ws.isTernaryLanguage && !!item[propName3]) {
          return item[propName3];
        } else {
          return item[propName];
        }
      }
    }

    return null;
  }
}
