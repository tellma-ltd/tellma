import { Component, Input } from '@angular/core';
import { Subject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { Role, RoleForSave } from '~/app/data/dto/role';
import { Permission, Permission_Level } from '~/app/data/dto/permission';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'b-roles-details',
  templateUrl: './roles-details.component.html',
  styleUrls: ['./roles-details.component.scss']
})
export class RolesDetailsComponent extends DetailsBaseComponent {

  @Input()
  public showMembers = true;

  @Input()
  public showPermissions = true;

  @Input()
  public showIsPublic = true;

  private _permissionLevelChoices: { [allowedLevels: string]: { name: string, value: any }[] } = {};
  private notifyDestruct$ = new Subject<void>();
  private rolesApi = this.api.rolesApi(this.notifyDestruct$); // for intellisense

  public expand = 'Permissions/View,Signatures/View,Members/User';

  create = () => {
    const result = new RoleForSave();
    if (this.ws.isPrimaryLanguage) {
      result.Name = this.initialText;
    } else {
      result.Name2 = this.initialText;
    }
    result.IsPublic = false;
    result.Permissions = [];
    result.Signatures = [];
    result.Members = [];
    return result;
  }

  clone: (item: Role) => Role = (item: Role) => {
    if (!!item) {
      const clone = <Role>JSON.parse(JSON.stringify(item));
      clone.Id = null;
      clone.EntityState = 'Inserted';

      if (!!clone.Permissions) {
        clone.Permissions.forEach(e => {
          e.Id = null;
          e.EntityState = 'Inserted';
        });
      }
      if (!!clone.Signatures) {
        clone.Signatures.forEach(e => {
          e.Id = null;
          e.EntityState = 'Inserted';
        });
      }
      if (!!clone.Members) {
        clone.Members.forEach(e => {
          e.Id = null;
          e.EntityState = 'Inserted';
        });
      }

      return clone;
    } else {
      // programmer mistake
      console.error('Cloning a non existing item');
      return null;
    }
  }

  constructor(public workspace: WorkspaceService, private api: ApiService, private translate: TranslateService) {
    super();

    this.rolesApi = this.api.rolesApi(this.notifyDestruct$);
  }

  permissionLevelChoices(item: Permission): { name: string, value: any }[] {
    // Returns the permission levels only permitted by the specified view
    const view = this.ws.get('View', item.ViewId);
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
      this.rolesApi.activate([model.Id], { returnEntities: true, expand: this.expand }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe(null, this.details.handleActionError);
    }
  }

  public onDeactivate = (model: Role): void => {
    if (!!model && !!model.Id) {
      this.rolesApi.deactivate([model.Id], { returnEntities: true, expand: this.expand }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe(null, this.details.handleActionError);
    }
  }

  public showActivate = (model: Role) => !!model && !model.IsActive;
  public showDeactivate = (model: Role) => !!model && model.IsActive;

  public canActivateDeactivateItem = (model: Role) => this.ws.canUpdate('roles', model.Id);

  public activateDeactivateTooltip = (model: Role) => this.canActivateDeactivateItem(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  public get ws() {
    return this.workspace.current;
  }

  permissionsCount(model: Role): number | string {
    return !!model && !!model.Permissions ? model.Permissions.length : 0;
  }

  signaturesCount(model: Role): number | string {
    return !!model && !!model.Signatures ? model.Signatures.length : 0;
  }

  membersCount(model: Role): number | string {
    return !!model && !!model.Members ? model.Members.filter(e => e.EntityState !== 'Deleted').length : 0;
  }

  showMembersTab(model: Role) {
    return this.showMembers && (!model || !model.IsPublic);
  }

  showPublicRoleWarning(model: Role) {
    return !model || model.IsPublic;
  }

  showPermissionsError(model: Role) {
    return !!model && !!model.Permissions && model.Permissions.some(e => !!e.serverErrors);
  }

  showSignaturesError(model: Role) {
    return !!model && !!model.Signatures && model.Signatures.some(e => !!e.serverErrors);
  }

  showMembersError(model: Role) {
    return !!model && !!model.Members && model.Members.some(e => !!e.serverErrors);
  }

  viewFormatter: (id: number | string) => string = (id: number | string) =>
    !!this.ws.get('View', id) && !!this.ws.get('View', id).ResourceName ?
      (this.translate.instant(this.ws.get('View', id).ResourceName)) :
      this.ws.getMultilingualValue('View', id, 'Name')
}
