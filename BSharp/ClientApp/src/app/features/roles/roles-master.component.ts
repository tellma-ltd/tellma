import { Component } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';

@Component({
  selector: 'b-roles-master',
  templateUrl: './roles-master.component.html'
})
export class RolesMasterComponent extends MasterBaseComponent {

  private rolesApi = this.api.rolesApi(this.notifyDestruct$); // for intellisense

  public expand = '';

  constructor(private workspace: WorkspaceService, private api: ApiService) {
    super();
    this.rolesApi = this.api.rolesApi(this.notifyDestruct$);
  }

  public get c() {
    return this.workspace.current.Role;
  }

  public get ws() {
    return this.workspace.current;
  }

  public onActivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.rolesApi.activate(ids, { returnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public onDeactivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.rolesApi.deactivate(ids, { returnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }
}
