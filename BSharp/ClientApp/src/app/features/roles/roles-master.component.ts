import { Component } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { DtoKeyBase } from '~/app/data/dto/dto-key-base';
import { Roles_DoNotApplyPermissions } from '~/app/data/dto/role';

@Component({
  selector: 'b-roles-master',
  templateUrl: './roles-master.component.html',
  styleUrls: ['./roles-master.component.scss']
})
export class RolesMasterComponent extends MasterBaseComponent {

  private rolesApi = this.api.rolesApi(this.notifyDestruct$); // for intellisense

  public expand = '';
  workspaceApplyFns: { [collection: string]: (stale: DtoKeyBase, fresh: DtoKeyBase) => DtoKeyBase } = {
    // This ensures that any existing permissions won't get wiped out
    Roles: Roles_DoNotApplyPermissions
  };

  constructor(private workspace: WorkspaceService, private api: ApiService) {
    super();
    this.rolesApi = this.api.rolesApi(this.notifyDestruct$);
  }

  public get c() {
    return this.workspace.current.Roles;
  }

  public get ws() {
    return this.workspace.current;
  }

  public onActivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.rolesApi.activate(ids, { returnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace, this.workspaceApplyFns))
    );

    // The master template handles any errors
    return obs$;
  }

  public onDeactivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.rolesApi.deactivate(ids, { returnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace, this.workspaceApplyFns))
    );

    // The master template handles any errors
    return obs$;
  }
}
