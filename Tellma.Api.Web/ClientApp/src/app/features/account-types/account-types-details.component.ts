import { Component } from '@angular/core';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { AccountTypeForSave, AccountType, metadata_AccountType } from '~/app/data/entities/account-type';
import { ApiService } from '~/app/data/api.service';
import { TranslateService } from '@ngx-translate/core';
import { tap } from 'rxjs/operators';
import { addToWorkspace } from '~/app/data/util';

@Component({
  selector: 't-account-types-details',
  templateUrl: './account-types-details.component.html',
  styles: []
})
export class AccountTypesDetailsComponent extends DetailsBaseComponent {

  private accountTypesApi = this.api.accountTypesApi(this.notifyDestruct$); // for intellisense

  public expand = `Parent,EntryTypeParent,
AgentDefinitions.AgentDefinition,ResourceDefinitions.ResourceDefinition,NotedAgentDefinitions.NotedAgentDefinition,NotedResourceDefinitions.NotedResourceDefinition`;

  constructor(
    private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();

    this.accountTypesApi = this.api.accountTypesApi(this.notifyDestruct$);
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
    result.StandardAndPure = false;

    result.AgentDefinitions = [];
    result.ResourceDefinitions = [];
    result.NotedAgentDefinitions = [];
    result.NotedResourceDefinitions = [];

    return result;
  }

  clone: (item: AccountType) => AccountType = (item: AccountType) => {
    if (!!item) {
      const clone = JSON.parse(JSON.stringify(item)) as AccountType;
      delete clone.Id;

      if (!!clone.AgentDefinitions) {
        clone.AgentDefinitions.forEach(e => {
          delete e.Id;
        });
      }
      if (!!clone.ResourceDefinitions) {
        clone.ResourceDefinitions.forEach(e => {
          delete e.Id;
        });
      }
      if (!!clone.NotedAgentDefinitions) {
        clone.NotedAgentDefinitions.forEach(e => {
          delete e.Id;
        });
      }
      if (!!clone.NotedResourceDefinitions) {
        clone.NotedResourceDefinitions.forEach(e => {
          delete e.Id;
        });
      }

      clone.IsSystem = false;

      return clone;
    } else {
      // programmer mistake
      console.error('Cloning a non existing item');
      return null;
    }
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
    const entityDesc = metadata_AccountType(this.workspace, this.translate);
    return !!entityDesc ? entityDesc.titlePlural() : '???';
  }

  // Entry Type Parent
  public showEntryTypeParent(_: AccountType): boolean {
    return true;
  }

  public resourceDefinitionFilter(model: AccountType): string {
    return `State ne 'Hidden'`;
  }

  public agentDefinitionFilter(model: AccountType): string {
    return `State ne 'Hidden'`;
  }

  public notedAgentDefinitionFilter(model: AccountType): string {
    return `State ne 'Hidden'`;
  }

  public notedResourceDefinitionFilter(model: AccountType): string {
    return `State ne 'Hidden'`;
  }

  // Is Inactive
  isInactive: (model: AccountType) => string = (at: AccountType) =>
    // !!at && at.IsSystem ? 'Error_CannotModifySystemItem' :
    !!at && !at.IsActive ? 'Error_CannotModifyInactiveItemPleaseActivate' : null


  public showDescriptionsError(model: AccountType): boolean {
    return !!model && !!model.serverErrors && (!!model.serverErrors.Description ||
      !!model.serverErrors.Description2 || !!model.serverErrors.Description3);
  }

  public showLabelsError(model: AccountType): boolean {
    return !!model && !!model.serverErrors &&
      Object.keys(model.serverErrors).some(e => e.endsWith('Label') || e.endsWith('Label2') || e.endsWith('Label3'));
  }

  public showAgentDefinitionsError(model: AccountType): boolean {
    return !!model && !!model.AgentDefinitions && model.AgentDefinitions.some(e => !!e.serverErrors);
  }

  public showResourceDefinitionsError(model: AccountType): boolean {
    return !!model && !!model.ResourceDefinitions && model.ResourceDefinitions.some(e => !!e.serverErrors);
  }

  public showNotedAgentDefinitionsError(model: AccountType): boolean {
    return !!model && !!model.NotedAgentDefinitions && model.NotedAgentDefinitions.some(e => !!e.serverErrors);
  }

  public showNotedResourceDefinitionsError(model: AccountType): boolean {
    return !!model && !!model.NotedResourceDefinitions && model.NotedResourceDefinitions.some(e => !!e.serverErrors);
  }
}
