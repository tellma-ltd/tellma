import { Component } from '@angular/core';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';

@Component({
  selector: 'b-users-master',
  templateUrl: './users-master.component.html',
  styleUrls: ['./users-master.component.scss']
})
export class UsersMasterComponent extends MasterBaseComponent {

  private usersApi = this.api.usersApi(this.notifyDestruct$); // for intellisense

  constructor(private workspace: WorkspaceService, private api: ApiService) {
    super();
    this.usersApi = this.api.usersApi(this.notifyDestruct$);
  }

  public get c() {
    return this.workspace.current.User;
  }

  public get ws() {
    return this.workspace.current;
  }

  public onActivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.usersApi.activate(ids, { returnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public onDeactivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.usersApi.deactivate(ids, { returnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public canActivateOrDeactivate = () => {
    return this.workspace.current.canDo('measurement-units', 'IsActive', null);
  }
}
