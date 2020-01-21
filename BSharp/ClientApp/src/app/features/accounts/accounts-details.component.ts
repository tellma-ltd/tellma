import { Component } from '@angular/core';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { addToWorkspace } from '~/app/data/util';
import { tap } from 'rxjs/operators';
import { WorkspaceService } from '~/app/data/workspace.service';
import { ApiService } from '~/app/data/api.service';
import { TranslateService } from '@ngx-translate/core';
import { AccountForSave, Account, metadata_Account } from '~/app/data/entities/account';
import { PropDescriptor, getChoices, ChoicePropDescriptor } from '~/app/data/entities/base/metadata';
import { SelectorChoice } from '~/app/shared/selector/selector.component';
import { Resource } from '~/app/data/entities/resource';
import { Agent } from '~/app/data/entities/agent';

@Component({
  selector: 'b-accounts-details',
  templateUrl: './accounts-details.component.html',
  styles: []
})
export class AccountsDetailsComponent extends DetailsBaseComponent {

  private accountsApi = this.api.accountsApi(this.notifyDestruct$); // for intellisense

  public expand = `AccountType,AccountClassification,Currency,ResponsibilityCenter,
  ResourceClassification,Agent,Resource/ResourceClassification,Resource/Currency,EntryClassification,
  Resource/CountUnit,Resource/MassUnit,Resource/VolumeUnit,Resource/TimeUnit`;

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
    const result: AccountForSave = { };
    if (this.ws.isPrimaryLanguage) {
      result.Name = this.initialText;
    } else if (this.ws.isSecondaryLanguage) {
      result.Name2 = this.initialText;
    } else if (this.ws.isTernaryLanguage) {
      result.Name3 = this.initialText;
    }

    result.IsSmart = false;

    return result;
  }

  public get ws() {
    return this.workspace.current;
  }

  public get p(): { [prop: string]: PropDescriptor } {
    return metadata_Account(this.ws, this.translate, null).properties;
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


  public get contractTypeChoices(): SelectorChoice[] {
    const meta = metadata_Account(this.ws, this.translate, null).properties.ContractType as ChoicePropDescriptor;
    return getChoices(meta);
  }

  public resourceCurrencyId(model: Account) {
    if (!model) {
      return null;
    }

    const resource = this.ws.get('Resource', model.ResourceId) as Resource;
    return !!resource ? resource.CurrencyId : null;
  }

  public CurrencyId(model: Account) {
    if (!model) {
      return null;
    }

    return this.resourceCurrencyId(model) || model.CurrencyId;
  }

  public AgentDefinitionId(model: Account) {
    if (!model) {
      return null;
    }

    if (!!model.AgentId) {
      return (this.ws.get('Agent', model.AgentId) as Agent).DefinitionId;
    }
  }

  public ResourceClassificationId(model: Account) {
    if (!model) {
      return null;
    }

    if (!!model.ResourceId) {
      return (this.ws.get('Resource', model.ResourceId) as Resource).AccountTypeId;
    }

    return model.ResourceClassificationId;
  }
}
