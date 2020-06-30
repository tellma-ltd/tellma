// tslint:disable:member-ordering
import { Component } from '@angular/core';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { addToWorkspace } from '~/app/data/util';
import { tap } from 'rxjs/operators';
import { WorkspaceService } from '~/app/data/workspace.service';
import { ApiService } from '~/app/data/api.service';
import { TranslateService } from '@ngx-translate/core';
import { AccountForSave, Account, metadata_Account } from '~/app/data/entities/account';
import { PropDescriptor } from '~/app/data/entities/base/metadata';
import { Resource } from '~/app/data/entities/resource';
import { AccountType } from '~/app/data/entities/account-type';
import { SelectorChoice } from '~/app/shared/selector/selector.component';
import { Contract } from '~/app/data/entities/contract';
import { AccountClassification } from '~/app/data/entities/account-classification';

@Component({
  selector: 't-accounts-details',
  templateUrl: './accounts-details.component.html',
  styles: []
})
export class AccountsDetailsComponent extends DetailsBaseComponent {

  private accountsApi = this.api.accountsApi(this.notifyDestruct$); // for intellisense

  public expand = `AccountType/ContractDefinitions,AccountType/NotedContractDefinitions,AccountType/ResourceDefinitions
,Classification,Currency,Center,Contract,Resource/Currency,Contract/Currency,Resource/Center,Contract/Center,EntryType`;

  constructor(
    private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();

    this.accountsApi = this.api.accountsApi(this.notifyDestruct$);
  }

  private get view(): string {
    return `accounts`;
  }

  // UI Binding

