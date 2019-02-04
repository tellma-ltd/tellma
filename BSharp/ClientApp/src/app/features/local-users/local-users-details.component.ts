import { Component } from '@angular/core';
import { Subject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { LocalUser, LocalUserForSave } from '~/app/data/dto/local-user';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { Roles_DoNotApplyPermissions } from '~/app/data/dto/role';
import { DtoKeyBase } from '~/app/data/dto/dto-key-base';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'b-local-users-details',
  templateUrl: './local-users-details.component.html',
  styleUrls: ['./local-users-details.component.scss']
})
export class LocalUsersDetailsComponent extends DetailsBaseComponent {

  private notifyDestruct$ = new Subject<void>();
  private localUsersApi = this.api.localUsersApi(this.notifyDestruct$); // for intellisense

  public expand = 'Agent,Roles/Role';
  public workspaceApplyFns: { [collection: string]: (stale: DtoKeyBase, fresh: DtoKeyBase) => DtoKeyBase } = {
    // Roles/Role This ensures that permissions won't get wiped out when the Role navigation properties are loaded
    Roles: Roles_DoNotApplyPermissions
    // Agent
  };

  create = () => {
    const result = new LocalUserForSave();
    result.Roles = [];
    return result;
  }

  constructor(public workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();

    this.localUsersApi = this.api.localUsersApi(this.notifyDestruct$);
  }

  public onActivate = (model: LocalUser): void => {
    if (!!model && !!model.Id) {
      this.localUsersApi.activate([model.Id], { returnEntities: true, expand: this.expand }).pipe(
        tap(res => addToWorkspace(res, this.workspace, this.workspaceApplyFns))
      ).subscribe(null, this.details.handleActionError);
    }
  }

  public onDeactivate = (model: LocalUser): void => {
    if (!!model && !!model.Id) {
      this.localUsersApi.deactivate([model.Id], { returnEntities: true, expand: this.expand }).pipe(
        tap(res => addToWorkspace(res, this.workspace, this.workspaceApplyFns))
      ).subscribe(null, this.details.handleActionError);
    }
  }

  public showActivate = (model: LocalUser) => !!model && !model.IsActive;
  public showDeactivate = (model: LocalUser) => !!model && model.IsActive;

  public canActivateDeactivateItem = (model: LocalUser) => this.ws.canUpdate('local-users', model.Id);

  public activateDeactivateTooltip = (model: LocalUser) => this.canActivateDeactivateItem(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  public get ws() {
    return this.workspace.current;
  }

  showRolesError(model: LocalUser) {
    return !!model && !!model.Roles &&
      Object.keys(this.details.validationErrors)
        .some(key => key.startsWith('Roles['));
  }
}
