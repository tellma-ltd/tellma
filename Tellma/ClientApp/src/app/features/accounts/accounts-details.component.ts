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
import { Custody } from '~/app/data/entities/custody';
import { DefinitionsForClient } from '~/app/data/dto/definitions-for-client';

@Component({
  selector: 't-accounts-details',
  templateUrl: './accounts-details.component.html',
  styles: []
})
export class AccountsDetailsComponent extends DetailsBaseComponent {

  private accountsApi = this.api.accountsApi(this.notifyDestruct$); // for intellisense

  public expand = `AccountType/CustodyDefinitions,AccountType/ResourceDefinitions,Classification,
Currency,Center,Custody,Resource/Currency,Custody/Currency,Resource/Center,Custody/Center,EntryType,Participant,Custodian`;

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
    if (!model) {
      return true;
    }

    // The center becomes readonly if either the resource or the custody have a center
    const at = this.accountType(model);
    if (!at) {
      return true;
    }

    const resource = this.ws.get('Resource', model.ResourceId) as Resource;
    const custody = this.ws.get('Custody', model.CustodyId) as Custody;
    if (at.IsBusinessUnit) {
      return (!!model.ResourceId && !!resource.CenterId) || (!!model.CustodyId && !!custody.CenterId);
    } else {
      return !!model.ResourceId && !!resource.CostCenterId;
    }
  }

  public readonlyValueCenterId(model: Account): number {
    if (!model) {
      return null;
    }

    const at = this.accountType(model);
    if (!at) {
      return null;
    }

    if (at.IsBusinessUnit) {
      if (!!model.ResourceId) {
        const resource = this.ws.get('Resource', model.ResourceId) as Resource;
        if (!!resource.CenterId) {
          return resource.CenterId;
        }
      }

      if (!!model.CustodyId) {
        const custody = this.ws.get('Custody', model.CustodyId) as Custody;
        if (!!custody.CenterId) {
          return custody.CenterId;
        }
      }
    } else {
      if (!!model.ResourceId) {
        const resource = this.ws.get('Resource', model.ResourceId) as Resource;
        if (!!resource.CostCenterId) {
          return resource.CostCenterId;
        }
      }
    }

    return null;
  }

  public filterCenter(model: Account): string {
    if (!model) {
      return null;
    }

    const at = this.accountType(model);
    if (!!at && at.IsBusinessUnit) {
      return 'CenterType eq \'BusinessUnit\'';
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

    // The currency becomes readonly if either the resource or the custody have a currency selected
    const resource = this.ws.get('Resource', model.ResourceId) as Resource;
    const custody = this.ws.get('Custody', model.CustodyId) as Custody;

    return (!!model.ResourceId && !!resource.CurrencyId) || (!!model.CustodyId && !!custody.CurrencyId);
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

    if (!!model.CustodyId) {
      const custody = this.ws.get('Custody', model.CustodyId) as Custody;
      if (!!custody.CurrencyId) {
        return custody.CurrencyId;
      }
    }

    return null;
  }

  public formatRelationDefinitionId(defId: number): string {
    if (!defId) {
      return '';
    }

    const def = this.ws.definitions.Relations[defId];
    return this.ws.getMultilingualValueImmediate(def, 'TitlePlural');
  }

  // Custody Definition
  private _choicesCustodyDefinitionIdDefinitions: DefinitionsForClient;
  private _choicesCustodyDefinitionIdAccountType: AccountType;
  private _choicesCustodyDefinitionIdResult: SelectorChoice[] = [];
  public choicesCustodyDefinitionId(model: Account): SelectorChoice[] {

    const ws = this.ws;
    const defs = ws.definitions;
    const at = this.accountType(model);
    if (this._choicesCustodyDefinitionIdAccountType !== at ||
      this._choicesCustodyDefinitionIdDefinitions !== defs) {
      this._choicesCustodyDefinitionIdAccountType = at;
      this._choicesCustodyDefinitionIdDefinitions = defs;

      if (!at || !at.CustodyDefinitions) {
        this._choicesCustodyDefinitionIdResult = [];
      } else {
        this._choicesCustodyDefinitionIdResult = at.CustodyDefinitions
          .filter(d => !!defs.Custodies[d.CustodyDefinitionId])
          .map(d =>
            ({
              value: d.CustodyDefinitionId,
              name: () => ws.getMultilingualValueImmediate(defs.Custodies[d.CustodyDefinitionId], 'TitleSingular')
            }));
      }
    }

    return this._choicesCustodyDefinitionIdResult;
  }

  public showCustodyDefinitionId(model: Account): boolean {
    return this.choicesCustodyDefinitionId(model).length > 0;
  }

  public onCustodyDefinitionChange(defId: number, model: Account) {
    // Delete the CustodyId if an incompatible definition is selected
    if (!defId) {
      // Will be deleted by the server anyways
      return;
    }

    const custody = this.ws.get('Custody', model.CustodyId) as Custody;
    if (!!custody && custody.DefinitionId !== defId) {
      delete model.CustodyId;
    }
  }

  public formatCustodyDefinitionId(defId: number): string {
    if (!defId) {
      return '';
    }

    const def = this.ws.definitions.Custodies[defId];
    return this.ws.getMultilingualValueImmediate(def, 'TitlePlural');
  }

  // Custody
  public showCustody(model: AccountForSave): boolean {
    return this.showCustodyDefinitionId(model) && !!model.CustodyDefinitionId;
  }

  public labelCustody(model: AccountForSave): string {
    let postfix = '';
    if (!!model && !!model.CustodyDefinitionId) {
      const custodyDef = this.ws.definitions.Custodies[model.CustodyDefinitionId];
      if (!!custodyDef) {
        postfix = ` (${this.ws.getMultilingualValueImmediate(custodyDef, 'TitleSingular')})`;
      }
    }
    return this.translate.instant('Account_Custody') + postfix;
  }

  public definitionIdsCustody(model: AccountForSave): number[] {
    if (!!model && !!model.CustodyDefinitionId) {
      return [model.CustodyDefinitionId];
    } else {
      return [];
    }
  }

  // Custodian

  public showCustodian(model: AccountForSave): boolean {
    const at = this.accountType(model);
    return !!at && !!at.CustodianDefinitionId && !this.showCustodyDefinitionId(model);
  }

  public labelCustodian(model: AccountForSave): string {
    let postfix = '';
    const at = this.accountType(model);
    if (!!at && !!at.CustodianDefinitionId) {
      const relationDef = this.ws.definitions.Relations[at.CustodianDefinitionId];
      if (!!relationDef) {
        postfix = ` (${this.ws.getMultilingualValueImmediate(relationDef, 'TitleSingular')})`;
      }
    }
    return this.translate.instant('Account_Custodian') + postfix;
  }

  public definitionIdsCustodian(model: AccountForSave): number[] {
    const at = this.accountType(model);
    if (!!at && !!at.CustodianDefinitionId) {
      return [at.CustodianDefinitionId];
    } else {
      return [];
    }
  }

  // Participant

  public showParticipant(model: AccountForSave): boolean {
    const at = this.accountType(model);
    return !!at && !!at.ParticipantDefinitionId && !this.showCustodyDefinitionId(model);
  }

  public labelParticipant(model: AccountForSave): string {
    let postfix = '';
    const at = this.accountType(model);
    if (!!at && !!at.ParticipantDefinitionId) {
      const relationDef = this.ws.definitions.Relations[at.ParticipantDefinitionId];
      if (!!relationDef) {
        postfix = ` (${this.ws.getMultilingualValueImmediate(relationDef, 'TitleSingular')})`;
      }
    }
    return this.translate.instant('Account_Participant') + postfix;
  }

  public definitionIdsParticipant(model: AccountForSave): number[] {
    const at = this.accountType(model);
    if (!!at && !!at.ParticipantDefinitionId) {
      return [at.ParticipantDefinitionId];
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
    const defaultSelect = `CustodyDefinitions/CustodyDefinitionId,
    ResourceDefinitions/ResourceDefinitionId,EntryTypeParentId,CustodianDefinitionId,ParticipantDefinitionId`;

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
    const defaultSelect = `DefinitionId,Currency/Name,Currency/Name2,Currency/Name3,Center/Name,Center/Name2,Center/Name3,
CostCenter/Name,CostCenter/Name2,CostCenter/Name3`;
    if (this.additionalSelect === '$DocumentDetails') {
      // Popup from document screen, get everything the document screen needs
      return '$DocumentDetails,' + defaultSelect;
    } else {
      // Just the account screen, get what the account screen needs
      return defaultSelect;
    }
  }

  public custodyAdditionalSelect =
    `DefinitionId,Currency/Name,Currency/Name2,Currency/Name3,Currency/E,Center/Name,Center/Name2,Center/Name3`;

}
