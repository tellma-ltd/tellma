import { Component } from '@angular/core';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { AccountTypeForSave, AccountType, metadata_AccountType } from '~/app/data/entities/account-type';
import { ApiService } from '~/app/data/api.service';
import { TranslateService } from '@ngx-translate/core';
import { tap } from 'rxjs/operators';
import { addToWorkspace } from '~/app/data/util';
import { SelectorChoice } from '~/app/shared/selector/selector.component';
import { ChoicePropDescriptor, getChoices, metadata } from '~/app/data/entities/base/metadata';

@Component({
  selector: 't-account-types-details',
  templateUrl: './account-types-details.component.html',
  styles: []
})
export class AccountTypesDetailsComponent extends DetailsBaseComponent {

  private accountTypesApi = this.api.accountTypesApi(this.notifyDestruct$); // for intellisense

  private _choicesRequiredAssignment: SelectorChoice[];
  private _choicesOptionalAssignment: SelectorChoice[];
  private _choicesEntryAssignment: SelectorChoice[];

  public expand = 'Parent,IfrsConcept,EntryTypeParent';

  constructor(
    private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();

    this.accountTypesApi = this.api.accountTypesApi(this.notifyDestruct$);
    this._choicesRequiredAssignment = ['A', 'E'].map(e => ({ value: e, name: () => translate.instant('Assignment_' + e) }));
    this._choicesOptionalAssignment = ['N', 'A', 'E'].map(e => ({ value: e, name: () => translate.instant('Assignment_' + e) }));
    this._choicesEntryAssignment = ['N', 'E'].map(e => ({ value: e, name: () => translate.instant('Assignment_' + e) }));
  }

  get view(): string {
    return `account-types`;
  }

  create = () => {
    const result: AccountTypeForSave = {};
    if (this.ws.isPrimaryLanguage) {
      result.Name = this.initialText;
    } else if (this.ws.isSecondaryLanguage) {
      result.Name2 = this.initialText;
    } else if (this.ws.isTernaryLanguage) {
      result.Name3 = this.initialText;
    }

    result.IsAssignable = true;
    result.AgentAssignment = 'N';
    result.CenterAssignment = 'A';
    result.CurrencyAssignment = 'A';
    result.EntryTypeAssignment = 'N';
    result.IdentifierAssignment = 'N';
    result.NotedAgentAssignment = 'N';
    result.ResourceAssignment = 'N';

    return result;
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  public onActivate = (model: AccountType): void => {
    if (!!model && !!model.Id) {
      this.accountTypesApi.activate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onDeactivate = (model: AccountType): void => {
    if (!!model && !!model.Id) {
      this.accountTypesApi.deactivate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public showActivate = (model: AccountType) => !!model && !model.IsActive;
  public showDeactivate = (model: AccountType) => !!model && model.IsActive;

  public canActivateDeactivateItem = (model: AccountType) => this.ws.canDo(this.view, 'IsActive', model.Id);

  public activateDeactivateTooltip = (model: AccountType) => this.canActivateDeactivateItem(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  public get masterCrumb(): string {
    const entityDesc = metadata_AccountType(this.workspace, this.translate, null);
    return !!entityDesc ? entityDesc.titlePlural() : '???';
  }

  // Assignment

  public get choicesRequiredAssignment(): SelectorChoice[] {
    return this._choicesRequiredAssignment;
  }

  public get choicesOptionalAssignment(): SelectorChoice[] {
    return this._choicesOptionalAssignment;
  }

  public get choicesEntryAssignment(): SelectorChoice[] {
    return this._choicesEntryAssignment;
  }

  public formatAssignment(assignment: string): string {
    return this.translate.instant('Assignment_' + assignment);
  }

  // Agent Definition

  public get choicesAgentDefinitionId(): SelectorChoice[] {
    const entityDesc = metadata.AccountType(this.workspace, this.translate, null);
    return getChoices(entityDesc.properties.AgentDefinitionId as ChoicePropDescriptor);
  }

  public showAgentDefinitionId(model: AccountType): boolean {
    return !!model && !!model.AgentAssignment && model.AgentAssignment !== 'N';
  }

  public formatAgentDefinitionId(defId: string): string {
    if (!defId) {
      return '';
    }

    const def = this.ws.definitions.Agents[defId];
    return this.ws.getMultilingualValueImmediate(def, 'TitlePlural');
  }

  // Noted Agent Definition

  public get choicesNotedAgentDefinitionId(): SelectorChoice[] {
    const entityDesc = metadata.AccountType(this.workspace, this.translate, null);
    return getChoices(entityDesc.properties.NotedAgentDefinitionId as ChoicePropDescriptor);
  }

  public showNotedAgentDefinitionId(model: AccountType): boolean {
    return !!model && !!model.NotedAgentAssignment && model.NotedAgentAssignment !== 'N';
  }

  public formatNotedAgentDefinitionId(defId: string): string {
    return this.formatAgentDefinitionId(defId);
  }

  // Resource Definition

  public get choicesResourceDefinitionId(): SelectorChoice[] {
    const entityDesc = metadata.AccountType(this.workspace, this.translate, null);
    return getChoices(entityDesc.properties.ResourceDefinitionId as ChoicePropDescriptor);
  }

  public showResourceDefinitionId(model: AccountType): boolean {
    return !!model && !!model.ResourceAssignment && model.ResourceAssignment !== 'N';
  }

  public formatResourceDefinitionId(defId: string): string {
    if (!defId) {
      return '';
    }

    const def = this.ws.definitions.Resources[defId];
    return this.ws.getMultilingualValueImmediate(def, 'TitlePlural');
  }

  // Entry Type Parent
  public showEntryTypeParent(model: AccountType): boolean {
    return !!model && !!model.EntryTypeAssignment && model.EntryTypeAssignment !== 'N';
  }

  // Is Inactive
  isInactive: (model: AccountType) => string = (at: AccountType) =>
    // !!at && at.IsSystem ? 'Error_CannotModifySystemItem' :
    !!at && !at.IsActive ? 'Error_CannotModifyInactiveItemPleaseActivate' : null
}
