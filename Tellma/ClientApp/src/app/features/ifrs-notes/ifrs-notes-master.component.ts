import { Component } from '@angular/core';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { WorkspaceService } from '~/app/data/workspace.service';
import { ApiService } from '~/app/data/api.service';
import { IfrsConcept_IfrsType } from '~/app/data/entities/ifrs-note';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { addToWorkspace } from '~/app/data/util';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'b-ifrs-notes-master',
  templateUrl: './ifrs-notes-master.component.html'
})
export class IfrsNotesMasterComponent extends MasterBaseComponent {

  private ifrsNotesApi = this.api.ifrsNotesApi(this.notifyDestruct$); // for intellisense

  public expand = '';

  constructor(private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();
    this.ifrsNotesApi = this.api.ifrsNotesApi(this.notifyDestruct$);
  }

  public get c() {
    return this.workspace.current.IfrsNote;
  }

  public get ws() {
    return this.workspace.current;
  }

  public ifrsTypeLookup(value: string): string {
    return IfrsConcept_IfrsType[value];
  }


  public onActivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.ifrsNotesApi.activate(ids, { returnEntities: true, expand: this.expand }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public onDeactivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.ifrsNotesApi.deactivate(ids, { returnEntities: true, expand: this.expand }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public canActivateDeactivateItem = (_: (number | string)[]) => this.ws.canDo('ifrs-notes', 'IsActive', null);

  public activateDeactivateTooltip = (ids: (number | string)[]) => this.canActivateDeactivateItem(ids) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')
}
