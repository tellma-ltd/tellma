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
import { Relation } from '~/app/data/entities/relation';

@Component({
  selector: 't-accounts-details',
  templateUrl: './accounts-details.component.html',
  styles: []
})
export class AccountsDetailsComponent extends DetailsBaseComponent {

  private accountsApi = this.api.accountsApi(this.notifyDestruct$); // for intellisense

  public expand = `AccountType.RelationDefinitions,AccountType.ResourceDefinitions,AccountType.NotedRelationDefinitions,Classification,
Currency,Center,Relation.Currency,Resource.Currency,NotedRelation.Currency,
Relation.Center,Resource.Center,NotedRelation.Center,EntryType`;

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

    const relation = this.ws.get('Relation', model.RelationId) as Relation;
    const resource = this.ws.get('Resource', model.ResourceId) as Resource;
    const notedRelation = this.ws.get('Relation', model.NotedRelationId) as Relation;

    return (!!relation ? relation.CenterId : null) ||
      (!!resource ? resource.CenterId : null) ||
      (!!notedRelation ? notedRelation.CenterId : null);
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

    const relation = this.ws.get('Relation', model.RelationId) as Relation;
    const resource = this.ws.get('Resource', model.ResourceId) as Resource;
    const notedRelation = this.ws.get('Relation', model.NotedRelationId) as Relation;

