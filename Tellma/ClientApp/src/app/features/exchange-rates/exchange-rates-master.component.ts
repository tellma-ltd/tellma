import { Component } from '@angular/core';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { WorkspaceService } from '~/app/data/workspace.service';

@Component({
  selector: 't-exchange-rates-master',
  templateUrl: './exchange-rates-master.component.html',
  styles: []
})
export class ExchangeRatesMasterComponent extends MasterBaseComponent {

  public expand = '';

  constructor(
    private workspace: WorkspaceService) {
    super();
  }

  // UI Binding

  public get c() {
    return this.ws.ExchangeRate;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }
}
