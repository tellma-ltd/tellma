import { Component } from '@angular/core';
import { MasterBaseComponent } from '~/app/shared/master-base/master-base.component';
import { WorkspaceService, AdminWorkspace } from '~/app/data/workspace.service';
import { ApiService } from '~/app/data/api.service';
import { TranslateService } from '@ngx-translate/core';
import { Observable } from 'rxjs';
import { addToWorkspace } from '~/app/data/util';
import { tap } from 'rxjs/operators';

@Component({
  selector: 't-admin-users-master',
  templateUrl: './admin-users-master.component.html',
  styles: []
})
export class AdminUsersMasterComponent extends MasterBaseComponent {

  private adminUsersApi = this.api.adminUsersApi(this.notifyDestruct$); // for intellisense

  constructor(private workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();
    this.adminUsersApi = this.api.adminUsersApi(this.notifyDestruct$);
  }

  public get c()  {
    return this.ws.AdminUser;
  }

  public get ws(): AdminWorkspace {
    return this.workspace.admin;
  }

  public onActivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.adminUsersApi.activate(ids, { returnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public onDeactivate = (ids: (number | string)[]): Observable<any> => {
    const obs$ = this.adminUsersApi.deactivate(ids, { returnEntities: true }).pipe(
      tap(res => addToWorkspace(res, this.workspace))
    );

    // The master template handles any errors
    return obs$;
  }

  public canActivateDeactivateItem = (_: (number | string)[]) => this.ws.canDo('admin-users', 'IsActive', null);

  public activateDeactivateTooltip = (ids: (number | string)[]) => this.canActivateDeactivateItem(ids) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

}
