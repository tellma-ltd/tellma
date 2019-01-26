import { Component } from '@angular/core';
import { Subject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { LocalUser, LocalUserForSave } from '~/app/data/dto/local-user';
import { RoleMembership, RoleMembershipForSave } from '~/app/data/dto/role-membership';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';

@Component({
  selector: 'b-local-users-details',
  templateUrl: './local-users-details.component.html',
  styleUrls: ['./local-users-details.component.scss']
})
export class LocalUsersDetailsComponent extends DetailsBaseComponent {

  private notifyDestruct$ = new Subject<void>();
  private localUsersApi = this.api.localUsersApi(this.notifyDestruct$); // for intellisense

  create = () => {
    const result = new LocalUserForSave();
    result.Roles = [];
    return result;
  }

  constructor(public workspace: WorkspaceService, private api: ApiService) {
    super();

    this.localUsersApi = this.api.localUsersApi(this.notifyDestruct$);
  }

  public onActivate = (model: LocalUser): void => {
    if (!!model && !!model.Id) {
      this.localUsersApi.activate([model.Id], { ReturnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe(null, this.details.handleActionError);
    }
  }

  public onDeactivate = (model: LocalUser): void => {
    if (!!model && !!model.Id) {
      this.localUsersApi.deactivate([model.Id], { ReturnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe(null, this.details.handleActionError);
    }
  }

  public showActivate = (model: LocalUser) => !!model && !model.IsActive;
  public showDeactivate = (model: LocalUser) => !!model && model.IsActive;

  public get ws() {
    return this.workspace.current;
  }

  showRolesError(model: LocalUser) {
    return !!model && !!model.Roles &&
      Object.keys(this.details.validationErrors)
        .some(key => key.startsWith('Roles['));
  }
}
