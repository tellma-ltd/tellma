import { Component } from '@angular/core';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { TranslateService } from '@ngx-translate/core';
import { AccountClassificationForSave, AccountClassification } from '~/app/data/entities/account-classification';

@Component({
  selector: 't-account-classifications-details',
  templateUrl: './account-classifications-details.component.html',
  styles: []
})
export class AccountClassificationsDetailsComponent extends DetailsBaseComponent {

  private accountClassificationsApi = this.api.accountClassificationsApi(this.notifyDestruct$); // for intellisense

  public expand = 'Parent';

  create = () => {
    const result: AccountClassificationForSave = {};
    if (this.ws.isPrimaryLanguage) {
      result.Name = this.initialText;
    } else if (this.ws.isSecondaryLanguage) {
      result.Name2 = this.initialText;
    } else if (this.ws.isTernaryLanguage) {
      result.Name3 = this.initialText;
    }

    return result;
  }

  constructor(
    private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();

    this.accountClassificationsApi = this.api.accountClassificationsApi(this.notifyDestruct$);
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  public onActivate = (model: AccountClassification): void => {
    if (!!model && !!model.Id) {
      this.accountClassificationsApi.activate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onDeactivate = (model: AccountClassification): void => {
    if (!!model && !!model.Id) {
      this.accountClassificationsApi.deactivate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public showActivate = (model: AccountClassification) => !!model && !model.IsActive;
  public showDeactivate = (model: AccountClassification) => !!model && model.IsActive;

  public canActivateDeactivateItem = (model: AccountClassification) => this.ws.canDo('account-classifications', 'IsActive', model.Id);

  public activateDeactivateTooltip = (model: AccountClassification) => this.canActivateDeactivateItem(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')
}
