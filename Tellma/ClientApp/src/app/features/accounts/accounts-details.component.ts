import { Component } from '@angular/core';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { addToWorkspace } from '~/app/data/util';
import { tap } from 'rxjs/operators';
import { WorkspaceService } from '~/app/data/workspace.service';
import { ApiService } from '~/app/data/api.service';
import { TranslateService } from '@ngx-translate/core';
import { AccountForSave, Account, metadata_Account } from '~/app/data/entities/account';
import { PropDescriptor, getChoices, ChoicePropDescriptor, metadata } from '~/app/data/entities/base/metadata';
import { SelectorChoice } from '~/app/shared/selector/selector.component';
import { Resource } from '~/app/data/entities/resource';
import { Agent } from '~/app/data/entities/agent';
import { AccountType } from '~/app/data/entities/account-type';

@Component({
  selector: 't-accounts-details',
  templateUrl: './accounts-details.component.html',
  styles: []
})
export class AccountsDetailsComponent extends DetailsBaseComponent {

  private accountsApi = this.api.accountsApi(this.notifyDestruct$); // for intellisense

  public expand = `AccountType,CustomClassification,Currency,Center,Agent,Resource/Currency,EntryType`;

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

    result.IsCurrent = false;
    result.HasResource = false;
    result.IsRelated = false;
    result.HasExternalReference = false;
    result.HasAdditionalReference = false;
    result.HasNotedAgentId = false;
    result.HasNotedAgentName = false;
    result.HasNotedAmount = false;
    result.HasNotedDate = false;

    return result;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  public get p(): { [prop: string]: PropDescriptor } {
    return metadata_Account(this.workspace, this.translate, null).properties;
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


  /////////////// New stuff

  // CenterId

  public showCenter(_: AccountForSave): boolean {
    return this.ws.settings.IsMultiCenter;
  }

  // IsCurrent

  public readonlyIsCurrent(model: AccountForSave): boolean {
    if (!model) {
      return true;
    }

    // returns true if the field is meant to stay readonly in edit mode
    const accountType = this.ws.get('AccountType', model.AccountTypeId) as AccountType;
    return false; // !!model.AccountTypeId && (accountType.IsCurrent === true || accountType.IsCurrent === false);
  }

  // AgentDefinitionId
  public showAgentDefinitionId(model: AccountForSave): boolean {
    if (!model || !model.AccountTypeId) {
      return false;
    }

    const accountType = this.ws.get('AccountType', model.AccountTypeId) as AccountType;
    return false; // accountType.IsPersonal;
  }

  public readonlyAgentDefinitionId(model: AccountForSave): boolean {
    if (!model) {
      return true;
    }

    return !!model.AgentId;
  }

  public readonlyValueAgentDefinitionId(model: Account): string {
    if (!model) {
      return null;
    }

    if (!!model.AgentId) {
      return (this.ws.get('Agent', model.AgentId) as Agent).DefinitionId;
    } else {
      return null;
    }
  }

  public get choicesAgentDefinitionId(): SelectorChoice[] {
    const entityDesc = metadata.Account(this.workspace, this.translate, null);
    return getChoices(entityDesc.properties.AgentDefinitionId as ChoicePropDescriptor);
  }

  public formatAgentDefinitionId(defId: string): string {
    if (!defId) {
      return null;
    }

    const def = this.ws.definitions.Agents[defId];
    return this.ws.getMultilingualValueImmediate(def, 'TitlePlural');
  }

  // HasResource
  public showHasResource(model: AccountForSave): boolean {
    if (!model || !model.AccountTypeId) {
      return false;
    }

    const accountType = this.ws.get('AccountType', model.AccountTypeId) as AccountType;
    return false; // !!model.AccountTypeId && accountType.IsReal;
  }

  // IsRelated
  public showIsRelated(model: AccountForSave): boolean {
    if (!model || !model.AccountTypeId) {
      return false;
    }

    const accountType = this.ws.get('AccountType', model.AccountTypeId) as AccountType;
    return false; // accountType.IsPersonal;
  }

  // Agent
  public showAgent(model: AccountForSave): boolean {
    if (!model || !model.AccountTypeId) {
      return false;
    }

    const accountType = this.ws.get('AccountType', model.AccountTypeId) as AccountType;
    return false; // accountType.IsPersonal;
  }

  // Resource
  public showResource(model: AccountForSave): boolean {
    if (!model || !model.AccountTypeId) {
      return false;
    }

    const accountType = this.ws.get('AccountType', model.AccountTypeId) as AccountType;
    return false; // accountType.IsReal && model.HasResource;
  }

  public filterResource(model: AccountForSave) {
    if (!model || !model.AccountTypeId) {
      return null;
    }

    const accountType = this.ws.get('AccountType', model.AccountTypeId) as AccountType;
    if (!!accountType.IsResourceClassification) {
      return `AccountType/Node descof ${model.AccountTypeId}`;
    } else {
      return null;
    }
  }

  // CurrencyId
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

  // EntryTypeId
  public showEntryType(model: AccountForSave) {
    if (!model || !model.AccountTypeId) {
      return false;
    }

    const accountType = this.ws.get('AccountType', model.AccountTypeId) as AccountType;
    return !!accountType.EntryTypeParentId;
  }

  public filterEntryType(model: AccountForSave) {
    if (!model || !model.AccountTypeId) {
      return null;
    }

    const accountType = this.ws.get('AccountType', model.AccountTypeId) as AccountType;
    return `IsAssignable eq true and IsActive eq true and Node descof ${accountType.EntryTypeParentId}`;
  }
}
