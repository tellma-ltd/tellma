import { Component } from '@angular/core';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { ApiService } from '~/app/data/api.service';
import { WorkspaceService } from '~/app/data/workspace.service';
import { tap } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { addToWorkspace } from '~/app/data/util';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'b-entry-classifications-master',
  templateUrl: './entry-classifications-master.component.html'
})
export class EntryClassificationsMasterComponent extends MasterBaseComponent {

  private entryClassificationsApi = this.api.entryClassificationsApi(this.notifyDestruct$); // for intellisense

  public expand = '';

  constructor(
    private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();

    this.entryClassificationsApi = this.api.entryClassificationsApi(this.notifyDestruct$);
  }

  private get view(): string {
    return `entry-classifications`;
  }

  // UI Binding

  public get c() {
    return this.workspace.current.EntryClassification;
  }

  public get ws() {
    return this.workspace.current;
  }
  public onActivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.entryClassificationsApi.activate(ids, { returnEntities: true, expand: this.expand }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public onDeactivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.entryClassificationsApi.deactivate(ids, { returnEntities: true, expand: this.expand }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public canActivateDeactivateItem = (_: (number | string)[]) => this.ws.canDo(this.view, 'IsActive', null);

  public activateDeactivateTooltip = (ids: (number | string)[]) => this.canActivateDeactivateItem(ids) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')
}
