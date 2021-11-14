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
import { AccountClassification } from '~/app/data/entities/account-classification';
import { DefinitionsForClient } from '~/app/data/dto/definitions-for-client';
import { Agent } from '~/app/data/entities/agent';

@Component({
  selector: 't-accounts-details',
  templateUrl: './accounts-details.component.html',
  styles: []
})
export class AccountsDetailsComponent extends DetailsBaseComponent {

  private accountsApi = this.api.accountsApi(this.notifyDestruct$); // for intellisense

  public expand = `AccountType.AgentDefinitions,AccountType.ResourceDefinitions,AccountType.NotedAgentDefinitions,AccountType.NotedResourceDefinitions,Classification,
Currency,Center,Agent.Currency,Resource.Currency,NotedAgent.Currency,NotedResource.Currency,
Agent.Center,Resource.Center,NotedAgent.Center,NotedResource.Center,EntryType`;

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

  public onDeactivate = (model: Account): void => {
    if (!!model && !!model.Id) {
      this.accountsApi.deactivate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public showActivate = (model: Account) => !!model && !model.IsActive;
  public showDeactivate = (model: Account) => !!model && model.IsActive;

  public canActivateDeactivateItem = (model: Account) => this.ws.canDo(this.view, 'IsActive', model.Id);

  public activateDeactivateTooltip = (model: Account) => this.canActivateDeactivateItem(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  // CenterId

  public showCenter(_: AccountForSave): boolean {
    return true;
  }

  public readonlyCenterId(model: AccountForSave): boolean {
    return !!this.readonlyValueCenterId(model);
  }

  public readonlyValueCenterId(model: Account): number {
    if (!model) {
      return null;
    }

    const agent = this.ws.get('Agent', model.AgentId) as Agent;
    return (!!agent ? agent.CenterId : null);
  }

  public viewModeCenterId(model: Account): number {
    return this.readonlyCenterId(model) ? this.readonlyValueCenterId(model) : model.CenterId;
  }

  public filterCenter(model: Account): string {
    return null;
  }

  // CurrencyId
  public showCurrency(_: AccountForSave): boolean {
    return true;
  }

  public readonlyCurrencyId(model: AccountForSave): boolean {
    return !!this.readonlyValueCurrencyId(model);
  }

  public readonlyValueCurrencyId(model: Account): string {
    if (!model) {
      return null;
    }

    const agent = this.ws.get('Agent', model.AgentId) as Agent;
    const resource = this.ws.get('Resource', model.ResourceId) as Resource;

    return (!!agent ? agent.CurrencyId : null) ||
      (!!resource ? resource.CurrencyId : null);
  }

  public viewModeCurrencyId(model: Account): string {
    return this.readonlyCurrencyId(model) ? this.readonlyValueCurrencyId(model) : model.CurrencyId;
  }

  // Agent Definition
  private _choicesAgentDefinitionIdDefinitions: DefinitionsForClient;
  private _choicesAgentDefinitionIdAccountType: AccountType;
  private _choicesAgentDefinitionIdResult: SelectorChoice[] = [];
  public choicesAgentDefinitionId(model: Account): SelectorChoice[] {

    const ws = this.ws;
    const defs = ws.definitions;
    const at = this.accountType(model);
    if (this._choicesAgentDefinitionIdAccountType !== at ||
      this._choicesAgentDefinitionIdDefinitions !== defs) {
      this._choicesAgentDefinitionIdAccountType = at;
      this._choicesAgentDefinitionIdDefinitions = defs;

      if (!at || !at.AgentDefinitions) {
        this._choicesAgentDefinitionIdResult = [];
      } else {
        this._choicesAgentDefinitionIdResult = at.AgentDefinitions
          .filter(d => !!defs.Agents[d.AgentDefinitionId])
          .map(d =>
          ({
            value: d.AgentDefinitionId,
            name: () => ws.getMultilingualValueImmediate(defs.Agents[d.AgentDefinitionId], 'TitleSingular')
          }));
      }
    }

    return this._choicesAgentDefinitionIdResult;
  }

  public showAgentDefinitionId(model: Account): boolean {
    return this.choicesAgentDefinitionId(model).length > 0;
  }

  public formatAgentDefinitionId(defId: number): string {
    if (!defId) {
      return '';
    }

    const def = this.ws.definitions.Agents[defId];
    return this.ws.getMultilingualValueImmediate(def, 'TitleSingular');
  }

  public onAgentDefinitionChange(defId: number, model: Account) {
    // Delete the AgentId if an incompatible definition is selected
    if (!defId) {
      return;
    }

    const agent = this.ws.get('Agent', model.AgentId) as Agent;
    if (!!agent && agent.DefinitionId !== defId) {
      delete model.AgentId;
    }
  }

  // Agent
  public labelAgent(model: AccountForSave): string {
    let postfix = '';
    if (!!model && !!model.AgentDefinitionId) {
      const agentDef = this.ws.definitions.Agents[model.AgentDefinitionId];
      if (!!agentDef) {
        postfix = ` (${this.ws.getMultilingualValueImmediate(agentDef, 'TitleSingular')})`;
      }
    }
    return this.translate.instant('Account_Agent') + postfix;
  }

  public showAgent(model: AccountForSave): boolean {
    return this.showAgentDefinitionId(model) && !!model.AgentDefinitionId;
  }

  public definitionIdsAgent(model: AccountForSave): number[] {
    if (!!model && !!model.AgentDefinitionId) {
      return [model.AgentDefinitionId];
    } else {
      return [];
    }
  }

  // Resource Definition
  private _choicesResourceDefinitionIdDefinitions: DefinitionsForClient;
  private _choicesResourceDefinitionIdAccountType: AccountType;
  private _choicesResourceDefinitionIdResult: SelectorChoice[] = [];
  public choicesResourceDefinitionId(model: Account): SelectorChoice[] {

    const ws = this.ws;
    const defs = ws.definitions;
    const at = this.accountType(model);
    if (this._choicesResourceDefinitionIdAccountType !== at ||
      this._choicesResourceDefinitionIdDefinitions !== defs) {
      this._choicesResourceDefinitionIdAccountType = at;
      this._choicesResourceDefinitionIdDefinitions = defs;

      if (!at || !at.ResourceDefinitions) {
        this._choicesResourceDefinitionIdResult = [];
      } else {
        this._choicesResourceDefinitionIdResult = at.ResourceDefinitions
          .filter(d => !!defs.Resources[d.ResourceDefinitionId])
          .map(d =>
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
    return this.ws.getMultilingualValueImmediate(def, 'TitleSingular');
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

  // NotedAgent Definition
  private _choicesNotedAgentDefinitionIdDefinitions: DefinitionsForClient;
  private _choicesNotedAgentDefinitionIdAccountType: AccountType;
  private _choicesNotedAgentDefinitionIdResult: SelectorChoice[] = [];
  public choicesNotedAgentDefinitionId(model: Account): SelectorChoice[] {

    const ws = this.ws;
    const defs = ws.definitions;
    const at = this.accountType(model);
    if (this._choicesNotedAgentDefinitionIdAccountType !== at ||
      this._choicesNotedAgentDefinitionIdDefinitions !== defs) {
      this._choicesNotedAgentDefinitionIdAccountType = at;
      this._choicesNotedAgentDefinitionIdDefinitions = defs;

      if (!at || !at.NotedAgentDefinitions) {
        this._choicesNotedAgentDefinitionIdResult = [];
      } else {
        this._choicesNotedAgentDefinitionIdResult = at.NotedAgentDefinitions
          .filter(d => !!defs.Agents[d.NotedAgentDefinitionId])
          .map(d =>
          ({
            value: d.NotedAgentDefinitionId,
            name: () => ws.getMultilingualValueImmediate(defs.Agents[d.NotedAgentDefinitionId], 'TitleSingular')
          }));
      }
    }

    return this._choicesNotedAgentDefinitionIdResult;
  }

  public showNotedAgentDefinitionId(model: Account): boolean {
    return this.choicesNotedAgentDefinitionId(model).length > 0;
  }

  public formatNotedAgentDefinitionId(defId: number): string {
    if (!defId) {
      return '';
    }

    const def = this.ws.definitions.Agents[defId];
    return this.ws.getMultilingualValueImmediate(def, 'TitleSingular');
  }

  public onNotedAgentDefinitionChange(defId: number, model: Account) {
    // Delete the NotedAgentId if an incompatible definition is selected
    if (!defId) {
      return;
    }

    const notedagent = this.ws.get('Agent', model.NotedAgentId) as Agent;
    if (!!notedagent && notedagent.DefinitionId !== defId) {
      delete model.NotedAgentId;
    }
  }

  // NotedAgent
  public labelNotedAgent(model: AccountForSave): string {
    let postfix = '';
    if (!!model && !!model.NotedAgentDefinitionId) {
      const notedagentDef = this.ws.definitions.Agents[model.NotedAgentDefinitionId];
      if (!!notedagentDef) {
        postfix = ` (${this.ws.getMultilingualValueImmediate(notedagentDef, 'TitleSingular')})`;
      }
    }
    return this.translate.instant('Account_NotedAgent') + postfix;
  }

  public showNotedAgent(model: AccountForSave): boolean {
    return this.showNotedAgentDefinitionId(model) && !!model.NotedAgentDefinitionId;
  }

  public definitionIdsNotedAgent(model: AccountForSave): number[] {
    if (!!model && !!model.NotedAgentDefinitionId) {
      return [model.NotedAgentDefinitionId];
    } else {
      return [];
    }
  }

  // NotedResource Definition
  private _choicesNotedResourceDefinitionIdDefinitions: DefinitionsForClient;
  private _choicesNotedResourceDefinitionIdAccountType: AccountType;
  private _choicesNotedResourceDefinitionIdResult: SelectorChoice[] = [];
  public choicesNotedResourceDefinitionId(model: Account): SelectorChoice[] {

    const ws = this.ws;
    const defs = ws.definitions;
    const at = this.accountType(model);
    if (this._choicesNotedResourceDefinitionIdAccountType !== at ||
      this._choicesNotedResourceDefinitionIdDefinitions !== defs) {
      this._choicesNotedResourceDefinitionIdAccountType = at;
      this._choicesNotedResourceDefinitionIdDefinitions = defs;

      if (!at || !at.NotedResourceDefinitions) {
        this._choicesNotedResourceDefinitionIdResult = [];
      } else {
        this._choicesNotedResourceDefinitionIdResult = at.NotedResourceDefinitions
          .filter(d => !!defs.Resources[d.NotedResourceDefinitionId])
          .map(d =>
          ({
            value: d.NotedResourceDefinitionId,
            name: () => ws.getMultilingualValueImmediate(defs.Resources[d.NotedResourceDefinitionId], 'TitleSingular')
          }));
      }
    }

    return this._choicesNotedResourceDefinitionIdResult;
  }

  public showNotedResourceDefinitionId(model: Account): boolean {
    return this.choicesNotedResourceDefinitionId(model).length > 0;
  }

  public formatNotedResourceDefinitionId(defId: number): string {
    if (!defId) {
      return '';
    }

    const def = this.ws.definitions.Resources[defId];
    return this.ws.getMultilingualValueImmediate(def, 'TitleSingular');
  }

  public onNotedResourceDefinitionChange(defId: number, model: Account) {
    // Delete the NotedResourceId if an incompatible definition is selected
    if (!defId) {
      return;
    }

    const notedresource = this.ws.get('Resource', model.NotedResourceId) as Resource;
    if (!!notedresource && notedresource.DefinitionId !== defId) {
      delete model.NotedResourceId;
    }
  }

  // NotedResource
  public labelNotedResource(model: AccountForSave): string {
    let postfix = '';
    if (!!model && !!model.NotedResourceDefinitionId) {
      const notedresourceDef = this.ws.definitions.Resources[model.NotedResourceDefinitionId];
      if (!!notedresourceDef) {
        postfix = ` (${this.ws.getMultilingualValueImmediate(notedresourceDef, 'TitleSingular')})`;
      }
    }
    return this.translate.instant('Account_NotedResource') + postfix;
  }

  public showNotedResource(model: AccountForSave): boolean {
    return this.showNotedResourceDefinitionId(model) && !!model.NotedResourceDefinitionId;
  }

  public definitionIdsNotedResource(model: AccountForSave): number[] {
    if (!!model && !!model.NotedResourceDefinitionId) {
      return [model.NotedResourceDefinitionId];
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
      result += ` and Id descof ${accountType.EntryTypeParentId}`;
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
    const defaultSelect = `AgentDefinitions.AgentDefinitionId,ResourceDefinitions.ResourceDefinitionId,
    NotedAgentDefinitions.NotedAgentDefinitionId,NotedResourceDefinitions.NotedResourceDefinitionId,EntryTypeParentId`;

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
      return `Id descof ${classification.AccountTypeParentId}`;
    }

    return '';
  }

  public get resourceAdditionalSelect(): string {
    const defaultSelect = `DefinitionId,Currency.Name,Currency.Name2,Currency.Name3,Center.Name,Center.Name2,Center.Name3`;
    if (this.additionalSelect === '$DocumentDetails') {
      // Popup from document screen, get everything the document screen needs
      return '$DocumentDetails,' + defaultSelect;
    } else {
      // Just the account screen, get what the account screen needs
      return defaultSelect;
    }
  }

  public get agentAdditionalSelect(): string {
    const defaultSelect = `DefinitionId,Currency.Name,Currency.Name2,Currency.Name3,Center.Name,Center.Name2,Center.Name3`;
    if (this.additionalSelect === '$DocumentDetails') {
      // Popup from document screen, get everything the document screen needs
      return '$DocumentDetails,' + defaultSelect;
    } else {
      // Just the account screen, get what the account screen needs
      return defaultSelect;
    }
  }

  public get notedAgentAdditionalSelect(): string {
    const defaultSelect = `DefinitionId,Currency.Name,Currency.Name2,Currency.Name3,Center.Name,Center.Name2,Center.Name3`;
    if (this.additionalSelect === '$DocumentDetails') {
      // Popup from document screen, get everything the document screen needs
      return '$DocumentDetails,' + defaultSelect;
    } else {
      // Just the account screen, get what the account screen needs
      return defaultSelect;
    }
  }

  public get notedResourceAdditionalSelect(): string {
    const defaultSelect = `DefinitionId,Currency.Name,Currency.Name2,Currency.Name3,Center.Name,Center.Name2,Center.Name3`;
    if (this.additionalSelect === '$DocumentDetails') {
      // Popup from document screen, get everything the document screen needs
      return '$DocumentDetails,' + defaultSelect;
    } else {
      // Just the account screen, get what the account screen needs
      return defaultSelect;
    }
  }
}
