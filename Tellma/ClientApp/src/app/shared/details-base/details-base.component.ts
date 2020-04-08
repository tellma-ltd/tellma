import { Component, ViewChild, Input, EventEmitter, OnDestroy, Output, OnInit } from '@angular/core';
import { DetailsComponent } from '~/app/shared/details/details.component';
import { Observable, Subscription, Subject } from 'rxjs';
import { ICanDeactivate } from '~/app/data/unsaved-changes.guard';
import { TenantWorkspace } from '~/app/data/workspace.service';

/**
 * a convenience class that canonical details components can inherit from
 * it provides all common functionality of the TypeScript part of the component
 * and complements a t-details in the root of its HTML template
 */
@Component({
  template: ''
})
export class DetailsBaseComponent implements ICanDeactivate, OnInit, OnDestroy {

  @Input()
  public mode: 'screen' | 'popup' = 'screen';

  @Input()
  public idString: string | number;

  @Input()
  public additionalSelect: string;

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
  public _subscriptions = new Subscription();

  @ViewChild(DetailsComponent, { static: false })
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
      if (!!v) {
        this.detailsSave = v.saved.subscribe(this.saved);
        this.detailsCancel = v.cancel.subscribe(this.cancel);
      }
    }
  }

  get details(): DetailsComponent {
    return this._details;
  }

  get isScreenMode(): boolean {
    return this.mode === 'screen';
  }

  get isPopupMode(): boolean {
    return this.mode === 'popup';
  }

  ngOnInit(): void {
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

    if (!!this._subscriptions) {
      this._subscriptions.unsubscribe();
    }
  }

  // triggers the user confirmation modal
  canDeactivate(currentUrl: string, nextUrl: string): boolean | Observable<boolean> {
    return !!this.details ? this.details.canDeactivate(currentUrl, nextUrl) : true;
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
