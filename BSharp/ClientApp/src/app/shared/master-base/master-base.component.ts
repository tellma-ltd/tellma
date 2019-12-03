import { Component, ViewChild, Input, EventEmitter, OnDestroy, Output, OnInit } from '@angular/core';
import { Subscription, Subject } from 'rxjs';
import { MasterComponent } from '../master/master.component';

@Component({
  template: ''
})
export class MasterBaseComponent implements OnDestroy {

  // a convenience class that canonical master components can inherit from
  // it provides all common functionality of the TypeScript part of the component
  // and complements a b-master in the root of its HTML template

  @Input()
  showCreate = true; // For details pickers

  @Input()
  public mode: 'screen' | 'popup' = 'screen';

  @Output()
  choose = new EventEmitter<number | string>();

  @Output()
  create = new EventEmitter<void>();

  @Output()
  cancel = new EventEmitter<void>();

  private _master: MasterComponent;
  private masterSelect: Subscription;
  private masterCreate: Subscription;
  private masterCancel: Subscription;
  public notifyDestruct$ = new Subject<void>();

  @ViewChild(MasterComponent, { static: true })
  set master(m: MasterComponent) {
    if (this._master !== m) {

      this.cleanupSubscriptions();

      this._master = m;

      if (!!m) {
        this.masterSelect = m.choose.subscribe(this.choose);
        this.masterCreate = m.create.subscribe(this.create);
        this.masterCancel = m.cancel.subscribe(this.cancel);
      }
    }
  }

  get master(): MasterComponent {
    return this._master;
  }

  get isScreenMode(): boolean {
    return this.mode === 'screen';
  }

  get isPopupMode(): boolean {
    return this.mode === 'popup';
  }

  private cleanupSubscriptions() {

    if (!!this.masterSelect) {
      this.masterSelect.unsubscribe();
    }

    if (!!this.masterCreate) {
      this.masterCreate.unsubscribe();
    }

    if (!!this.masterCancel) {
      this.masterCancel.unsubscribe();
    }
  }

  ngOnDestroy(): void {
    // cleanup duty
    this.cleanupSubscriptions();

    this.notifyDestruct$.next();
  }
}
