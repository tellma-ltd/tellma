import { Component } from '@angular/core';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { TranslateService } from '@ngx-translate/core';
import { CustomClassificationForSave, CustomClassification } from '~/app/data/entities/custom-classification';

@Component({
  selector: 't-custom-classifications-details',
  templateUrl: './custom-classifications-details.component.html',
  styles: []
})
export class CustomClassificationsDetailsComponent extends DetailsBaseComponent {

  private customClassificationsApi = this.api.customClassificationsApi(this.notifyDestruct$); // for intellisense

  public expand = 'Parent';

  create = () => {
    const result: CustomClassificationForSave = {};
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

    this.customClassificationsApi = this.api.customClassificationsApi(this.notifyDestruct$);
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  public onActivate = (model: CustomClassification): void => {
    if (!!model && !!model.Id) {
      this.customClassificationsApi.activate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onDeprecate = (model: CustomClassification): void => {
    if (!!model && !!model.Id) {
      this.customClassificationsApi.deactivate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public showActivate = (model: CustomClassification) => !!model && model.IsDeprecated;
  public showDeprecate = (model: CustomClassification) => !!model && !model.IsDeprecated;

  public canActivateDeprecateItem = (model: CustomClassification) => this.ws.canDo('custom-classifications', 'IsDeprecated', model.Id);

  public activateDeprecateTooltip = (model: CustomClassification) => this.canActivateDeprecateItem(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')
}
