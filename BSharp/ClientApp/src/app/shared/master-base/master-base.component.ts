import { Component, ViewChild, Input, EventEmitter, OnDestroy, Output } from '@angular/core';
import { Observable, Subscription, Subject } from 'rxjs';
import { MasterComponent } from '../master/master.component';

@Component({
  template: ''
})
export class MasterBaseComponent implements OnDestroy {

  // a convenience class that canonical master components can inherit from
  // it provides all common functionality of the TypeScript part of the component
  // and complements a b-master in the root of its HTML template

  @Input()
  public mode: 'screen' | 'popup' = 'screen';

  @Output()
  select = new EventEmitter<number | string>();

  @Output()
  create = new EventEmitter<void>();

  @Output()
  cancel = new EventEmitter<void>();

  private _master: MasterComponent;
  private masterSelect: Subscription;
  private masterCreate: Subscription;
  private masterCancel: Subscription;
  public notifyDestruct$ = new Subject<void>();

  @ViewChild(MasterComponent)
  set master(v: MasterComponent) {
    if (this._master !== v) {

      // unsubscribe from old details events
      if (!!this.masterSelect) {
        this.masterSelect.unsubscribe();
      }

      if (!!this.masterCreate) {
        this.masterCreate.unsubscribe();
      }

      if (!!this.masterCancel) {
        this.masterCancel.unsubscribe();
      }

      // set the new NgControl
      this._master = v;

      // subscribe to new details events
      if (!!this._master) {
        this.masterSelect = this._master.select.subscribe(this.select);
        this.masterCreate = this._master.create.subscribe(this.create);
        this.masterCancel = this._master.cancel.subscribe(this.cancel);
      }
    }
  }

  get master(): MasterComponent {
    return this._master;
  }

  ngOnDestroy(): void {
    // cleanup duty
    if (!!this.masterSelect) {
      this.masterSelect.unsubscribe();
    }

    if (!!this.masterCreate) {
      this.masterCreate.unsubscribe();
    }

    if (!!this.masterCancel) {
      this.masterCancel.unsubscribe();
    }

    this.notifyDestruct$.next();
  }

}
