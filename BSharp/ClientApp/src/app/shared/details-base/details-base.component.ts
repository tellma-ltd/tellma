import { Component, OnInit, ViewChild, Input, EventEmitter, OnDestroy, Output } from '@angular/core';
import { DetailsComponent } from '~/app/shared/details/details.component';
import { Observable, Subscription } from 'rxjs';
import { ICanDeactivate } from '~/app/data/unsaved-changes.guard';

@Component({
  template: ''
})
export class DetailsBaseComponent implements ICanDeactivate, OnDestroy {

  // a convenience class that canonical details components can inherit from
  // it provides all common functionality of the TypeScript part of the component
  // and complements a b-details in the root of its HTML template

  @Input()
  public mode: 'screen' | 'popup' = 'screen';

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

  @ViewChild(DetailsComponent)
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
  }

  // triggers the user confirmation modal
  canDeactivate(): boolean | Observable<boolean> {
    return !!this.details ? this.details.canDeactivate() : true;
  }

}
