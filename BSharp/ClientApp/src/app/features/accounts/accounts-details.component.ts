import { Component } from '@angular/core';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { addToWorkspace } from '~/app/data/util';
import { tap } from 'rxjs/operators';
import { WorkspaceService } from '~/app/data/workspace.service';
import { ApiService } from '~/app/data/api.service';
import { TranslateService } from '@ngx-translate/core';
import { AccountForSave, Account, metadata_Account } from '~/app/data/entities/account';
import { PropDescriptor } from '~/app/data/entities/base/metadata';

@Component({
  selector: 'b-accounts-details',
  templateUrl: './accounts-details.component.html',
  styles: []
})
export class AccountsDetailsComponent extends DetailsBaseComponent {

  private accountsApi = this.api.accountsApi(this.notifyDestruct$); // for intellisense

  public expand = `AccountType,AccountClassification,Currency`;

  constructor(
    private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();

    this.accountsApi = this.api.accountsApi(this.notifyDestruct$);
  }

  private get viewId(): string {
    return `accounts`;
  }

  // UI Binding

  create = () => {
    const result = new AccountForSave();
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

  public canActivateDeprecateItem = (model: Account) => this.ws.canDo(this.viewId, 'IsDeprecated', model.Id);

  public activateDeprecateTooltip = (model: Account) => this.canActivateDeprecateItem(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

}
