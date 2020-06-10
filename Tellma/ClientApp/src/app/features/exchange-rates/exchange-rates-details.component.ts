import { Component } from '@angular/core';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { WorkspaceService } from '~/app/data/workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { ExchangeRateForSave, metadata_ExchangeRate, ExchangeRate } from '~/app/data/entities/exchange-rate';
import { toLocalDateISOString } from '~/app/data/util';
import { Currency } from '~/app/data/entities/currency';

@Component({
  selector: 't-exchange-rates-details',
  templateUrl: './exchange-rates-details.component.html',
  styles: []
})
export class ExchangeRatesDetailsComponent extends DetailsBaseComponent {

  public expand = 'Currency';

  constructor(
    private workspace: WorkspaceService, private translate: TranslateService) {
    super();
  }

  create = () => {
    const result: ExchangeRateForSave = {
      ValidAsOf: toLocalDateISOString(new Date()),
      AmountInCurrency: 1
     };

    return result;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  public get masterCrumb(): string {
    const entityDesc = metadata_ExchangeRate(this.workspace, this.translate);
    return !!entityDesc ? entityDesc.titlePlural() : '???';
  }

  // Currency

  public get currenciesFilter(): string {
    return `Id ne '${this.ws.settings.FunctionalCurrencyId}'`;
  }

  // Amount in Currency

  public AmountInCurrency_postfix(model: ExchangeRate): string {
    return !!model && !!model.CurrencyId ? ` (${this.ws.getMultilingualValue('Currency', model.CurrencyId, 'Name')})` : ``;
  }

  public AmountInCurrency_decimals(model: ExchangeRate): number {
    const currencyId = !!model ? model.CurrencyId : null;
    const currency = this.ws.get('Currency', currencyId) as Currency;
    return !!currency ? currency.E : this.ws.settings.FunctionalCurrencyDecimals;
  }

  public AmountInCurrency_format(model: ExchangeRate): string {
    const decimals = this.AmountInCurrency_decimals(model);
    return `1.${decimals}-6`;
  }

  // Amount in Functional

  public get functional_decimals(): number {
    return this.ws.settings.FunctionalCurrencyDecimals;
  }

  public get functional_format(): string {
    const decimals = this.functional_decimals;
    return `1.${decimals}-6`;
  }

  public get functional_postfix(): string {
    return ' (' + this.ws.getMultilingualValueImmediate(this.ws.settings, 'FunctionalCurrencyName') + ')';
  }

  public isInactive(): boolean {
    return false;
  }
}
