import { Component } from '@angular/core';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { WorkspaceService } from '~/app/data/workspace.service';
import { ApiService } from '~/app/data/api.service';
import { IfrsNote, IfrsConcept_IfrsType } from '~/app/data/entities/ifrs-note';
import { addToWorkspace } from '~/app/data/util';
import { tap } from 'rxjs/operators';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 't-ifrs-notes-details',
  templateUrl: './ifrs-notes-details.component.html'
})
export class IfrsNotesDetailsComponent extends DetailsBaseComponent {

  private ifrsNotesApi = this.api.ifrsNotesApi(this.notifyDestruct$); // for intellisense

  public expand = 'Parent';

  constructor(private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();

    this.ifrsNotesApi = this.api.ifrsNotesApi(this.notifyDestruct$);
  }

  public ifrsTypeLookup(value: string): string {
    return IfrsConcept_IfrsType[value];
  }

  public get ws() {
    return this.workspace.current;
  }

  public onActivate = (model: IfrsNote): void => {
    if (!!model && !!model.Id) {
      this.ifrsNotesApi.activate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onDeactivate = (model: IfrsNote): void => {
    if (!!model && !!model.Id) {
      this.ifrsNotesApi.deactivate([model.Id], { returnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public showActivate = (model: IfrsNote) => !!model && !model.IsActive;
  public showDeactivate = (model: IfrsNote) => !!model && model.IsActive;

  public canActivateDeactivateItem = (model: IfrsNote) => this.ws.canDo('ifrs-notes', 'IsActive', model.Id);

  public activateDeactivateTooltip = (model: IfrsNote) => this.canActivateDeactivateItem(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')
}
