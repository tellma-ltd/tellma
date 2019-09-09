import { Component, ViewChild, Input, EventEmitter, OnDestroy, Output, OnInit } from '@angular/core';
import { Subscription, Subject } from 'rxjs';
import { MasterComponent } from '../master/master.component';

@Component({
  template: ''
})
export class MasterBaseComponent implements OnInit, OnDestroy {

  // a convenience class that canonical master components can inherit from
  // it provides all common functionality of the TypeScript part of the component
  // and complements a b-master in the root of its HTML template

  @Input()
  public mode: 'screen' | 'popup' = 'screen';

  @Output()
  choose = new EventEmitter<number | string>();

  @Output()
  create = new EventEmitter<void>();

  @Output()
  cancel = new EventEmitter<void>();

  private masterSelect: Subscription;
  private masterCreate: Subscription;
  private masterCancel: Subscription;
  public notifyDestruct$ = new Subject<void>();

  @ViewChild(MasterComponent, { static : true })
  master: MasterComponent;

  ngOnInit(): void {
    this.masterSelect = this.master.choose.subscribe(this.choose);
    this.masterCreate = this.master.create.subscribe(this.create);
    this.masterCancel = this.master.cancel.subscribe(this.cancel);
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
