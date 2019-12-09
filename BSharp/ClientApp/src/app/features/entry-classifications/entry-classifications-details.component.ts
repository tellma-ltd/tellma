import { Component } from '@angular/core';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import {
  EntryClassificationForSave, EntryClassification, metadata_EntryClassification
} from '~/app/data/entities/entry-classification';
import { ApiService } from '~/app/data/api.service';
import { WorkspaceService } from '~/app/data/workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { tap } from 'rxjs/operators';
import { addToWorkspace } from '~/app/data/util';

@Component({
  selector: 'b-entry-classifications-details',
  templateUrl: './entry-classifications-details.component.html'
})
export class EntryClassificationsDetailsComponent extends DetailsBaseComponent {

  private entryClassificationsApi = this.api.entryClassificationsApi(this.notifyDestruct$); // for intellisense

  public expand = 'Parent';

  constructor(
    private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();

    this.entryClassificationsApi = this.api.entryClassificationsApi(this.notifyDestruct$);
  }

  get viewId(): string {
    return `entry-classifications`;
  }

  create = () => {
    const result = new EntryClassificationForSave();
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

  public onActivate = (model: EntryClassification): void => {
    if (!!model && !!model.Id) {
      this.entryClassificationsApi.activate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onDeactivate = (model: EntryClassification): void => {
    if (!!model && !!model.Id) {
      this.entryClassificationsApi.deactivate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public showActivate = (model: EntryClassification) => !!model && !model.IsActive;
  public showDeactivate = (model: EntryClassification) => !!model && model.IsActive;

  public canActivateDeactivateItem = (model: EntryClassification) => this.ws.canDo(this.viewId, 'IsActive', model.Id);

  public activateDeactivateTooltip = (model: EntryClassification) => this.canActivateDeactivateItem(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  public get masterCrumb(): string {
    const entityDesc = metadata_EntryClassification(this.ws, this.translate, null);
    return !!entityDesc ? entityDesc.titlePlural() : '???';
  }
}
