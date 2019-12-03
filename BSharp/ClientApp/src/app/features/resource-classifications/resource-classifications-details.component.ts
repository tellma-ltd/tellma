import { Component, Input, OnInit } from '@angular/core';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import {
  ResourceClassificationForSave, ResourceClassification, metadata_ResourceClassification
} from '~/app/data/entities/resource-classification';
import { ApiService } from '~/app/data/api.service';
import { WorkspaceService } from '~/app/data/workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { tap } from 'rxjs/operators';
import { addToWorkspace } from '~/app/data/util';
import { Router, ActivatedRoute, ParamMap } from '@angular/router';

@Component({
  selector: 'b-resource-classifications-details',
  templateUrl: './resource-classifications-details.component.html'
})
export class ResourceClassificationsDetailsComponent extends DetailsBaseComponent {

  private resourceClassificationsApi = this.api.resourceClassificationsApi(this.notifyDestruct$); // for intellisense

  public expand = 'Parent';

  constructor(
    private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();

    this.resourceClassificationsApi = this.api.resourceClassificationsApi(this.notifyDestruct$);
  }

  get viewId(): string {
    return `resource-classifications`;
  }

  create = () => {
    const result = new ResourceClassificationForSave();
    if (this.ws.isPrimaryLanguage) {
      result.Name = this.initialText;
    } else if (this.ws.isSecondaryLanguage) {
      result.Name2 = this.initialText;
    } else if (this.ws.isTernaryLanguage) {
      result.Name3 = this.initialText;
    }

    result.IsAssignable = true;
    return result;
  }

  public get ws() {
    return this.workspace.current;
  }

  public onActivate = (model: ResourceClassification): void => {
    if (!!model && !!model.Id) {
      this.resourceClassificationsApi.activate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onDeactivate = (model: ResourceClassification): void => {
    if (!!model && !!model.Id) {
      this.resourceClassificationsApi.deactivate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public showActivate = (model: ResourceClassification) => !!model && !model.IsActive;
  public showDeactivate = (model: ResourceClassification) => !!model && model.IsActive;

  public canActivateDeactivateItem = (model: ResourceClassification) => this.ws.canDo(this.viewId, 'IsActive', model.Id);

  public activateDeactivateTooltip = (model: ResourceClassification) => this.canActivateDeactivateItem(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  public get masterCrumb(): string {
    const entityDesc = metadata_ResourceClassification(this.ws, this.translate, null);
    return !!entityDesc ? entityDesc.titlePlural() : '???';
  }
}
