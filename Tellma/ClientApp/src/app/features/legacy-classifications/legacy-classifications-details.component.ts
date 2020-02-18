import { Component } from '@angular/core';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { TranslateService } from '@ngx-translate/core';
import { LegacyClassificationForSave, LegacyClassification } from '~/app/data/entities/legacy-classification';

@Component({
  selector: 't-legacy-classifications-details',
  templateUrl: './legacy-classifications-details.component.html',
  styles: []
})
export class LegacyClassificationsDetailsComponent extends DetailsBaseComponent {

  private legacyClassificationsApi = this.api.legacyClassificationsApi(this.notifyDestruct$); // for intellisense

  public expand = 'Parent';

  create = () => {
    const result: LegacyClassificationForSave = {};
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

    this.legacyClassificationsApi = this.api.legacyClassificationsApi(this.notifyDestruct$);
  }

  public get ws() {
    return this.workspace.currentTenant;
  }

  public onActivate = (model: LegacyClassification): void => {
    if (!!model && !!model.Id) {
      this.legacyClassificationsApi.activate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onDeprecate = (model: LegacyClassification): void => {
    if (!!model && !!model.Id) {
      this.legacyClassificationsApi.deactivate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public showActivate = (model: LegacyClassification) => !!model && model.IsDeprecated;
  public showDeprecate = (model: LegacyClassification) => !!model && !model.IsDeprecated;

  public canActivateDeprecateItem = (model: LegacyClassification) => this.ws.canDo('legacy-classifications', 'IsDeprecated', model.Id);

  public activateDeprecateTooltip = (model: LegacyClassification) => this.canActivateDeprecateItem(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')
}
