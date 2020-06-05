import { Component, Input, OnInit } from '@angular/core';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { Contract, ContractForSave } from '~/app/data/entities/contract';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { TranslateService } from '@ngx-translate/core';
import { ActivatedRoute, ParamMap } from '@angular/router';
import { ContractDefinitionForClient } from '~/app/data/dto/definitions-for-client';

@Component({
  selector: 't-contracts-details',
  templateUrl: './contracts-details.component.html'
})
export class ContractsDetailsComponent extends DetailsBaseComponent implements OnInit {

  private contractsApi = this.api.contractsApi(null, this.notifyDestruct$); // for intellisense
  private _definitionId: number;

  @Input()
  public set definitionId(t: number) {
    if (this._definitionId !== t) {
      this.contractsApi = this.api.contractsApi(t, this.notifyDestruct$);
      this._definitionId = t;
    }
  }

  public get definitionId(): number {
    return this._definitionId;
  }

  public expand = 'User,Rates/Resource,Rates/Unit,Rates/Currency';

  create = () => {
    const result: ContractForSave = {};
    if (this.ws.isPrimaryLanguage) {
      result.Name = this.initialText;
    } else if (this.ws.isSecondaryLanguage) {
      result.Name2 = this.initialText;
    } else if (this.ws.isTernaryLanguage) {
      result.Name3 = this.initialText;
    }
    result.IsRelated = false;

    // TODO Set defaults from definition

    return result;
  }

  clone = (item: Contract): Contract => {

    if (!!item) {
      const clone = JSON.parse(JSON.stringify(item)) as Contract;
      clone.Id = null;

      return clone;
    } else {
      // programmer mistake
      console.error('Cloning a non existing item');
      return null;
    }
  }

  constructor(
    private workspace: WorkspaceService, private api: ApiService,
    private translate: TranslateService, private route: ActivatedRoute) {
    super();
  }

  ngOnInit() {
    this.route.paramMap.subscribe((params: ParamMap) => {
      // This triggers changes on the screen

      if (this.isScreenMode) {

        const definitionId = +params.get('definitionId');

        if (this.definitionId !== definitionId) {
          this.definitionId = definitionId;
        }
      }
    });
  }

  get view(): string {
    return `contracts/${this.definitionId}`;
  }

  private get definition(): ContractDefinitionForClient {
    return !!this.definitionId ? this.ws.definitions.Contracts[this.definitionId] : null;
  }

  // UI Bindings

  public get found(): boolean {
    return !!this.definition;
  }

  public onActivate = (model: Contract): void => {
    if (!!model && !!model.Id) {
      this.contractsApi.activate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onDeactivate = (model: Contract): void => {
    if (!!model && !!model.Id) {
      this.contractsApi.deactivate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public showActivate = (model: Contract) => !!model && !model.IsActive;
  public showDeactivate = (model: Contract) => !!model && model.IsActive;

  public canActivateDeactivateItem = (model: Contract) => this.ws.canDo(this.view, 'IsActive', model.Id);

  public activateDeactivateTooltip = (model: Contract) => this.canActivateDeactivateItem(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  public get ws() {
    return this.workspace.currentTenant;
  }

  public get masterCrumb(): string {
    return this.ws.getMultilingualValueImmediate(this.definition, 'TitlePlural');
  }

  public get TaxIdentificationNumber_isVisible(): boolean {
    return !!this.definition.TaxIdentificationNumberVisibility;
  }

  public get TaxIdentificationNumber_isRequired(): boolean {
    return this.definition.TaxIdentificationNumberVisibility === 'Required';
  }

  public get Image_isVisible(): boolean {
    return !!this.definition.ImageVisibility;
  }

  public get StartDate_isVisible(): boolean {
    return !!this.definition.StartDateVisibility;
  }

  public get StartDate_isRequired(): boolean {
    return this.definition.StartDateVisibility === 'Required';
  }

  public get StartDate_label(): string {
    return !!this.definition.StartDateLabel ?
      this.ws.getMultilingualValueImmediate(this.definition, 'StartDateLabel') :
      this.translate.instant('Contract_StartDate');
  }

  public get Job_isVisible(): boolean {
    return !!this.definition.JobVisibility;
  }

  public get Job_isRequired(): boolean {
    return this.definition.JobVisibility === 'Required';
  }

  public get BankAccountNumber_isVisible(): boolean {
    return !!this.definition.BankAccountNumberVisibility;
  }

  public get BankAccountNumber_isRequired(): boolean {
    return this.definition.BankAccountNumberVisibility === 'Required';
  }

  public get Tabs_isVisible(): boolean {
    return false; // More tabs may be added
  }

  // public get Rates_isVisible(): boolean {
  //   return !!this.definition.RatesVisibility;
  // }

  // public get Rates_label(): string {
  //   return !!this.definition.RatesLabel ?
  //     this.ws.getMultilingualValueImmediate(this.definition, 'RatesLabel') :
  //     this.translate.instant('Contract_Rates');
  // }

  // public Rates_count(model: ContractForSave): number {
  //   return !!model && !!model.Rates ? model.Rates.length : 0;
  // }

  // public Rates_showError(model: ContractForSave): boolean {
  //   return !!model && !!model.Rates && model.Rates.some(e => !!e.serverErrors);
  // }

  // public Rate_minDecimalPlaces(line: ContractRateForSave): number {
  //   const curr = this.ws.get('Currency', line.CurrencyId) as Currency;
  //   return !!curr ? curr.E : this.ws.settings.FunctionalCurrencyDecimals;
  // }

  // public Rate_maxDecimalPlaces(line: ContractRateForSave): number {
  //   const curr = this.ws.get('Currency', line.CurrencyId) as Currency;
  //   return !!curr ? curr.E : this.ws.settings.FunctionalCurrencyDecimals;
  // }

  // public Rate_format(line: ContractRateForSave): string {
  //   return `1.${this.Rate_minDecimalPlaces(line)}-${this.Rate_maxDecimalPlaces(line)}`;
  // }

}
