import { Component } from '@angular/core';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { WorkspaceService } from '~/app/data/workspace.service';
import { toLocalDateOnlyISOString } from '~/app/data/date-util';

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

  public exchangeRateDisplay(id: number): string {
    const er = this.ws.get('ExchangeRate', id);
    if (!er) {
      return '';
    } else {
      return `${toLocalDateOnlyISOString(new Date(er.ValidAsOf))}-${er.CurrencyId}`;
    }
  }
}
