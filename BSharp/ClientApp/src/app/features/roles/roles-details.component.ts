import { Component } from '@angular/core';
import { Subject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { Role, RoleForSave } from '~/app/data/dto/role';
import { Permission, PermissionForSave, Permission_Level } from '~/app/data/dto/permission';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';

@Component({
  selector: 'b-roles-details',
  templateUrl: './roles-details.component.html',
  styleUrls: ['./roles-details.component.scss']
})
export class RolesDetailsComponent extends DetailsBaseComponent {

  private _permissionLevelChoices: { [allowedLevels: string]: { name: string, value: any }[] } = {};
  private notifyDestruct$ = new Subject<void>();
  private rolesApi = this.api.rolesApi(this.notifyDestruct$); // for intellisense

  create = () => {
    const result = new RoleForSave();
    result.IsPublic = false;
    result.Permissions = [];
    return result;
  }

  constructor(public workspace: WorkspaceService, private api: ApiService) {
    super();

    this.rolesApi = this.api.rolesApi(this.notifyDestruct$);
  }

  permissionLevelChoices(item: Permission): { name: string, value: any }[] {
    // Returns the permission levels only permitted by the specified view
    const view = this.ws.get('Views', item.ViewId);
    const allowedLevels = view ? view.AllowedPermissionLevels : '';
    if (!this._permissionLevelChoices[allowedLevels]) {
      this._permissionLevelChoices[allowedLevels] = Object.keys(Permission_Level)
        .filter(key => key !== 'Sign' && allowedLevels.indexOf(key) !== -1)
        .map(key => ({ name: Permission_Level[key], value: key }));
    }

    return this._permissionLevelChoices[allowedLevels];
  }

  public permissionLevelLookup(value: string): string {
    if (!value) {
      return '';
    }

    return Permission_Level[value];
  }

  public onActivate = (model: Role): void => {
    if (!!model && !!model.Id) {
      this.rolesApi.activate([model.Id], { ReturnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe(null, this.details.handleActionError);
    }
  }

  public onDeactivate = (model: Role): void => {
    if (!!model && !!model.Id) {
      this.rolesApi.deactivate([model.Id], { ReturnEntities: true }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe(null, this.details.handleActionError);
    }
  }

  public showActivate = (model: Role) => !!model && !model.IsActive;
  public showDeactivate = (model: Role) => !!model && model.IsActive;

  public get ws() {
    return this.workspace.current;
  }

  onNewSignature(permission: PermissionForSave) {
    permission.Level = 'Sign';
    return permission;
  }

  isPermission(item: Permission) {
    return item.Level !== 'Sign';
  }

  permissionsCount(model: Role): number {
    return !!model && !!model.Permissions ? model.Permissions.filter(this.isPermission).length : 0;
  }

  isSignature(item: Permission) {
    return item.Level === 'Sign';
  }

  signaturesCount(model: Role): number {
    return !!model && !!model.Permissions ? model.Permissions.filter(this.isSignature).length : 0;
  }

  showMembersTab(model: Role) {
    return !model.IsPublic;
  }

  membersCount(model: Role): number {
    return 0; // TODO
  }

  showPublicRoleWarning(model: Role) {
    return model.IsPublic;
  }
}