    return (!!relation ? relation.CurrencyId : null) ||
      (!!resource ? resource.CurrencyId : null) ||
      (!!notedRelation ? notedRelation.CurrencyId : null);
  }

  // Relation Definition
  private _choicesRelationDefinitionIdDefinitions: DefinitionsForClient;
  private _choicesRelationDefinitionIdAccountType: AccountType;
  private _choicesRelationDefinitionIdResult: SelectorChoice[] = [];
  public choicesRelationDefinitionId(model: Account): SelectorChoice[] {

    const ws = this.ws;
    const defs = ws.definitions;
    const at = this.accountType(model);
    if (this._choicesRelationDefinitionIdAccountType !== at ||
      this._choicesRelationDefinitionIdDefinitions !== defs) {
      this._choicesRelationDefinitionIdAccountType = at;
      this._choicesRelationDefinitionIdDefinitions = defs;

      if (!at || !at.RelationDefinitions) {
        this._choicesRelationDefinitionIdResult = [];
      } else {
        this._choicesRelationDefinitionIdResult = at.RelationDefinitions
          .filter(d => !!defs.Relations[d.RelationDefinitionId])
          .map(d =>
          ({
            value: d.RelationDefinitionId,
            name: () => ws.getMultilingualValueImmediate(defs.Relations[d.RelationDefinitionId], 'TitleSingular')
          }));
      }
    }

    return this._choicesRelationDefinitionIdResult;
  }

  public showRelationDefinitionId(model: Account): boolean {
    return this.choicesRelationDefinitionId(model).length > 0;
  }

  public formatRelationDefinitionId(defId: number): string {
    if (!defId) {
      return '';
    }

    const def = this.ws.definitions.Relations[defId];
    return this.ws.getMultilingualValueImmediate(def, 'TitlePlural');
  }

  public onRelationDefinitionChange(defId: number, model: Account) {
    // Delete the RelationId if an incompatible definition is selected
    if (!defId) {
      return;
    }

    const relation = this.ws.get('Relation', model.RelationId) as Relation;
    if (!!relation && relation.DefinitionId !== defId) {
      delete model.RelationId;
    }
  }

  // Relation
  public labelRelation(model: AccountForSave): string {
    let postfix = '';
    if (!!model && !!model.RelationDefinitionId) {
      const relationDef = this.ws.definitions.Relations[model.RelationDefinitionId];
      if (!!relationDef) {
        postfix = ` (${this.ws.getMultilingualValueImmediate(relationDef, 'TitleSingular')})`;
      }
    }
    return this.translate.instant('Account_Relation') + postfix;
  }

  public showRelation(model: AccountForSave): boolean {
    return this.showRelationDefinitionId(model) && !!model.RelationDefinitionId;
  }

  public definitionIdsRelation(model: AccountForSave): number[] {
    if (!!model && !!model.RelationDefinitionId) {
      return [model.RelationDefinitionId];
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

  // NotedRelation Definition
  private _choicesNotedRelationDefinitionIdDefinitions: DefinitionsForClient;
  private _choicesNotedRelationDefinitionIdAccountType: AccountType;
  private _choicesNotedRelationDefinitionIdResult: SelectorChoice[] = [];
  public choicesNotedRelationDefinitionId(model: Account): SelectorChoice[] {

    const ws = this.ws;
    const defs = ws.definitions;
    const at = this.accountType(model);
    if (this._choicesNotedRelationDefinitionIdAccountType !== at ||
      this._choicesNotedRelationDefinitionIdDefinitions !== defs) {
      this._choicesNotedRelationDefinitionIdAccountType = at;
      this._choicesNotedRelationDefinitionIdDefinitions = defs;

      if (!at || !at.NotedRelationDefinitions) {
        this._choicesNotedRelationDefinitionIdResult = [];
      } else {
        this._choicesNotedRelationDefinitionIdResult = at.NotedRelationDefinitions
          .filter(d => !!defs.Relations[d.NotedRelationDefinitionId])
          .map(d =>
          ({
            value: d.NotedRelationDefinitionId,
            name: () => ws.getMultilingualValueImmediate(defs.Relations[d.NotedRelationDefinitionId], 'TitleSingular')
          }));
      }
    }

    return this._choicesNotedRelationDefinitionIdResult;
  }

  public showNotedRelationDefinitionId(model: Account): boolean {
    return this.choicesNotedRelationDefinitionId(model).length > 0;
  }

  public formatNotedRelationDefinitionId(defId: number): string {
    if (!defId) {
      return '';
    }

    const def = this.ws.definitions.Relations[defId];
    return this.ws.getMultilingualValueImmediate(def, 'TitlePlural');
  }

  public onNotedRelationDefinitionChange(defId: number, model: Account) {
    // Delete the NotedRelationId if an incompatible definition is selected
    if (!defId) {
      return;
    }

    const notedrelation = this.ws.get('Relation', model.NotedRelationId) as Relation;
    if (!!notedrelation && notedrelation.DefinitionId !== defId) {
      delete model.NotedRelationId;
    }
  }

  // NotedRelation
  public labelNotedRelation(model: AccountForSave): string {
    let postfix = '';
    if (!!model && !!model.NotedRelationDefinitionId) {
      const notedrelationDef = this.ws.definitions.Relations[model.NotedRelationDefinitionId];
      if (!!notedrelationDef) {
        postfix = ` (${this.ws.getMultilingualValueImmediate(notedrelationDef, 'TitleSingular')})`;
      }
    }
    return this.translate.instant('Account_NotedRelation') + postfix;
  }

  public showNotedRelation(model: AccountForSave): boolean {
    return this.showNotedRelationDefinitionId(model) && !!model.NotedRelationDefinitionId;
  }

  public definitionIdsNotedRelation(model: AccountForSave): number[] {
    if (!!model && !!model.NotedRelationDefinitionId) {
      return [model.NotedRelationDefinitionId];
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
    const defaultSelect = `RelationDefinitions.RelationDefinitionId,ResourceDefinitions.ResourceDefinitionId,
    NotedRelationDefinitions.NotedRelationDefinitionId,EntryTypeParentId`;

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

  public get relationAdditionalSelect(): string {
    const defaultSelect = `DefinitionId,Currency.Name,Currency.Name2,Currency.Name3,Center.Name,Center.Name2,Center.Name3`;
    if (this.additionalSelect === '$DocumentDetails') {
      // Popup from document screen, get everything the document screen needs
      return '$DocumentDetails,' + defaultSelect;
    } else {
      // Just the account screen, get what the account screen needs
      return defaultSelect;
    }
  }

  public get notedRelationAdditionalSelect(): string {
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
