import { Component } from '@angular/core';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { EntryTypeForSave, EntryType, metadata_EntryType } from '~/app/data/entities/entry-type';
import { ApiService } from '~/app/data/api.service';
import { WorkspaceService } from '~/app/data/workspace.service';
import { TranslateService } from '@ngx-translate/core';
import { tap } from 'rxjs/operators';
import { addToWorkspace } from '~/app/data/util';

@Component({
  selector: 'b-entry-types-details',
  templateUrl: './entry-types-details.component.html'
})
export class EntryTypesDetailsComponent extends DetailsBaseComponent {

  private entryTypesApi = this.api.entryTypesApi(this.notifyDestruct$); // for intellisense

  public expand = 'Parent';

  constructor(
    private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();

    this.entryTypesApi = this.api.entryTypesApi(this.notifyDestruct$);
  }

  get view(): string {
    return `entry-types`;
  }

  create = () => {
    const result: EntryTypeForSave = { };
    if (this.ws.isPrimaryLanguage) {
      result.Name = this.initialText;
    } else if (this.ws.isSecondaryLanguage) {
      result.Name2 = this.initialText;
    } else if (this.ws.isTernaryLanguage) {
      result.Name3 = this.initialText;
    }

    result.IsAssignable = true;
    result.ForDebit = true;
    result.ForCredit = true;
    return result;
  }

  public get ws() {
    return this.workspace.current;
  }

  public onActivate = (model: EntryType): void => {
    if (!!model && !!model.Id) {
      this.entryTypesApi.activate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onDeactivate = (model: EntryType): void => {
    if (!!model && !!model.Id) {
      this.entryTypesApi.deactivate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public showActivate = (model: EntryType) => !!model && !model.IsActive;
  public showDeactivate = (model: EntryType) => !!model && model.IsActive;

  public canActivateDeactivateItem = (model: EntryType) => this.ws.canDo(this.view, 'IsActive', model.Id);

  public activateDeactivateTooltip = (model: EntryType) => this.canActivateDeactivateItem(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  public get masterCrumb(): string {
    const entityDesc = metadata_EntryType(this.ws, this.translate, null);
    return !!entityDesc ? entityDesc.titlePlural() : '???';
  }
}
