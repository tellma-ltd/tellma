import { Component } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'b-report-definitions-master',
  templateUrl: './report-definitions-master.component.html',
  styles: []
})
export class ReportDefinitionsMasterComponent extends MasterBaseComponent {

  private reportDefinitionsApi = this.api.reportDefinitionsApi(this.notifyDestruct$); // for intellisense

  public expand = '';

  constructor(private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();
    this.reportDefinitionsApi = this.api.reportDefinitionsApi(this.notifyDestruct$);
  }

  public get c() {
    return this.workspace.current.ReportDefinition;
  }

  public get ws() {
    return this.workspace.current;
  }

  public onEdit = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.reportDefinitionsApi.updateState(ids, { returnEntities: true, expand: this.expand, state: 'Draft' }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public onDeploy = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.reportDefinitionsApi.updateState(ids, { returnEntities: true, expand: this.expand, state: 'Deployed' }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public onArchive = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.reportDefinitionsApi.updateState(ids, { returnEntities: true, expand: this.expand, state: 'Archived' }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public canUpdateState = (_: (number | string)[]) => this.ws.canDo('report-definitions', 'UpdateState', null);

  public updateStateTooltip = (ids: (number | string)[]) => this.canUpdateState(ids) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')
}
