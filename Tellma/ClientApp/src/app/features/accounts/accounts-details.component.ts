import { Component } from '@angular/core';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { addToWorkspace } from '~/app/data/util';
import { tap } from 'rxjs/operators';
import { WorkspaceService } from '~/app/data/workspace.service';
import { ApiService } from '~/app/data/api.service';
import { TranslateService } from '@ngx-translate/core';
import { AccountForSave, Account, metadata_Account } from '~/app/data/entities/account';
import { PropDescriptor, ChoicePropDescriptor, getChoices } from '~/app/data/entities/base/metadata';
import { Resource } from '~/app/data/entities/resource';
import { AccountType } from '~/app/data/entities/account-type';
import { SelectorChoice } from '~/app/shared/selector/selector.component';
import { Contract } from '~/app/data/entities/contract';

@Component({
  selector: 't-accounts-details',
  templateUrl: './accounts-details.component.html',
  styles: []
})
export class AccountsDetailsComponent extends DetailsBaseComponent {

  private accountsApi = this.api.accountsApi(this.notifyDestruct$); // for intellisense

  public expand = `AccountType/ContractDefinitions,AccountType/NotedContractDefinitions,AccountType/ResourceDefinitions
,Classification,Currency,Center,Contract,Resource/Currency,EntryType`;

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


  private accountType(model: AccountForSave): AccountType {
    if (!model) {
      return null;
    }

    return this.ws.get('AccountType', model.AccountTypeId) as AccountType;
  }

  /////////////// New stuff

  // CenterId

  public showCenter(_: AccountForSave): boolean {
    return this.ws.settings.IsMultiCenter;
  }

  // CurrencyId
  public showCurrency(_: AccountForSave): boolean {
    return true;
  }

  public readonlyCurrencyId(model: AccountForSave): boolean {
    if (!model) {
      return true;
    }

    const resource = this.ws.get('Resource', model.ResourceId) as Resource;
    return !!model.ResourceId && !!resource.CurrencyId;
  }

  public readonlyValueCurrencyId(model: Account): string {
    if (!model) {
      return null;
    }

    const resource = this.ws.get('Resource', model.ResourceId) as Resource;
    return !!model.ResourceId ? resource.CurrencyId : null;
  }

  // Noted Contract Definition

  public showNotedContractDefinitionId(model: Account): boolean {
    const at = this.accountType(model);
    return !!at && !!at.NotedContractDefinitions && at.NotedContractDefinitions.length > 0;
  }

  // Contract Definition
  public get choicesContractDefinitionId(): SelectorChoice[] {
    return getChoices(this.p.ContractDefinitionId as ChoicePropDescriptor);
  }

  public showContractDefinitionId(model: Account): boolean {
    const at = this.accountType(model);
    return !!at && !!at.ContractDefinitions && at.ContractDefinitions.length > 0;
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
  public get choicesResourceDefinitionId(): SelectorChoice[] {
    return getChoices(this.p.ResourceDefinitionId as ChoicePropDescriptor);
  }

  public showResourceDefinitionId(model: Account): boolean {
    const at = this.accountType(model);
    return !!at && !!at.ResourceDefinitions && at.ResourceDefinitions.length > 0;
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

  public filterEntryType(model: AccountForSave) {
    let result = 'IsAssignable eq true';
    const accountType = this.accountType(model);
    if (!!accountType && !!accountType.EntryTypeParentId) {
      result += ` and Node descof ${accountType.EntryTypeParentId}`;
    }

    return result;
  }
}
