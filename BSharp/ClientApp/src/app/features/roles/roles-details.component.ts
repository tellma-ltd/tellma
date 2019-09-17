import { Component, Input } from '@angular/core';
import { tap } from 'rxjs/operators';
import { ApiService } from '~/app/data/api.service';
import { Role, RoleForSave } from '~/app/data/entities/role';
import { Permission, Permission_Level as Permission_Action } from '~/app/data/entities/permission';
import { addToWorkspace } from '~/app/data/util';
import { WorkspaceService } from '~/app/data/workspace.service';
import { DetailsBaseComponent } from '~/app/shared/details-base/details-base.component';
import { TranslateService } from '@ngx-translate/core';
import { View } from '~/app/data/entities/view';

@Component({
  selector: 'b-roles-details',
  templateUrl: './roles-details.component.html'
})
export class RolesDetailsComponent extends DetailsBaseComponent {

  @Input()
  public showMembers = true;

  @Input()
  public showPermissions = true;

  @Input()
  public showIsPublic = true;

  private _permissionActionChoices: { [viewId: string]: { name: string, value: any }[] } = {};
  private rolesApi = this.api.rolesApi(this.notifyDestruct$); // for intellisense

  public expand = 'Permissions/View/Actions,Members/Agent';

  create = () => {
    const result = new RoleForSave();
    if (this.ws.isPrimaryLanguage) {
      result.Name = this.initialText;
    } else {
      result.Name2 = this.initialText;
    }
    result.IsPublic = false;
    result.Permissions = [];
    result.Members = [];
    return result;
  }

  clone: (item: Role) => Role = (item: Role) => {
    if (!!item) {
      const clone = JSON.parse(JSON.stringify(item)) as Role;
      clone.Id = null;

      if (!!clone.Permissions) {
        clone.Permissions.forEach(e => {
          e.Id = null;
        });
      }
      if (!!clone.Members) {
        clone.Members.forEach(e => {
          e.Id = null;
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

  permissionActionChoices(item: Permission): { name: string, value: any }[] {
    if (!item.ViewId) {
      return [];
    }

    // Returns the permission actions only permitted by the specified view
    if (!this._permissionActionChoices[item.ViewId]) {
      const view = this.ws.get('View', item.ViewId) as View;
      if (!!view && !!view.Actions) {
        this._permissionActionChoices[item.ViewId] =
        view.Actions.map(e => ({ name: Permission_Action[e.Action], value: e.Action })).concat([{ value: 'All', name: 'View_All' }]);
      } else {
        this._permissionActionChoices[item.ViewId] = [];
      }
    }

    return this._permissionActionChoices[item.ViewId];
  }

  public permissionActionLookup(value: string): string {
    if (!value) {
      return '';
    }

    if (value === 'All') {
      return 'View_All';
    }

    return Permission_Action[value];
  }

  public disableCriteria(viewId: string, action: string) {
    // TODO cache this
    if (!viewId || !action) {
      return true;
    }
    const view = this.ws.get('View', viewId) as View;
    if (!!view && !!view.Actions) {
      const viewAction = view.Actions.find(e => e.Action === action);
      return !(viewAction && viewAction.SupportsCriteria);
    } else {
      return true;
    }
  }

  public disableMask(viewId: string, action: string) {
    // TODO cache this
    if (!viewId || !action) {
      return true;
    }
    const view = this.ws.get('View', viewId) as View;
    if (!!view && !!view.Actions) {
      const viewAction = view.Actions.find(e => e.Action === action);
      return !(viewAction && viewAction.SupportsMask);
    } else {
      return true;
    }
  }

  public onPermissionChanged(item: Permission) {
    // Here we clear away fields that are not compatible with other field values
    const choices = this.permissionActionChoices(item);
    if (choices.length === 1) {
      item.Action = choices[0].value;
    } else if (choices.every(e => e.value !== item.Action)) {
      item.Action = null;
    }

    if (this.disableMask(item.ViewId, item.Action)) {
      item.Mask = null;
    }

    if (this.disableCriteria(item.ViewId, item.Action)) {
      item.Criteria = null;
    }
  }

  public onActivate = (model: Role): void => {
    if (!!model && !!model.Id) {
      this.rolesApi.activate([model.Id], { returnEntities: true, expand: this.expand }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public onDeactivate = (model: Role): void => {
    if (!!model && !!model.Id) {
      this.rolesApi.deactivate([model.Id], { returnEntities: true, expand: this.expand }).pipe(
        tap(res => addToWorkspace(res, this.workspace))
      ).subscribe({ error: this.details.handleActionError });
    }
  }

  public showActivate = (model: Role) => !!model && !model.IsActive;
  public showDeactivate = (model: Role) => !!model && model.IsActive;

  public canActivateDeactivateItem = (model: Role) => this.ws.canDo('roles', 'IsActive', model.Id);

  public activateDeactivateTooltip = (model: Role) => this.canActivateDeactivateItem(model) ? '' :
    this.translate.instant('Error_AccountDoesNotHaveSufficientPermissions')

  public get ws() {
    return this.workspace.current;
  }

  permissionsCount(model: Role): number | string {
    return !!model && !!model.Permissions ? model.Permissions.length : 0;
  }

  membersCount(model: Role): number | string {
    return !!model && !!model.Members ? model.Members.length : 0;
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

  showMembersError(model: Role) {
    return !!model && !!model.Members && model.Members.some(e => !!e.serverErrors);
  }

  viewFormatter: (id: number | string) => string = (id: number | string) =>
    !!this.ws.get('View', id) && !!this.ws.get('View', id).ResourceName ?
      (this.translate.instant(this.ws.get('View', id).ResourceName)) :
      this.ws.getMultilingualValue('View', id, 'Name')
}
