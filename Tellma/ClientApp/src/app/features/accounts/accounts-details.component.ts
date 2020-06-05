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

@Component({
  selector: 't-accounts-details',
  templateUrl: './accounts-details.component.html',
  styles: []
})
export class AccountsDetailsComponent extends DetailsBaseComponent {

  private accountsApi = this.api.accountsApi(this.notifyDestruct$); // for intellisense

  public expand = `AccountType,CustomClassification,Currency,Center,Contract,Resource/Currency,EntryType`;

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

    result.IsRelated = false;
    result.IsSmart = false;

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

  public showCenter(model: AccountForSave): boolean {
    const accountType = this.accountType(model);
    const isAccountAssignment = !!accountType && accountType.CenterAssignment === 'A';
    const isSmart = !!model && model.IsSmart;

    return this.ws.settings.IsMultiCenter && (!isSmart || isAccountAssignment);
  }

  // CurrencyId
  public showCurrency(model: AccountForSave): boolean {
    const accountType = this.accountType(model);
    const isAccountAssignment = !!accountType && accountType.CurrencyAssignment === 'A';
    const isSmart = !!model && model.IsSmart;

    // Currency is required in dumb accounts,
    return !isSmart || isAccountAssignment;
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

  // IsRelated
  public showIsRelated(_: AccountForSave): boolean {
    // if (!model || !model.AccountTypeId) {
    //   return false;
    // }

    // const accountType = this.accountType(model);
    return false; // accountType.IsPersonal;
  }

  // Contract
  public showContract(model: AccountForSave): boolean {
    const accountType = this.accountType(model);
    const isSmart = !!model && model.IsSmart;
    const isAccountAssignment = !!accountType && accountType.ContractAssignment === 'A';

    return isSmart && isAccountAssignment;
  }

  public labelContract(model: AccountForSave): string {
    let postfix = '';
    const accountType = this.accountType(model);
    if (!!accountType && !!accountType.ContractDefinitionId) {
      const contractDef = this.ws.definitions.Contracts[accountType.ContractDefinitionId];
      if (!!contractDef) {
        postfix = ` (${this.ws.getMultilingualValueImmediate(contractDef, 'TitleSingular')})`;
      }
    }
    return this.translate.instant('Account_Contract') + postfix;
  }

  public definitionIdsContract(model: AccountForSave): string[] {
    const accountType = this.accountType(model);
    if (!!accountType && !!accountType.ContractDefinitionId) {
      return [accountType.ContractDefinitionId];
    } else {
      return [];
    }
  }

  // Resource
  public labelResource(model: AccountForSave): string {
    let postfix = '';
    const accountType = this.accountType(model);
    if (!!accountType && !!accountType.ResourceDefinitionId) {
      const resourceDef = this.ws.definitions.Resources[accountType.ResourceDefinitionId];
      if (!!resourceDef) {
        postfix = ` (${this.ws.getMultilingualValueImmediate(resourceDef, 'TitleSingular')})`;
      }
    }
    return this.translate.instant('Account_Resource') + postfix;
  }

  public showResource(model: AccountForSave): boolean {
    const accountType = this.accountType(model);
    const isAccountAssignment = !!accountType && accountType.ResourceAssignment === 'A';
    const isSmart = !!model && model.IsSmart;

    return isSmart && isAccountAssignment;
  }

  public filterResource(model: AccountForSave) {
    if (!model || !model.AccountTypeId) {
      return null;
    }

    const accountType = this.accountType(model);
    if (!!accountType.IsResourceClassification) {
      return `AssetType/Node descof ${model.AccountTypeId}`;
    } else {
      return null;
    }
  }

  public definitionIdsResource(model: AccountForSave): string[] {
    const accountType = this.accountType(model);
    if (!!accountType && !!accountType.ResourceDefinitionId) {
      return [accountType.ResourceDefinitionId];
    } else {
      return [];
    }
  }

  // Identifier

  public showIdentifier(model: AccountForSave): boolean {
    const accountType = this.accountType(model);
    const isAccountAssignment = !!accountType && accountType.IdentifierAssignment === 'A';
    const isSmart = !!model && model.IsSmart;

    return isSmart && isAccountAssignment;
  }

  public labelIdentifier(model: AccountForSave): string {
    let postfix = '';
    const accountType = this.accountType(model);
    if (!!accountType.IdentifierLabel) {
      postfix = ` (${this.ws.getMultilingualValueImmediate(accountType, 'IdentifierLabel')})`;
    }

    return this.translate.instant('Account_Identifier') + postfix;
  }

  // EntryTypeId
  public showEntryType(model: AccountForSave) {
    const accountType = this.accountType(model);
    const isSmart = !!model && model.IsSmart;
    const isAccountAssignment = !!accountType && accountType.EntryTypeAssignment === 'A';

    return isSmart && isAccountAssignment;
  }

  public filterEntryType(model: AccountForSave) {
    let result = 'IsAssignable eq true and IsActive eq true';
    const accountType = this.accountType(model);
    if (!!accountType && !!accountType.EntryTypeParentId) {
      result += ` and Node descof ${accountType.EntryTypeParentId}`;
    }

    return result;
  }
}
