import { Component } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';

@Component({
  selector: 'b-local-users-master',
  templateUrl: './local-users-master.component.html',
  styleUrls: ['./local-users-master.component.scss']
})
export class LocalUsersMasterComponent extends MasterBaseComponent {

  private localUsersApi = this.api.localUsersApi(this.notifyDestruct$); // for intellisense

  public expand = '';

  constructor(private workspace: WorkspaceService, private api: ApiService) {
    super();
    this.localUsersApi = this.api.localUsersApi(this.notifyDestruct$);
  }

  public get c() {
    return this.workspace.current.LocalUsers;
  }

  public get ws() {
    return this.workspace.current;
  }

  public onActivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.localUsersApi.activate(ids, { returnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public onDeactivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.localUsersApi.deactivate(ids, { returnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public canActivateOrDeactivate = () => {
    return this.workspace.current.canUpdate('measurement-units', null);
  }
}