  create = () => {
    const result: AccountForSave = {};
    if (this.ws.isPrimaryLanguage) {
      result.Name = this.initialText;
    } else if (this.ws.isSecondaryLanguage) {
      result.Name2 = this.initialText;
    } else if (this.ws.isTernaryLanguage) {
      result.Name3 = this.initialText;
    }

    return result;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  public get p(): { [prop: string]: PropDescriptor } {
    return metadata_Account(this.workspace, this.translate).properties;
  }

  public onActivate = (model: Account): void => {
    if (!!model && !!model.Id) {
      this.accountsApi.activate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onDeprecate = (model: Account): void => {
    if (!!model && !!model.Id) {
      this.accountsApi.deactivate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public showActivate = (model: Account) => !!model && model.IsDeprecated;
  public showDeprecate = (model: Account) => !!model && !model.IsDeprecated;

  public canActivateDeprecateItem = (model: Account) => this.ws.canDo(this.view, 'IsDeprecated', model.Id);

  public activateDeprecateTooltip = (model: Account) => this.canActivateDeprecateItem(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  // CenterId

  public showCenter(_: AccountForSave): boolean {
    return true;
  }


  public readonlyCenterId(model: AccountForSave): boolean {
    if (!model) {
      return true;
    }

    // The center becomes readonly if either the resource or the contract have a center selected
    const resource = this.ws.get('Resource', model.ResourceId) as Resource;
    const contract = this.ws.get('Contract', model.ContractId) as Contract;
    return (!!model.ResourceId && !!resource.CenterId) || (!!model.ContractId && !!contract.CenterId);
  }

  public readonlyValueCenterId(model: Account): number {
    if (!model) {
      return null;
    }

    if (!!model.ResourceId) {
      const resource = this.ws.get('Resource', model.ResourceId) as Resource;
      if (!!resource.CenterId) {
        return resource.CenterId;
      }
    }

    if (!!model.ContractId) {
      const contract = this.ws.get('Contract', model.ContractId) as Contract;
      if (!!contract.CenterId) {
        return contract.CenterId;
      }
    }

    return null;
  }

  // CurrencyId
  public showCurrency(_: AccountForSave): boolean {
    return true;
  }

  public readonlyCurrencyId(model: AccountForSave): boolean {
    if (!model) {
      return true;
    }

    // The currency becomes readonly if either the resource or the contract have a currency selected
    const resource = this.ws.get('Resource', model.ResourceId) as Resource;
    const contract = this.ws.get('Contract', model.ContractId) as Contract;
    return (!!model.ResourceId && !!resource.CurrencyId) || (!!model.ContractId && !!contract.CurrencyId);
  }

  public readonlyValueCurrencyId(model: Account): string {
    if (!model) {
      return null;
    }

    if (!!model.ResourceId) {
      const resource = this.ws.get('Resource', model.ResourceId) as Resource;
      if (!!resource.CurrencyId) {
        return resource.CurrencyId;
      }
    }

    if (!!model.ContractId) {
      const contract = this.ws.get('Contract', model.ContractId) as Contract;
      if (!!contract.CurrencyId) {
        return contract.CurrencyId;
      }
    }

    return null;
  }

  // Noted Contract Definition
  private _choicesNotedContractDefinitionIdAccountType: AccountType;
  private _choicesNotedContractDefinitionIdResult: SelectorChoice[] = [];
  public choicesNotedContractDefinitionId(model: Account): SelectorChoice[] {

    const at = this.accountType(model);
    if (this._choicesNotedContractDefinitionIdAccountType !== at) {
      this._choicesNotedContractDefinitionIdAccountType = at;

      if (!at || !at.NotedContractDefinitions) {
        this._choicesNotedContractDefinitionIdResult = [];
      } else {
        const ws = this.ws;
        const defs = ws.definitions;
        this._choicesNotedContractDefinitionIdResult = at.NotedContractDefinitions.map(d =>
          ({
            value: d.NotedContractDefinitionId,
            name: () => ws.getMultilingualValueImmediate(defs.Contracts[d.NotedContractDefinitionId], 'TitleSingular')
          }));
      }
    }

    return this._choicesNotedContractDefinitionIdResult;
  }

  public showNotedContractDefinitionId(model: Account): boolean {
    return this.choicesNotedContractDefinitionId(model).length > 0;
  }

  // Contract Definition
  private _choicesContractDefinitionIdAccountType: AccountType;
  private _choicesContractDefinitionIdResult: SelectorChoice[] = [];
  public choicesContractDefinitionId(model: Account): SelectorChoice[] {

    const at = this.accountType(model);
    if (this._choicesContractDefinitionIdAccountType !== at) {
      this._choicesContractDefinitionIdAccountType = at;

      if (!at || !at.ContractDefinitions) {
        this._choicesContractDefinitionIdResult = [];
      } else {
        const ws = this.ws;
        const defs = ws.definitions;
        this._choicesContractDefinitionIdResult = at.ContractDefinitions.map(d =>
          ({
            value: d.ContractDefinitionId,
            name: () => ws.getMultilingualValueImmediate(defs.Contracts[d.ContractDefinitionId], 'TitleSingular')
          }));
      }
    }

    return this._choicesContractDefinitionIdResult;
  }

  public showContractDefinitionId(model: Account): boolean {
    return this.choicesContractDefinitionId(model).length > 0;
  }

  public formatContractDefinitionId(defId: number): string {
    if (!defId) {
      return '';
    }

    const def = this.ws.definitions.Contracts[defId];
    return this.ws.getMultilingualValueImmediate(def, 'TitlePlural');
  }

  public onContractDefinitionChange(defId: number, model: Account) {
    // Delete the ContractId if an incompatible definition is selected
    if (!defId) {
      // Will be deleted by the server anyways
      return;
    }

    const contract = this.ws.get('Contract', model.ContractId) as Contract;
    if (!!contract && contract.DefinitionId !== defId) {
      delete model.ContractId;
    }
  }

  // Contract
  public showContract(model: AccountForSave): boolean {
    return this.showContractDefinitionId(model) && !!model.ContractDefinitionId;
  }

  public labelContract(model: AccountForSave): string {
    let postfix = '';
    if (!!model && !!model.ContractDefinitionId) {
      const contractDef = this.ws.definitions.Contracts[model.ContractDefinitionId];
      if (!!contractDef) {
        postfix = ` (${this.ws.getMultilingualValueImmediate(contractDef, 'TitleSingular')})`;
      }
    }
    return this.translate.instant('Account_Contract') + postfix;
  }

  public definitionIdsContract(model: AccountForSave): number[] {
    if (!!model && !!model.ContractDefinitionId) {
      return [model.ContractDefinitionId];
    } else {
      return [];
    }
  }

  // Resource Definition
  private _choicesResourceDefinitionIdAccountType: AccountType;
  private _choicesResourceDefinitionIdResult: SelectorChoice[] = [];
  public choicesResourceDefinitionId(model: Account): SelectorChoice[] {

    const at = this.accountType(model);
    if (this._choicesResourceDefinitionIdAccountType !== at) {
      this._choicesResourceDefinitionIdAccountType = at;

      if (!at || !at.ResourceDefinitions) {
        this._choicesResourceDefinitionIdResult = [];
      } else {
        const ws = this.ws;
        const defs = ws.definitions;
        this._choicesResourceDefinitionIdResult = at.ResourceDefinitions.map(d =>
          ({
            value: d.ResourceDefinitionId,
            name: () => ws.getMultilingualValueImmediate(defs.Resources[d.ResourceDefinitionId], 'TitleSingular')
          }));
      }
    }

    return this._choicesResourceDefinitionIdResult;
  }

  public showResourceDefinitionId(model: Account): boolean {
    return this.choicesResourceDefinitionId(model).length > 0;
  }

  public formatResourceDefinitionId(defId: number): string {
    if (!defId) {
      return '';
    }

    const def = this.ws.definitions.Resources[defId];
    return this.ws.getMultilingualValueImmediate(def, 'TitlePlural');
  }

  public onResourceDefinitionChange(defId: number, model: Account) {
    // Delete the ResourceId if an incompatible definition is selected
    if (!defId) {
      return;
    }

    const resource = this.ws.get('Resource', model.ResourceId) as Resource;
    if (!!resource && resource.DefinitionId !== defId) {
      delete model.ResourceId;
    }
  }

  // Resource
  public labelResource(model: AccountForSave): string {
    let postfix = '';
    if (!!model && !!model.ResourceDefinitionId) {
      const resourceDef = this.ws.definitions.Resources[model.ResourceDefinitionId];
      if (!!resourceDef) {
        postfix = ` (${this.ws.getMultilingualValueImmediate(resourceDef, 'TitleSingular')})`;
      }
    }
    return this.translate.instant('Account_Resource') + postfix;
  }

  public showResource(model: AccountForSave): boolean {
    return this.showResourceDefinitionId(model) && !!model.ResourceDefinitionId;
  }

  public definitionIdsResource(model: AccountForSave): number[] {
    if (!!model && !!model.ResourceDefinitionId) {
      return [model.ResourceDefinitionId];
    } else {
      return [];
    }
  }

  // EntryTypeId
  public showEntryType(model: AccountForSave) {
    const accountType = this.accountType(model);

    return !!accountType && !!accountType.EntryTypeParentId;
  }

  public filterEntryType(model: AccountForSave): string {
    let result = 'IsAssignable eq true';
    const accountType = this.accountType(model);
    if (!!accountType && !!accountType.EntryTypeParentId) {
      result += ` and Node descof ${accountType.EntryTypeParentId}`;
    }

    return result;
  }

  // Account Type

  private accountType(model: AccountForSave): AccountType {
    if (!model) {
      return null;
    }

    return this.ws.get('AccountType', model.AccountTypeId) as AccountType;
  }

  public get accountTypeAdditionalSelect(): string {
    const defaultSelect = `ContractDefinitions/ContractDefinitionId,
    NotedContractDefinitions/NotedContractDefinitionId,
    ResourceDefinitions/ResourceDefinitionId,EntryTypeParentId`;

    if (this.additionalSelect === '$DocumentDetails') {
      // Popup from document screen, get everything the document screen needs
      return '$DocumentDetails,' + defaultSelect;
    } else {
      // Just the account screen, get what the account screen needs
      return defaultSelect;
    }
  }

  public filterAccountType(model: AccountForSave): string {
    // Add account type parent Id from classification
    const classification = this.ws.get('AccountClassification', model.ClassificationId) as AccountClassification;
    if (!!classification && !!classification.AccountTypeParentId) {
      return `Node descof ${classification.AccountTypeParentId}`;
    }

    return '';
  }

  public get resourceAdditionalSelect(): string {
    const defaultSelect = `DefinitionId,Currency/Name,Currency/Name2,Currency/Name3,Center/Name,Center/Name2,Center/Name3`;
    if (this.additionalSelect === '$DocumentDetails') {
      // Popup from document screen, get everything the document screen needs
      return '$DocumentDetails,' + defaultSelect;
    } else {
      // Just the account screen, get what the account screen needs
      return defaultSelect;
    }
  }

  public contractAdditionalSelect =
    `DefinitionId,Currency/Name,Currency/Name2,Currency/Name3,Currency/E,Center/Name,Center/Name2,Center/Name3`;
}
